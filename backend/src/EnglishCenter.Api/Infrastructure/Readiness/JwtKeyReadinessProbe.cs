using System.Collections.Immutable;
using System.Security.Cryptography;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using Microsoft.IdentityModel.Tokens;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public sealed record JwtValidationMaterial(string Reference, SecurityKey Key, string Algorithm,
    DateTimeOffset ValidFrom, DateTimeOffset ValidUntil)
{
    public override string ToString() => nameof(JwtValidationMaterial);
}
public sealed record JwtReadinessMaterial(string SigningReference, SigningCredentials Signing,
    string Issuer, string Audience, ImmutableArray<JwtValidationMaterial> ValidationKeys)
{
    public override string ToString() => nameof(JwtReadinessMaterial);
}

public sealed class JwtKeyReadinessProbe : IRuntimeReadinessProbe
{
    private readonly Func<CancellationToken, ValueTask<JwtReadinessMaterial?>> resolve;
    private readonly TimeProvider clock;
    public JwtKeyReadinessProbe(TimeProvider clock, Func<CancellationToken, ValueTask<JwtReadinessMaterial?>> resolve)
    { this.clock = clock ?? throw new ArgumentNullException(nameof(clock)); this.resolve = resolve ?? throw new ArgumentNullException(nameof(resolve)); }
    public ReadinessDependency Dependency => ReadinessDependency.JwtKeys;
    public async Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (RuntimeProfileValidator.Validate(profile) != ProfileIssue.None) return ReadinessProbeStatus.NotReady;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(profile.Execution!.ReadinessMs);
        try
        {
            var pending = resolve(deadline.Token).AsTask(); ProbeWait.Observe(pending);
            var material = await pending.WaitAsync(deadline.Token);
            if (material is null) return ReadinessProbeStatus.NotReady;
            var check = Task.Run(() => CheckMaterial(profile, material, deadline.Token), deadline.Token);
            ProbeWait.Observe(check);
            var result = await check.WaitAsync(deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();
            return result ? ReadinessProbeStatus.Pass : ReadinessProbeStatus.NotReady;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception)
        { cancellationToken.ThrowIfCancellationRequested(); return ReadinessProbeStatus.NotReady; }
    }
    private bool CheckMaterial(RuntimeProfile profile, JwtReadinessMaterial material, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var auth = profile.Auth!;
        var keys = material.ValidationKeys;
        var now = clock.GetUtcNow();
        if (keys.IsDefaultOrEmpty || keys.Any(key => key is null || key.Key is null) ||
            material.Signing is null || material.Issuer != auth.Issuer || material.Audience != auth.Audience ||
            keys.Length != auth.JwtKeySecretReferences.Length ||
            keys.Select(key => key.Reference).Distinct(StringComparer.Ordinal).Count() != keys.Length ||
            !keys.Select(key => key.Reference).ToHashSet(StringComparer.Ordinal).SetEquals(auth.JwtKeySecretReferences) ||
            keys.Any(key => key.ValidFrom > now || key.ValidUntil <= now)) return false;
        var active = keys.SingleOrDefault(key => key.Reference == material.SigningReference);
        if (active is null || active.Algorithm != material.Signing.Algorithm ||
            active.ValidUntil < SessionIssuanceWindow.Normal(now).AbsoluteExpiresAt) return false;
        // Prove the active verification key, not just any key in a rotation set.
        var activeValidator = new JwtSessionValidator([active.Key], [active.Algorithm],
            material.Issuer, material.Audience, new BusinessClock(clock), int.MaxValue);
        _ = new JwtSessionValidator(keys.Select(key => key.Key), keys.Select(key => key.Algorithm),
            material.Issuer, material.Audience, new BusinessClock(clock), int.MaxValue);
        var sid = RandomNumberGenerator.GetBytes(32);
        try
        {
            var issuer = new JwtSessionIssuer(material.Signing, material.Issuer, material.Audience);
            var encoded = issuer.Issue(1, 1, sid, SessionIssuanceWindow.Normal(now));
            var proof = activeValidator.Validate(encoded);
            token.ThrowIfCancellationRequested();
            return proof is not null;
        }
        finally { CryptographicOperations.ZeroMemory(sid); }
    }
}

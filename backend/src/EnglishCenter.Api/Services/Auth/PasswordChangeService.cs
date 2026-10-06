namespace EnglishCenter.Api.Services.Auth;

using System.Text;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;
using Microsoft.AspNetCore.Identity;

public sealed class PasswordChangeInput
{
    public string CurrentPassword { get; }
    public string NewPassword { get; }
    private readonly byte[] _version;
    public byte[] CopyVersion() => _version.ToArray();
    private PasswordChangeInput(string current, string next, byte[] version)
    { CurrentPassword = current; NewPassword = next; _version = version.ToArray(); }
    public static PasswordChangeInput Parse(string json)
    {
        var body = StrictJsonObject.Parse(json, ["currentPassword", "newPassword", "expectedVersions"], ["currentPassword", "newPassword", "expectedVersions"]);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var versions = StrictJsonObject.Parse(doc.RootElement.GetProperty("expectedVersions").GetRawText(), ["account"], ["account"]);
        var current = body.ReadString("currentPassword"); var next = body.ReadString("newPassword");
        // Raw credentials are not text-normalized. Strict encoding rejects malformed UTF16.
        try { _ = new System.Text.UTF8Encoding(false, true).GetByteCount(current); }
        catch (System.Text.EncoderFallbackException) { throw ApiErrorException.Validation("currentPassword"); }
        if (current.EnumerateRunes().Count() > PasswordPolicy.MaxCodePoints) throw ApiErrorException.Validation("currentPassword");
        return new(current, next, versions.ReadParsed("account", RowVersionCodec.Decode));
    }
    public override string ToString() => nameof(PasswordChangeInput);
}
public interface IPasswordChangeRepository : ICurrentSessionRepository
{
    Task<IReadOnlyList<AuthSession>> LockAllSessionsAsync(int accountId, CancellationToken ct);
    Task SaveChangeAsync(AuditEntry audit, CancellationToken ct);
}
public interface IPasswordChangeExecutor
{
    // Return only after the mutation and mandatory audit have committed.
    Task ExecuteAsync(VerifiedSessionToken token, PasswordChangeInput input, CancellationToken ct);
}
public sealed class PasswordChangeService(IPasswordChangeRepository repository, CredentialHasher hasher, BusinessClock clock)
{
    public async Task<DateTimeOffset> ChangeAsync(VerifiedSessionToken token, PasswordChangeInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(token); ArgumentNullException.ThrowIfNull(input); ct.ThrowIfCancellationRequested();
        var account = await repository.LockAccountAsync(token.AccountId, ct) ?? throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        var sessions = await repository.LockAllSessionsAsync(account.AccountId, ct);
        var current = await new CurrentSessionService(repository, clock).ReadAsync(token, false, ct);
        if (!PasswordChangeVersionGuard.Matches(input.CopyVersion(), account.RowVersion)) throw new ApiErrorException(ApiErrorCode.CONFLICT);
        if (hasher.Verify(account.PasswordHash, input.CurrentPassword) == PasswordVerificationResult.Failed) throw ApiErrorException.Validation("currentPassword");
        var version = PasswordChangeVersionGuard.Next(account.SecurityVersion) ?? throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        string hash;
        try { hash = hasher.Hash(input.NewPassword); }
        catch (ApiErrorException ex) when (ex.Code == ApiErrorCode.VALIDATION_ERROR) { throw ApiErrorException.Validation("newPassword"); }
        if (!clock.IsBefore(current.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        var now = clock.GetUtcNow();
        if (sessions.Any(s => s.AccountId != account.AccountId || s.IssuedAt > now.UtcDateTime)) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        var revoked = sessions.Count(s => s.RevokedAt is null);
        var audit = AuditEntry.OwnPasswordChange(account.AccountId, account.MustChangePassword, account.SecurityVersion, version, revoked, Guid.NewGuid(), now);
        account.PasswordHash = hash; account.SecurityVersion = version; account.MustChangePassword = false;
        account.TempIssuedAt = null; account.TempExpiresAt = null;
        foreach (var session in sessions) if (session.RevokedAt is null) session.RevokedAt = now.UtcDateTime;
        await repository.SaveChangeAsync(audit, ct);
        if (!clock.IsBefore(current.EffectiveDeadline)) throw new ApiErrorException(ApiErrorCode.AUTHENTICATION_REQUIRED);
        return current.EffectiveDeadline;
    }
}

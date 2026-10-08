using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EnglishCenter.Api.Api.Runtime;
using EnglishCenter.Api.Common.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public sealed record LocalTlsTrust(string CertificateReference, string TrustReference, string LeafSha256)
{
    public override string ToString() => nameof(LocalTlsTrust);
}
public sealed class TransportReadinessProbe : IRuntimeReadinessProbe
{
    private readonly LocalTransportBinding? binding;
    private readonly TimeProvider clock;
    private readonly string connectionReference;
    private readonly Func<CancellationToken, ValueTask<LocalTlsTrust?>> trust;
    private readonly Func<CancellationToken, ValueTask<string?>> sql;
    private long completedTlsChecks;
    public long CompletedTlsChecks => Interlocked.Read(ref completedTlsChecks);
    public TransportReadinessProbe(LocalTransportBinding? binding, TimeProvider clock, string connectionReference,
        Func<CancellationToken, ValueTask<LocalTlsTrust?>> trust, Func<CancellationToken, ValueTask<string?>> sql)
    {
        this.binding = binding; this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
        this.connectionReference = connectionReference;
        this.trust = trust ?? throw new ArgumentNullException(nameof(trust)); this.sql = sql ?? throw new ArgumentNullException(nameof(sql));
    }
    public ReadinessDependency Dependency => ReadinessDependency.Transport;
    public async Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (RuntimeProfileValidator.Validate(profile) != ProfileIssue.None || binding is null ||
            profile.Kind is not (RuntimeProfileKind.LOCAL_DEV or RuntimeProfileKind.LOCAL_DEMO) ||
            profile.Web != binding.Web || profile.Web!.ForwardingEnabled != false ||
            string.IsNullOrWhiteSpace(connectionReference) || profile.Database!.ConnectionSecretReference != connectionReference)
            return ReadinessProbeStatus.NotReady;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(profile.Execution!.ReadinessMs);
        try
        {
            if (!SafeFrameworkOptions()) return ReadinessProbeStatus.NotReady;
            var pendingTrust = trust(deadline.Token).AsTask(); ProbeWait.Observe(pendingTrust);
            var approved = await pendingTrust.WaitAsync(deadline.Token);
            if (approved is null || approved.CertificateReference != profile.Web.CertificateSecretReference ||
                approved.TrustReference != profile.Web.ClientTrustReference || approved.LeafSha256 is null ||
                approved.LeafSha256.Length != 64 || !approved.LeafSha256.All(char.IsAsciiHexDigit)) return ReadinessProbeStatus.NotReady;
            var origin = new Uri(profile.Web.Origin!);
            using var socket = new TcpClient();
            await socket.ConnectAsync(IPAddress.Parse(profile.Web.ListenAddress!), profile.Web.ListenPort, deadline.Token);
            using var stream = new SslStream(socket.GetStream(), leaveInnerStreamOpen: false, (_, certificate, _, _) =>
                IsApprovedCertificate(certificate, approved, origin.IdnHost));
            await stream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
            {
                TargetHost = origin.IdnHost, EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                AllowRenegotiation = false, CertificateRevocationCheckMode = X509RevocationMode.NoCheck
            }, deadline.Token);
            Interlocked.Increment(ref completedTlsChecks);
            var pendingSql = sql(deadline.Token).AsTask(); ProbeWait.Observe(pendingSql);
            var resolved = await pendingSql.WaitAsync(deadline.Token);
            if (string.IsNullOrWhiteSpace(resolved)) return ReadinessProbeStatus.NotReady;
            var settings = LocalSqlReadinessConnection.Prepare(resolved, profile.Execution, requireCertificateValidation: true);
            if (settings is null) return ReadinessProbeStatus.NotReady;
            await using var connection = new SqlConnection(settings.ConnectionString);
            await connection.OpenAsync(deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();
            return ReadinessProbeStatus.Pass;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception)
        { cancellationToken.ThrowIfCancellationRequested(); return ReadinessProbeStatus.NotReady; }
    }
    private bool SafeFrameworkOptions()
    {
        var options = binding!.Services.GetService<IOptions<AntiforgeryOptions>>()?.Value;
        var cookie = options?.Cookie;
        var auth = SafeCookiePolicy.AuthOptions();
        return options?.HeaderName == "X-CSRF-TOKEN" && cookie is not null &&
            cookie.Name == SafeCookiePolicy.CsrfCookieName && cookie.HttpOnly &&
            cookie.SecurePolicy == CookieSecurePolicy.Always && cookie.SameSite == SameSiteMode.Strict &&
            cookie.Path == "/" && cookie.Domain is null && auth.HttpOnly && auth.Secure &&
            auth.SameSite == SameSiteMode.Strict && auth.Path == "/" && auth.Domain is null;
    }
    private bool IsApprovedCertificate(X509Certificate? supplied, LocalTlsTrust approved, string host)
    {
        if (supplied is null) return false;
        using var certificate = X509CertificateLoader.LoadCertificate(supplied.GetRawCertData());
        var now = clock.GetUtcNow();
        if (!Convert.ToHexString(SHA256.HashData(certificate.RawData)).Equals(approved.LeafSha256, StringComparison.OrdinalIgnoreCase) ||
            now < certificate.NotBefore.ToUniversalTime() || now >= certificate.NotAfter.ToUniversalTime() ||
            !certificate.MatchesHostname(host, allowWildcards: false, allowCommonName: false)) return false;
        var usages = certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>().ToArray();
        if (usages.Length != 1 || !usages[0].EnhancedKeyUsages.Cast<Oid>().Any(oid => oid.Value == "1.3.6.1.5.5.7.3.1")) return false;
        using var rsa = certificate.GetRSAPublicKey(); using var ec = certificate.GetECDsaPublicKey();
        return rsa?.KeySize >= 2048 || ec?.KeySize >= 256;
    }
}

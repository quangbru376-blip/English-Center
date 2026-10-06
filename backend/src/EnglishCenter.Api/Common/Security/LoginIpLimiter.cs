namespace EnglishCenter.Api.Common.Security;

using System.Net;
using System.Threading.RateLimiting;

public sealed class LoginIpLimiter : IDisposable
{
    private readonly PartitionedRateLimiter<string> _limiter = PartitionedRateLimiter.Create<string, string>(ip => RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
    public bool TryAcquire(IPAddress address, out int retryAfterSeconds)
    {
        ArgumentNullException.ThrowIfNull(address);
        var key = (address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address).ToString();
        using var lease = _limiter.AttemptAcquire(key, 1);
        retryAfterSeconds = lease.TryGetMetadata(MetadataName.RetryAfter, out var retry) ? Math.Max(1, (int)Math.Ceiling(retry.TotalSeconds)) : 60;
        return lease.IsAcquired;
    }
    public void Dispose() => _limiter.Dispose();
}

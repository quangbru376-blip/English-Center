namespace EnglishCenter.Api.Services.Academics;

using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Services.Auth;

public interface IAcademicRepository : ICurrentSessionRepository
{
    Task PrepareAccountsAsync(VerifiedSessionToken token, IEnumerable<int> targets, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task AuditAsync(AuditEntry audit, CancellationToken ct);
}
public static class AcademicAuthority
{
    public static async Task<CurrentSessionSnapshot> RequireAsync(ICurrentSessionRepository repository, VerifiedSessionToken token, BusinessClock clock, CancellationToken ct)
    { var current = await new CurrentSessionService(repository, clock).ReadAsync(token, true, ct); if (current.Context.Role != "AcademicAffairs") throw new ApiErrorException(ApiErrorCode.FORBIDDEN); return current; }
}

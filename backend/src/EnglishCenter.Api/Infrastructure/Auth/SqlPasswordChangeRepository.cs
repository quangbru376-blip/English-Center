namespace EnglishCenter.Api.Infrastructure.Auth;

using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Auth;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class SqlPasswordChangeRepository(SqlAuthDeadlineScope scope, SqlAuditAppender audit) : IPasswordChangeRepository
{
    private readonly SqlCurrentSessionRepository _current = new(scope);
    public Task<Account?> LockAccountAsync(int id, CancellationToken ct) => _current.LockAccountAsync(id, ct);
    public Task<AuthSession?> LockSessionAsync(byte[] sid, CancellationToken ct) => _current.LockSessionAsync(sid, ct);
    public Task<string?> RoleCodeAsync(int id, CancellationToken ct) => _current.RoleCodeAsync(id, ct);
    public Task SaveSessionAsync(CancellationToken ct) => _current.SaveSessionAsync(ct);
    public async Task<IReadOnlyList<AuthSession>> LockAllSessionsAsync(int id, CancellationToken ct)
    {
        scope.RequireActive(); var context = scope.Context;
        // The held Account serializes protocol writers. Lock each SID explicitly in
        // binary order; ORDER BY on a bulk UPDLOCK query alone cannot order lock acquisition.
        var ids = await context.AuthSessions.AsNoTracking().Where(s => s.AccountId == id)
            .OrderBy(s => s.Sid).Select(s => s.Sid).ToListAsync(ct);
        var result = new List<AuthSession>(ids.Count);
        foreach (var sid in ids)
        {
            var session = await _current.LockSessionAsync(sid, ct)
                ?? throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
            if (session.AccountId != id) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
            result.Add(session);
        }
        return result;
    }
    public async Task SaveChangeAsync(AuditEntry entry, CancellationToken ct)
    {
        scope.RequireActive(); var context = scope.Context;
        await context.SaveChangesAsync(ct);
        await audit.AppendMandatoryAsync((SqlConnection)context.Database.GetDbConnection(),
            (SqlTransaction)context.Database.CurrentTransaction!.GetDbTransaction(), entry, context.Database.GetCommandTimeout()!.Value, ct);
    }
}

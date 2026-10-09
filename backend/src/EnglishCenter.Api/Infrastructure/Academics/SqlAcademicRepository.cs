namespace EnglishCenter.Api.Infrastructure.Academics;

using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Infrastructure.Auth;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Academics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public class SqlAcademicRepository(SqlAuthDeadlineScope scope) : IAcademicRepository
{
    protected EnglishCenterDbContext Db => scope.Context;
    private readonly SqlCurrentSessionRepository current = new(scope);
    public Task<Account?> LockAccountAsync(int id, CancellationToken ct) => current.LockAccountAsync(id, ct);
    public Task<AuthSession?> LockSessionAsync(byte[] sid, CancellationToken ct) => current.LockSessionAsync(sid, ct);
    public Task<string?> RoleCodeAsync(int id, CancellationToken ct) => current.RoleCodeAsync(id, ct);
    public Task SaveSessionAsync(CancellationToken ct) => current.SaveSessionAsync(ct);
    public async Task PrepareAccountsAsync(VerifiedSessionToken token, IEnumerable<int> targets, CancellationToken ct)
    { foreach (var id in targets.Append(token.AccountId).Distinct().Order()) await LockAccountAsync(id, ct); await LockSessionAsync(token.CopySid(), ct); }
    public async Task SaveAsync(CancellationToken ct) => await Db.SaveChangesAsync(ct);
    public async Task AuditAsync(AuditEntry audit, CancellationToken ct) => await new SqlAuditAppender().AppendMandatoryAsync((SqlConnection)Db.Database.GetDbConnection(), (SqlTransaction)Db.Database.CurrentTransaction!.GetDbTransaction(), audit, scope.SqlSeconds, ct);
}

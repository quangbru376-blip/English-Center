namespace EnglishCenter.Api.Infrastructure.Administration;

using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Auth;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Administration;
using Microsoft.EntityFrameworkCore;

public sealed class SqlAdminAccountRepository(SqlAuthDeadlineScope scope) : IAdminAccountRepository
{
    private readonly SqlPasswordChangeRepository _password=new(scope,new());
    private readonly SqlCurrentSessionRepository _current=new(scope);
    public Task<Account?> LockAccountAsync(int id,CancellationToken ct)=>_current.LockAccountAsync(id,ct);
    public Task<AuthSession?> LockSessionAsync(byte[] sid,CancellationToken ct)=>_current.LockSessionAsync(sid,ct);
    public Task<string?> RoleCodeAsync(int id,CancellationToken ct)=>_current.RoleCodeAsync(id,ct);
    public Task SaveSessionAsync(CancellationToken ct)=>_current.SaveSessionAsync(ct);
    public Task SaveChangeAsync(AuditEntry audit,CancellationToken ct)=>_password.SaveChangeAsync(audit,ct);
    public Task<IReadOnlyList<AuthSession>> LockAllSessionsAsync(int id,CancellationToken ct)=>_password.LockAllSessionsAsync(id,ct);
    public async Task PrepareAsync(int actor,IReadOnlyCollection<int> targets,CancellationToken ct)
    {
        scope.RequireActive();var context=scope.Context;
        if(actor<=0||targets.Any(t=>t<=0))throw new ArgumentException("Invalid account reference.");
        // ADMINSET is held by the service. Discovery is not authorization.
        var admins=await context.Accounts.AsNoTracking().Where(a=>context.Roles.Any(r=>r.RoleId==a.RoleId&&r.RoleCode=="Admin"))
            .Select(a=>a.AccountId).ToListAsync(ct);
        var ids=admins.Append(actor).Concat(targets).Distinct().Order().ToArray();
        foreach(var id in ids)await _current.LockAccountAsync(id,ct);
        var involved=targets.Append(actor).Distinct().ToArray();
        var sids=await context.AuthSessions.AsNoTracking().Where(s=>involved.Contains(s.AccountId))
            .OrderBy(s=>s.Sid).Select(s=>s.Sid).ToListAsync(ct);
        // Explicit individual queries order acquisition; bulk ORDER BY does not.
        foreach(var sid in sids)await _current.LockSessionAsync(sid,ct);
    }
    public async Task<int> CountActiveAdminsAsync(CancellationToken ct)
    {
        scope.RequireActive();
        return await scope.Context.Accounts.AsNoTracking().CountAsync(a=>a.Status=="ACTIVE"&&
            scope.Context.Roles.Any(r=>r.RoleId==a.RoleId&&r.RoleCode=="Admin"),ct);
    }
}

namespace EnglishCenter.Api.Infrastructure.Auth;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Auth;
using Microsoft.EntityFrameworkCore;

public sealed class SqlCurrentSessionRepository(SqlAuthDeadlineScope scope) : ICurrentSessionRepository
{
    private EnglishCenterDbContext Context { get { scope.RequireActive(); return scope.Context; } }
    public async Task<Account?> LockAccountAsync(int accountId, CancellationToken ct)
    {
        var context = Context;
        var fresh = await context.Accounts.FromSqlInterpolated($"SELECT * FROM dbo.Account WITH(UPDLOCK,HOLDLOCK) WHERE account_id={accountId}").AsNoTracking().SingleOrDefaultAsync(ct);
        if (fresh is null) return null;
        var tracked = context.Accounts.Local.SingleOrDefault(a => a.AccountId == accountId);
        if (tracked is null) { context.Attach(fresh); return fresh; }
        var entry = context.Entry(tracked); if (entry.State != EntityState.Unchanged) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        entry.CurrentValues.SetValues(fresh); entry.OriginalValues.SetValues(fresh); entry.State = EntityState.Unchanged; return tracked;
    }
    public async Task<AuthSession?> LockSessionAsync(byte[] sid, CancellationToken ct)
    {
        var context = Context;
        var fresh = await context.AuthSessions.FromSqlInterpolated($"SELECT * FROM dbo.AuthSession WITH(UPDLOCK,HOLDLOCK) WHERE sid={sid}").AsNoTracking().SingleOrDefaultAsync(ct);
        if (fresh is null) return null;
        var tracked = context.AuthSessions.Local.SingleOrDefault(s => s.Sid.SequenceEqual(sid));
        if (tracked is null) { context.Attach(fresh); return fresh; }
        var entry = context.Entry(tracked); if (entry.State != EntityState.Unchanged) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        entry.CurrentValues.SetValues(fresh); entry.OriginalValues.SetValues(fresh); entry.State = EntityState.Unchanged; return tracked;
    }
    public Task<string?> RoleCodeAsync(int roleId, CancellationToken ct) => Context.Roles.AsNoTracking().Where(r => r.RoleId == roleId).Select(r => r.RoleCode).SingleOrDefaultAsync(ct);
    public async Task SaveSessionAsync(CancellationToken ct) => await Context.SaveChangesAsync(ct);
}

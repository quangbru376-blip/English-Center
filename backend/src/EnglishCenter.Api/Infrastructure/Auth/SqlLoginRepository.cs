namespace EnglishCenter.Api.Infrastructure.Auth;

using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Auth;
using Microsoft.EntityFrameworkCore;

public sealed class SqlLoginRepository(SqlAuthDeadlineScope scope) : ILoginRepository
{
    private EnglishCenterDbContext Context { get { scope.RequireActive(); return scope.Context; } }
    public async Task<Account?> LockAccountAsync(string email, CancellationToken cancellationToken)
    {
        var context = Context;
        var fresh = await context.Accounts.FromSqlInterpolated($"SELECT * FROM dbo.Account WITH(UPDLOCK,HOLDLOCK) WHERE email={email}").AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        if (fresh is null) return null;
        var tracked = context.Accounts.Local.SingleOrDefault(a => a.AccountId == fresh.AccountId);
        if (tracked is null) { context.Attach(fresh); return fresh; }
        var entry = context.Entry(tracked);
        if (entry.State != EntityState.Unchanged) throw new EnglishCenter.Api.Common.Errors.ApiErrorException(EnglishCenter.Api.Common.Errors.ApiErrorCode.STATE_CONFLICT);
        // Locks guard fresh SQL facts, never an earlier tracked status/version snapshot.
        entry.CurrentValues.SetValues(fresh); entry.OriginalValues.SetValues(fresh); entry.State=EntityState.Unchanged; return tracked;
    }
    public Task<string?> RoleCodeAsync(int roleId, CancellationToken cancellationToken) => Context.Roles.AsNoTracking().Where(r => r.RoleId == roleId).Select(r => r.RoleCode).SingleOrDefaultAsync(cancellationToken);
    public async Task SaveAccountAsync(CancellationToken cancellationToken) { await Context.SaveChangesAsync(cancellationToken); }
    public async Task InsertSessionAsync(AuthSession session, CancellationToken cancellationToken) { Context.Add(session); await Context.SaveChangesAsync(cancellationToken); }
}

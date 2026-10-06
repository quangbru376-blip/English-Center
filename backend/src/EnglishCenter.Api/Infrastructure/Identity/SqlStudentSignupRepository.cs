namespace EnglishCenter.Api.Infrastructure.Identity;

using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Services.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class SqlStudentSignupRepository(SqlAuthDeadlineScope scope) : IStudentSignupRepository
{
    private EnglishCenterDbContext Context { get { scope.RequireActive(); return scope.Context; } }
    public async Task<int> StudentRoleAsync(CancellationToken cancellationToken)
    {
        var role = await Context.Roles.AsNoTracking().SingleOrDefaultAsync(r => r.RoleCode == "Student", cancellationToken);
        if (role is null || role.RoleCode != "Student") throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
        return role.RoleId;
    }
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) => Context.Accounts.FromSqlInterpolated($"SELECT * FROM dbo.Account WITH(UPDLOCK,HOLDLOCK) WHERE email={email}").AsNoTracking().AnyAsync(cancellationToken);
    public Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken) => Context.Accounts.FromSqlInterpolated($"SELECT * FROM dbo.Account WITH(UPDLOCK,HOLDLOCK) WHERE phone={phone}").AsNoTracking().AnyAsync(cancellationToken);
    public async Task InsertWithAuditAsync(Account account, StudentProfile profile, DateTimeOffset at, CancellationToken cancellationToken)
    {
        var context = Context; context.Add(account); context.Add(profile);
        try { await context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException e) when (e.InnerException is SqlException sql && (sql.Number is 2601 or 2627) && (sql.Message.Contains("UQ_Account_Email", StringComparison.Ordinal) || sql.Message.Contains("UQ_Account_Phone", StringComparison.Ordinal)))
        {
            var field = sql.Message.Contains("UQ_Account_Email", StringComparison.Ordinal) ? "email" : sql.Message.Contains("UQ_Account_Phone", StringComparison.Ordinal) ? "phone" : null;
            throw new ApiErrorException(ApiErrorCode.CONFLICT, field);
        }
        await new SqlAuditAppender().AppendMandatoryAsync((SqlConnection)context.Database.GetDbConnection(),
            (SqlTransaction)context.Database.CurrentTransaction!.GetDbTransaction(), AuditEntry.StudentSignup(account.AccountId, account.RoleId, Guid.NewGuid(), at), scope.SqlSeconds, cancellationToken);
    }
}

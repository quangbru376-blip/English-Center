namespace EnglishCenter.Api.Infrastructure.Identity;

using System.Data;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Services.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public sealed class SqlStudentSignupExecutor(Func<EnglishCenterDbContext> contextFactory, CredentialHasher hasher, BusinessClock clock, int sqlSeconds, int lockMs) : IStudentSignupExecutor
{
    public async Task<RegisterStudentResult> ExecuteAsync(StudentSignupInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contextFactory); ArgumentNullException.ThrowIfNull(hasher); ArgumentNullException.ThrowIfNull(clock); ArgumentNullException.ThrowIfNull(input);
        if (sqlSeconds <= 0 || lockMs <= 0 || lockMs >= (long)sqlSeconds * 1000) throw new ArgumentException("Invalid auth deadline profile.");
        await using var context = contextFactory();
        if (context.Database.CurrentTransaction is not null) throw new ApiErrorException(ApiErrorCode.STATE_CONFLICT);
        await context.Database.OpenConnectionAsync(cancellationToken);
        await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await using var scope = await SqlAuthDeadlineScope.EnterAsync(context, sqlSeconds, lockMs, cancellationToken);
        try
        {
            var result = await new StudentSignupService(new SqlStudentSignupRepository(scope), hasher, clock).RegisterAsync(input, cancellationToken);
            await tx.CommitAsync(cancellationToken); return result;
        }
        catch (SqlException e) when (e.Number is 1205 or 1222) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
        catch (DbUpdateException e) when (e.InnerException is SqlException sql && sql.Number is 1205 or 1222) { throw new ApiErrorException(ApiErrorCode.CONFLICT); }
    }
}

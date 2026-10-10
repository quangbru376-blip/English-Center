namespace EnglishCenter.Api.Infrastructure.Enrollment;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Enrollment;

public sealed class SqlStudentEnrollmentExecutor(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    private readonly SqlEnrollmentTransactions transactions = new(factory, clock, sqlSeconds, lockMs);

    public async Task<object> CommandAsync(VerifiedSessionToken token, int api, int registration, int target, Guid request, string json, int bytes, CancellationToken ct)
    {
        if (api is not (25 or 28 or 30 or 31 or 32 or 36)) throw new ApiErrorException(ApiErrorCode.FORBIDDEN);
        await transactions.Write(async (scope, c) =>
        {
            var repository = new SqlEnrollmentRepository(scope);
            await EnrollmentAuthority.RequireAsync(repository, token, clock, "Student", c);
            if (api == 25) await repository.OwnStudentAsync(token.AccountId, c);
            else await repository.RequireOwnRegistrationAsync(token.AccountId, registration, api is 31 or 32 ? target : null, c);
            return true;
        }, ct);
        var input = EnrollmentInput.Parse(api, registration, target, request, json, bytes);
        return await transactions.Write((scope, c) => new StudentEnrollmentService(new(scope), new SqlReplayRepository(scope, lockMs), clock).CommandAsync(token, input, c), ct);
    }
}

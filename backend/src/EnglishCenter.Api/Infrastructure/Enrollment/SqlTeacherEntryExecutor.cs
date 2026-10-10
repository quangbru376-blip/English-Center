namespace EnglishCenter.Api.Infrastructure.Enrollment;

using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Enrollment;

public sealed class SqlTeacherEntryExecutor(Func<EnglishCenterDbContext> factory, BusinessClock clock, int sqlSeconds, int lockMs)
{
    private readonly SqlEnrollmentTransactions transactions = new(factory, clock, sqlSeconds, lockMs);
    public async Task<TeacherCommandResult> CommandAsync(VerifiedSessionToken token, int api, int target, Guid request, string json, int bytes, CancellationToken ct)
    {
        if (api is not (70 or 71 or 72)) throw new Common.Errors.ApiErrorException(Common.Errors.ApiErrorCode.FORBIDDEN);
        // This short preflight commits no business effect. All authority is rechecked
        // inside the subsequent request/business-gated owned transaction.
        await transactions.Write(async (s, c) =>
        {
            var repository = new SqlEnrollmentRepository(s);
            await EnrollmentAuthority.RequireAsync(repository, token, clock, "Teacher", c);
            return await repository.RequireTeacherAsync(token.AccountId, target, api == 72, c);
        }, ct);
        var input = EnrollmentInput.Parse(api, 0, target, request, json, bytes);
        return await transactions.Write((s, c) => new TeacherEntryService(new(s), new SqlReplayRepository(s, lockMs), clock).CommandAsync(token, input, c), ct);
    }
}

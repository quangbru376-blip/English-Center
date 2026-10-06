namespace EnglishCenter.Tests.Api.Auth;

using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Services.Auth;
using Xunit;

public class LoginAuditCompletionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task CommittedSuccessOrRejectedCounterOutcomeSurvivesAuditFailureOrCancellation(bool accepted, bool canceled)
    {
        var decision = accepted ? LoginDecision.Success(new(new(7, "Synthetic", "fixture@example.invalid", "Student", "NORMAL", "AAAAAAAAAAE=", "2026-10-05T00:00:00.000Z", "2026-10-05T02:00:00.000Z", "2026-10-05T00:30:00.000Z"), "fixture-only", DateTimeOffset.UtcNow.AddHours(2))) : LoginDecision.Rejected();
        var store = new Failure(canceled);
        var incidents = new AuditIncidentCounter();
        var result = await LoginAuditCompletion.AfterCommitAsync(decision, new(store, incidents), new BusinessClock(TimeProvider.System), TestContext.Current.CancellationToken);
        Assert.Same(decision, result);
        Assert.Equal(1, incidents.GetFailureCount(SecurityAuditKind.Login));
        Assert.NotNull(store.Entry);
        Assert.Equal(accepted ? 7 : (int?)null, store.Entry.ActorAccountId);
        Assert.Null(store.Entry.Details);
        Assert.Null(store.Entry.IpAddress);
        Assert.Equal(accepted ? "SUCCESS" : "FAILURE", store.Entry.Result);
    }
    private sealed class Failure(bool canceled) : ISecurityAuditStore
    {
        internal AuditEntry? Entry;
        public Task AppendAsync(AuditEntry entry, CancellationToken cancellationToken)
        {
            Entry = entry;
            return Task.FromException(canceled ? new OperationCanceledException() : new InvalidOperationException("fixture failure"));
        }
    }
}

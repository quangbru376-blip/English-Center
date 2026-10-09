using EnglishCenter.Api.Infrastructure.Operator;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class ManagedOperatorLifetimeTests
{
    [Theory][InlineData("")][InlineData("stop")]
    public async Task ParentEofOrInputCancelsOwnHost(string input)
    {
        var canceled=false;
        var result=await ManagedOperatorLifetime.RunAsync(async token=>{
            try{await Task.Delay(Timeout.InfiniteTimeSpan,token);}catch(OperationCanceledException){canceled=token.IsCancellationRequested;}
            return 0;
        },new StringReader(input),TestContext.Current.CancellationToken);
        Assert.Equal(0,result);Assert.True(canceled);
    }
    [Fact]
    public async Task HostFailureDoesNotWaitForUncooperativeParentInput()
    {
        using var input=new PendingReader();
        Assert.Equal(1,await ManagedOperatorLifetime.RunAsync(_=>Task.FromResult(1),input,TestContext.Current.CancellationToken));
        input.Release();
    }
    [Fact]
    public async Task CallerCancellationPrecedesHostAndInput()
    {
        using var canceled=new CancellationTokenSource();canceled.Cancel();var invoked=false;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>ManagedOperatorLifetime.RunAsync(_=>{invoked=true;return Task.FromResult(0);},new StringReader(""),canceled.Token));
        Assert.False(invoked);
    }
    [Fact]
    public async Task MissingDependencyCannotCountACommittedCleanup()
    {
        using var service=new EnglishCenter.Api.Infrastructure.Maintenance.SqlMaintenanceService(LocalRuntimeReferences.Maintenance,
            LocalRuntimeReferences.Execution,3600,TimeProvider.System,_=>ValueTask.FromResult<string?>(null));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.RunReplayOnceAsync(TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.RunAuditOnceAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,service.CompletedReplayRuns);Assert.Equal(0,service.CompletedAuditRuns);
    }
    private sealed class PendingReader:TextReader
    {
        private readonly TaskCompletionSource<int> pending=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override ValueTask<int> ReadAsync(Memory<char> buffer,CancellationToken cancellationToken=default)=>new(pending.Task);
        public void Release()=>pending.TrySetResult(0);
    }
}
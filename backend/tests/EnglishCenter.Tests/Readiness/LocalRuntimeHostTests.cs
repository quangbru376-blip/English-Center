using EnglishCenter.Api.Infrastructure.Operator;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class LocalRuntimeHostTests
{
    [Fact]
    public async Task OperatorCannotOpenOrRunRuntimeHost()
    {
        Assert.Null(await LocalRuntimeHost.CreateAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await LocalRuntimeHost.RunAsync(TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task CallerCancellationPrecedesCustodyAccess()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => LocalRuntimeHost.CreateAsync(cancellation.Token));
    }
}

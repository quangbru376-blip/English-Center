using EnglishCenter.Api.Infrastructure.Operator;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class RuntimeCsrfProvisionerTests
{
    [Fact]
    public void UnassignedCurrentIdentityCannotProvisionDedicatedRuntimeStore()
    {
        Assert.Equal(1, RuntimeCsrfProvisioner.Run(TestContext.Current.CancellationToken));
    }
}

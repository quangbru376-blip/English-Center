using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class DatabaseReadinessProbeTests
{
    internal static RuntimeProfile ValidProfile => ProfileFixture.Valid with
    { Database = ProfileFixture.Valid.Database! with { SchemaManifestReference = DatabaseReadinessProbe.SchemaManifestReference } };

    [Fact]
    public void OnlyKnownRuntimeAndSixKnownExclusionsAreSafe()
        => Assert.True(RuntimePrincipalPolicy.IsSafe(true, false, false, false, false, false, false));

    [Theory]
    [InlineData(0, false)] [InlineData(0, null)]
    [InlineData(1, true)] [InlineData(1, null)] [InlineData(2, true)] [InlineData(2, null)]
    [InlineData(3, true)] [InlineData(3, null)] [InlineData(4, true)] [InlineData(4, null)]
    [InlineData(5, true)] [InlineData(5, null)] [InlineData(6, true)] [InlineData(6, null)]
    public void AnyUnsafeOrUnknownPrivilegeFactFailsClosed(int index, bool? value)
    {
        bool?[] facts = [true, false, false, false, false, false, false];
        facts[index] = value;
        Assert.False(RuntimePrincipalPolicy.IsSafe(facts[0], facts[1], facts[2], facts[3], facts[4], facts[5], facts[6]));
    }

    [Theory]
    [InlineData("connection")] [InlineData("schema")] [InlineData("compatibility")] [InlineData("profile")]
    public async Task UnboundOrInvalidDescriptorsNeverResolveCredentials(string variant)
    {
        var calls = 0;
        var probe = new DatabaseReadinessProbe("synthetic-connection-reference", _ =>
        { calls++; throw new Exception("private-config-detail"); });
        var profile = variant switch
        {
            "connection" => ValidProfile with { Database = ValidProfile.Database! with { ConnectionSecretReference = "different" } },
            "schema" => ValidProfile with { Database = ValidProfile.Database! with { SchemaManifestReference = "different" } },
            "compatibility" => ValidProfile with { Database = ValidProfile.Database! with { EfCompatibility = 170 } },
            _ => ValidProfile with { Kind = (RuntimeProfileKind)0 }
        };
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(profile, TestContext.Current.CancellationToken));
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData("Server=private-secret;Password=example;Encrypt=True")]
    [InlineData("Server=remote.invalid\\SQLEXPRESS;Database=MetadataOnly;Integrated Security=True;Encrypt=True")]
    [InlineData("Server=.\\SQLEXPRESS;Database=MetadataOnly;Integrated Security=True;Encrypt=False")]
    [InlineData("Server=.\\SQLEXPRESS;Database=MetadataOnly;Integrated Security=True")]
    [InlineData("Server=.\\SQLEXPRESS;Database=MetadataOnly;Integrated Security=True;Encrypt=True;AttachDbFilename=forbidden.mdf")]
    [InlineData("Server=.\\SQLEXPRESS;Database=MetadataOnly;Integrated Security=True;Encrypt=True;User Instance=True")]
    [InlineData("Server=tcp:localhost,1433;Database=MetadataOnly;Integrated Security=True;Encrypt=True")]
    [InlineData("Password='malformed")]
    public async Task MissingMalformedRemoteOrUnsafeConnectionRejectsWithoutDiagnostic(string? resolved)
    {
        var probe = new DatabaseReadinessProbe("synthetic-connection-reference", _ => ValueTask.FromResult(resolved));
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(ValidProfile, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ResolverExceptionsDoNotExposeCredentialOrPathDetails()
    {
        var probe = new DatabaseReadinessProbe("synthetic-connection-reference", _ => throw new Exception("private-config-detail"));
        Assert.Equal(ReadinessProbeStatus.NotReady, await probe.CheckAsync(ValidProfile, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task PreCancelledRequestNeverResolvesConnection()
    {
        var calls = 0;
        var probe = new DatabaseReadinessProbe("synthetic-connection-reference", _ => { calls++; return ValueTask.FromResult<string?>(null); });
        using var cancelled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => probe.CheckAsync(ValidProfile, cancelled.Token));
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task CallerCancellationDuringResolutionPropagates()
    {
        using var cancelled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        CancellationToken observed = default;
        var probe = new DatabaseReadinessProbe("synthetic-connection-reference", token =>
        { observed = token; cancelled.Cancel(); return ValueTask.FromCanceled<string?>(token); });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => probe.CheckAsync(ValidProfile, cancelled.Token));
        Assert.True(observed.IsCancellationRequested);
    }

    [Fact]
    public async Task UncooperativeAsyncResolverCannotKeepProbePendingPastDeadline()
    {
        var pending = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationToken observed = default;
        var probe = new DatabaseReadinessProbe("synthetic-connection-reference", token =>
        { observed = token; return new ValueTask<string?>(pending.Task); });
        var profile = ValidProfile with { Execution = ValidProfile.Execution! with { ReadinessMs = 30 } };
        var result = await probe.CheckAsync(profile, TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);
        Assert.Equal(ReadinessProbeStatus.NotReady, result);
        Assert.True(observed.IsCancellationRequested);
        pending.TrySetException(new Exception("late-private-detail"));
    }
}

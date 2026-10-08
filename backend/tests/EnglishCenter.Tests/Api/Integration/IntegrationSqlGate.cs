namespace EnglishCenter.Tests.Api.Integration;

using Xunit;

public static class IntegrationSqlGate
{
    public static bool Enabled => Environment.GetEnvironmentVariable("EC_V01_SQL") == "1";
}
[CollectionDefinition("V01 HTTP SQL", DisableParallelization = true)]
public sealed class IntegrationSqlCollection;

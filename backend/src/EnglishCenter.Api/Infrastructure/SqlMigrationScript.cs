namespace EnglishCenter.Api.Infrastructure;

internal static class SqlMigrationScript
{
    internal static string Batch(string versionedName) => $"EXEC(N'{Read(versionedName).Replace("'", "''")}');";

    internal static string Read(string versionedName)
    {
        var name = $"EnglishCenter.Api.Infrastructure.Sql.{versionedName}";
        using var stream = typeof(SqlMigrationScript).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Missing embedded migration script: {name}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

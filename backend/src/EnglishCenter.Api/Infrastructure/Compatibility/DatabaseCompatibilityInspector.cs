namespace EnglishCenter.Api.Infrastructure.Compatibility;

using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed record DatabaseCompatibilityResult(bool IsCompatible, string Cause);

/// <summary>SQL compatibility only; callers must also validate the full runtime profile.</summary>
public sealed class DatabaseCompatibilityInspector
{
    public async Task<DatabaseCompatibilityResult> InspectAsync(EnglishCenterDbContext context,
        int commandTimeoutSeconds, bool requireRuntimePrincipal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (commandTimeoutSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(commandTimeoutSeconds));
        cancellationToken.ThrowIfCancellationRequested();
        var connection = context.Database.GetDbConnection();
        var openedHere = connection.State == ConnectionState.Closed;
        try
        {
            if (openedHere) await context.Database.OpenConnectionAsync(cancellationToken);
            if (requireRuntimePrincipal)
            {
                await using var principal = connection.CreateCommand();
                principal.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
                principal.CommandTimeout = commandTimeoutSeconds;
                principal.CommandText = "SELECT CASE WHEN USER_NAME()<>'dbo' AND IS_ROLEMEMBER('ec_app_runtime')=1 AND IS_ROLEMEMBER('db_owner')=0 AND IS_ROLEMEMBER('db_ddladmin')=0 AND IS_SRVROLEMEMBER('sysadmin')=0 AND HAS_PERMS_BY_NAME(DB_NAME(),'DATABASE','CONTROL')=0 THEN 1 ELSE 0 END;";
                if (await principal.ExecuteScalarAsync(cancellationToken) is not int value || value != 1)
                    return new(false, "UNSAFE_PRINCIPAL");
            }
            var expected = JsonSerializer.Deserialize<Dictionary<string, string>>(ReadResource("ExpectedCatalog.json"))
                ?? throw new InvalidOperationException("Invalid compatibility manifest");
            var actual = new Dictionary<string, string>(StringComparer.Ordinal);
            await using var command = connection.CreateCommand();
            command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
            command.CommandTimeout = commandTimeoutSeconds;
            command.CommandText = ReadResource("ReadCatalog.sql");
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            do
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    if (reader.IsDBNull(0) || reader.IsDBNull(1)) return new(false, "UNREADABLE_CATALOG");
                    if (!actual.TryAdd(reader.GetString(0), reader.GetString(1)))
                        return new(false, "CATALOG_DRIFT");
                }
            } while (await reader.NextResultAsync(cancellationToken));
            foreach (var key in expected.Keys.Where(k => k.StartsWith("module/", StringComparison.Ordinal)).ToArray())
                expected[key] = CatalogCompatibility.NormalizeModuleDefinition(expected[key]);
            foreach (var key in actual.Keys.Where(k => k.StartsWith("module/", StringComparison.Ordinal)).ToArray())
                actual[key] = CatalogCompatibility.NormalizeModuleDefinition(actual[key]);
            return CatalogCompatibility.Matches(expected, actual)
                ? new(true, "COMPATIBLE") : new(false, "CATALOG_DRIFT");
        }
        catch (JsonException) { return new(false, "DATABASE_UNAVAILABLE"); }
        catch (SqlException) { return new(false, "DATABASE_UNAVAILABLE"); }
        catch (InvalidOperationException) { return new(false, "DATABASE_UNAVAILABLE"); }
        finally
        {
            if (openedHere) await context.Database.CloseConnectionAsync();
        }
    }

    private static string ReadResource(string name)
    {
        using var stream = typeof(DatabaseCompatibilityInspector).Assembly.GetManifestResourceStream(
            "EnglishCenter.Api.Infrastructure.Compatibility." + name)
            ?? throw new InvalidOperationException("Compatibility manifest unavailable");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

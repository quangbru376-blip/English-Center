using System.Data;
using EnglishCenter.Api.Infrastructure.Compatibility;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public sealed class DatabaseReadinessProbe : IRuntimeReadinessProbe
{
    public const string SchemaManifestReference = "embedded:EnglishCenter.Api.Infrastructure.Compatibility.ExpectedCatalog.json";
    private long completedPrincipalChecks;
    public long CompletedPrincipalChecks => Interlocked.Read(ref completedPrincipalChecks);
    private readonly string connectionReference;
    private readonly Func<CancellationToken, ValueTask<string?>> resolveConnection;

    public DatabaseReadinessProbe(string connectionReference, Func<CancellationToken, ValueTask<string?>> resolveConnection)
    {
        this.connectionReference = connectionReference;
        this.resolveConnection = resolveConnection ?? throw new ArgumentNullException(nameof(resolveConnection));
    }
    public ReadinessDependency Dependency => ReadinessDependency.DatabaseAuthorityAndCatalog;

    public async Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (RuntimeProfileValidator.Validate(profile) != ProfileIssue.None || string.IsNullOrWhiteSpace(connectionReference) ||
            !string.Equals(profile.Database!.ConnectionSecretReference, connectionReference, StringComparison.Ordinal) ||
            profile.Database.SchemaManifestReference != SchemaManifestReference) return ReadinessProbeStatus.NotReady;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(profile.Execution!.ReadinessMs);
        try
        {
            var resolution = resolveConnection(deadline.Token).AsTask();
            _ = resolution.ContinueWith(static task => { _ = task.Exception; }, CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            var resolved = await resolution.WaitAsync(deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(resolved)) return ReadinessProbeStatus.NotReady;
            var connection = PrepareConnection(resolved, profile.Execution);
            if (connection is null) return ReadinessProbeStatus.NotReady;
            var options = new DbContextOptionsBuilder<EnglishCenterDbContext>()
                .UseSqlServer(connection.ConnectionString, sql =>
                    sql.UseCompatibilityLevel(160).CommandTimeout(profile.Execution.SqlSeconds)).Options;
            await using var context = new EnglishCenterDbContext(options);
            await context.Database.OpenConnectionAsync(deadline.Token);
            if (!await HasSafePrincipalAsync(context, profile.Execution.SqlSeconds, deadline.Token))
                return ReadinessProbeStatus.NotReady;
            var result = await new DatabaseCompatibilityInspector().InspectAsync(context,
                profile.Execution.SqlSeconds, requireRuntimePrincipal: true, deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();
            return result.IsCompatible && result.Cause == "COMPATIBLE" ? ReadinessProbeStatus.Pass : ReadinessProbeStatus.NotReady;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception)
        { cancellationToken.ThrowIfCancellationRequested(); return ReadinessProbeStatus.NotReady; }
    }

    private static SqlConnectionStringBuilder? PrepareConnection(string resolved, ExecutionProfile execution)
    {
        var builder = new SqlConnectionStringBuilder(resolved);
        if (!builder.ShouldSerialize("Encrypt") || builder.Encrypt.Equals(SqlConnectionEncryptOption.Optional) ||
            string.IsNullOrWhiteSpace(builder.InitialCatalog) || builder.UserInstance || builder.AttachDBFilename.Length != 0)
            return null;
        var source = builder.DataSource;
        if (source.StartsWith("lpc:", StringComparison.OrdinalIgnoreCase)) source = source[4..];
        var parts = source.Split('\\');
        if (parts.Length != 2 || parts[0] is not ("." or "localhost" or "(local)") ||
            string.IsNullOrWhiteSpace(parts[1]) || !parts[1].All(character => char.IsAsciiLetterOrDigit(character) || character == '_'))
            return null;
        // Force local shared memory; a SQL client alias cannot silently redirect this probe.
        builder.DataSource = "lpc:.\\" + parts[1];
        builder.Pooling = false;
        builder.Enlist = false;
        builder.ConnectRetryCount = 0;
        builder.PersistSecurityInfo = false;
        builder.ConnectTimeout = Math.Min(execution.SqlSeconds, (int)((execution.ReadinessMs + 999L) / 1000));
        return builder;
    }

    private async Task<bool> HasSafePrincipalAsync(EnglishCenterDbContext context, int timeout, CancellationToken token)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandTimeout = timeout;
        using var resource = typeof(DatabaseReadinessProbe).Assembly.GetManifestResourceStream(
            "EnglishCenter.Api.Infrastructure.Sql.Readiness.RuntimePrincipalRead.sql")
            ?? throw new InvalidOperationException("Readiness principal query unavailable");
        using var text = new StreamReader(resource);
        command.CommandText = await text.ReadToEndAsync(token);
        await using var reader = await command.ExecuteReaderAsync(token);
        if (reader.FieldCount != 8 || !await reader.ReadAsync(token)) return false;
        var facts = new bool?[7];
        for (var index = 0; index < facts.Length; index++)
            facts[index] = reader.IsDBNull(index) ? null : reader.GetInt32(index) switch { 0 => false, 1 => true, _ => null };
        var express = !reader.IsDBNull(7) && reader.GetInt32(7) == 4;
        if (await reader.ReadAsync(token) || await reader.NextResultAsync(token)) return false;
        Interlocked.Increment(ref completedPrincipalChecks);
        return express && RuntimePrincipalPolicy.IsSafe(facts[0], facts[1], facts[2], facts[3], facts[4], facts[5], facts[6]);
    }
}

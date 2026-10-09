using System.Data;
using System.Security.Principal;
using System.Text.Json;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Bootstrap;
using EnglishCenter.Api.Services.Bootstrap;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Api.Infrastructure.Operator;

public static class LocalAdminBootstrap
{
    public const string Email = "admin@englishcenter.local";
    public const string FullName = "LOCAL_DEV Administrator";
    public enum Outcome { Refused, Created, Verified }
    public sealed class Envelope
    {
        public int DatabaseId { get; }
        public Guid DatabaseGuid { get; }
        public bool VerifyOnly { get; }
        internal string Password { get; }
        internal Envelope(int id, Guid guid, bool verify, string password)
        { DatabaseId = id; DatabaseGuid = guid; VerifyOnly = verify; Password = password; }
        public override string ToString() => "Protected bootstrap envelope";
    }
    public static async Task<Envelope?> ReadEnvelopeAsync(TextReader input, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(input);
        var chars = new char[4097]; var length = 0;
        try
        {
            while (length < chars.Length)
            {
                var count = await input.ReadAsync(chars.AsMemory(length), token);
                if (count == 0) break;
                length += count;
            }
            if (length is 0 or > 4096) return null;
            using var json = JsonDocument.Parse(new string(chars, 0, length), new JsonDocumentOptions { MaxDepth = 2 });
            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var field in root.EnumerateObject())
                if (!names.Add(field.Name) || field.Name is not ("databaseId" or "databaseGuid" or "verifyOnly" or "password")) return null;
            if (names.Count != 4 || !root.GetProperty("databaseId").TryGetInt32(out var id) || id <= 4 ||
                root.GetProperty("databaseGuid").ValueKind != JsonValueKind.String ||
                !Guid.TryParseExact(root.GetProperty("databaseGuid").GetString(), "D", out var guid) || guid == Guid.Empty ||
                root.GetProperty("verifyOnly").ValueKind is not (JsonValueKind.True or JsonValueKind.False) ||
                root.GetProperty("password").ValueKind != JsonValueKind.String) return null;
            var password = root.GetProperty("password").GetString();
            return password is { Length: >= 32 and <= 128 } ? new(id, guid, root.GetProperty("verifyOnly").GetBoolean(), password) : null;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception) { return null; }
        finally { Array.Clear(chars); }
    }
    public static async Task<Outcome> RunAsync(TextReader input, CancellationToken token, Action<string>? checkpoint = null)
    {
        token.ThrowIfCancellationRequested(); checkpoint?.Invoke("identity");
        if (!OperatingSystem.IsWindows()) return Outcome.Refused;
        using var identity = WindowsIdentity.GetCurrent();
        if (identity.Name != Environment.MachineName + @"\EcM0Schema" ||
            new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator)) return Outcome.Refused;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token); deadline.CancelAfter(30000);
        try
        {
            checkpoint?.Invoke("input");
            var envelope = await ReadEnvelopeAsync(input, deadline.Token);
            if (envelope is null) return Outcome.Refused;
            var options = new DbContextOptionsBuilder<EnglishCenterDbContext>().UseSqlServer(LocalRuntimeReferences.SqlConnection,
                sql => sql.UseCompatibilityLevel(160).CommandTimeout(10)).Options;
            await using var db = new EnglishCenterDbContext(options);
            checkpoint?.Invoke("connection");
            await db.Database.OpenConnectionAsync(deadline.Token);
            var connection = (SqlConnection)db.Database.GetDbConnection();
            checkpoint?.Invoke("catalog");
            await using (var command = connection.CreateCommand())
            {
                command.CommandTimeout = 10;
                command.CommandText = "SELECT DB_ID(),r.database_guid,CASE WHEN DB_NAME()=N'EnglishCenter_M0_LOCALDEV' AND USER_NAME()=N'M0SchemaUser' AND ORIGINAL_LOGIN()=@login AND IS_ROLEMEMBER('db_owner')=1 AND IS_SRVROLEMEMBER('sysadmin')=0 AND HAS_PERMS_BY_NAME(NULL,NULL,'CONTROL SERVER')=0 THEN 1 ELSE 0 END FROM sys.database_recovery_status r WHERE r.database_id=DB_ID()";
                command.Parameters.Add(new SqlParameter("@login", System.Data.SqlDbType.NVarChar, 256) { Value = identity.Name });
                await using var reader = await command.ExecuteReaderAsync(deadline.Token);
                if (!await reader.ReadAsync(deadline.Token) || reader.GetInt32(0) != envelope.DatabaseId || reader.GetGuid(1) != envelope.DatabaseGuid ||
                    reader.GetInt32(2) != 1 || await reader.ReadAsync(deadline.Token)) return Outcome.Refused;
            }
            checkpoint?.Invoke("migrations");
            var migrations = db.Database.GetMigrations().ToArray();
            if (migrations.Length != 6 || !(await db.Database.GetAppliedMigrationsAsync(deadline.Token)).SequenceEqual(migrations, StringComparer.Ordinal)) return Outcome.Refused;
            checkpoint?.Invoke("credential");
            var hasher = new CredentialHasher(new PasswordPolicy(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist"))));
            if (envelope.VerifyOnly) return await MatchesAsync(db, hasher, envelope.Password, deadline.Token) ? Outcome.Verified : Outcome.Refused;
            checkpoint?.Invoke("freshness");
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, deadline.Token);
            if (await db.Accounts.AnyAsync(deadline.Token) || await db.Roles.AnyAsync(deadline.Token) || await db.AuditLogs.AnyAsync(deadline.Token) ||
                await db.AuthSessions.AnyAsync(deadline.Token) || await db.StudentProfiles.AnyAsync(deadline.Token)) return Outcome.Refused;
            checkpoint?.Invoke("bootstrap");
            var service = new BootstrapService(new SqlBootstrapRepository(db, 10, 3000), hasher);
            if (await service.EnsureAdminAsync(Email, FullName, envelope.Password, DateTimeOffset.UtcNow, deadline.Token) != BootstrapOutcome.Created ||
                !await MatchesAsync(db, hasher, envelope.Password, deadline.Token) || await db.AuditLogs.CountAsync(deadline.Token) != 1) return Outcome.Refused;
            checkpoint?.Invoke("commit");
            await transaction.CommitAsync(deadline.Token);
            return Outcome.Created;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception) { return Outcome.Refused; }
    }
    private static async Task<bool> MatchesAsync(EnglishCenterDbContext db, CredentialHasher hasher, string password, CancellationToken token)
    {
        if (!FixedRoleCodes.MatchesExact(await db.Roles.Select(role => role.RoleCode).ToArrayAsync(token))) return false;
        var admins = await db.Accounts.AsNoTracking().Where(account => account.Role!.RoleCode == "Admin").ToArrayAsync(token);
        return admins.Length == 1 && admins[0].Email == Email && admins[0].FullName == FullName &&
            hasher.Verify(admins[0].PasswordHash, password) != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed;
    }
}
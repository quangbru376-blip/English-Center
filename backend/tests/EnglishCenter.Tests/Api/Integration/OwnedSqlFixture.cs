namespace EnglishCenter.Tests.Api.Integration;

using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text.Json;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure;
using EnglishCenter.Api.Infrastructure.Administration;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Infrastructure.Auth;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Administration;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public static class OwnedSqlGate
{
    public static bool Enabled => IntegrationSqlGate.Enabled && Environment.GetEnvironmentVariable("EC_V01_OWNED_SQL") == "1";
}

// This fixture never opens D07. Every observed mutation uses real owned COMMIT.
internal sealed class OwnedSqlFixture : IAsyncDisposable
{
    private const string Prefix = "EnglishCenter_V01E_20261008_";
    private const string Master = "Server=lpc:.\\SQLEXPRESS;Database=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Pooling=False";
    internal static CancellationToken Ct => TestContext.Current.CancellationToken;
    private readonly string _name = Prefix + Guid.NewGuid().ToString("N");
    private int _databaseId;
    private Guid _databaseGuid;
    private bool _created;
    private string Connection => new SqlConnectionStringBuilder(Master) { InitialCatalog = _name }.ConnectionString;
    private readonly IJwtSessionIssuer _signer;
    internal BusinessClock Clock { get; }
    internal CredentialHasher Hasher { get; }
    internal string Password { get; }
    internal JwtSessionValidator Validator { get; }
    internal readonly AuditIncidentCounter Incidents = new();
    internal readonly Dictionary<string, int> Accounts = new(StringComparer.Ordinal);
    private sealed class FixedClock : TimeProvider
    {
        private readonly DateTimeOffset _now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        public override DateTimeOffset GetUtcNow() => _now;
    }
    private OwnedSqlFixture()
    {
        Clock = new(new FixedClock());
        var policy = new PasswordPolicy(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist")));
        Hasher = new(policy); Password = new TemporaryPasswordGenerator(policy).Generate();
        var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
        _signer = new JwtSessionIssuer(new(key, "HS256"), "v01e-fixture", "v01e-fixture");
        Validator = new([key], ["HS256"], "v01e-fixture", "v01e-fixture", Clock, 4096);
    }
    internal EnglishCenterDbContext Db(params IInterceptor[] interceptors) => new(new DbContextOptionsBuilder<EnglishCenterDbContext>()
        .UseSqlServer(Connection, sql => sql.UseCompatibilityLevel(160)).AddInterceptors(interceptors).Options);
    internal SecurityAuditRecorder Audit => new(new SqlSecurityAuditStore(() => new SqlConnection(Connection), 1), Incidents);
    internal SqlStudentSignupExecutor Signup => new(() => Db(), Hasher, Clock, 10, 1000);
    internal SqlLoginExecutor Login => new(() => Db(), Hasher, Clock, _signer, Audit, 10, 1000);
    internal SqlCurrentSessionReader Current => new(() => Db(), Clock, 10, 1000);
    internal SqlCurrentSessionLogout Logout => new(() => Db(), Clock, Audit, 10, 1000);
    internal SqlPasswordChangeExecutor Change => new(() => Db(), Hasher, Clock, 10, 1000);
    internal SqlAdminAccountExecutor Admin => new(() => Db(), Clock, 10, 1000);
    internal static async Task<OwnedSqlFixture> CreateAsync()
    {
        if (!OwnedSqlGate.Enabled) throw new InvalidOperationException("Owned SQL requires both explicit gates.");
        var f = new OwnedSqlFixture(); f.ValidateName();
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("EC_V01_E_RESULTS"))) throw new InvalidOperationException("Ownership receipt directory required before creation.");
        try
        {
            await using var master = new SqlConnection(Master); await master.OpenAsync(Ct);
            await using var create = master.CreateCommand(); create.CommandTimeout = 30;
            create.CommandText = $"IF DB_ID(@name) IS NOT NULL THROW 51000,'Scratch already exists',1; CREATE DATABASE [{f._name}] COLLATE Latin1_General_100_CI_AS_SC;";
            create.Parameters.AddWithValue("@name", f._name); await create.ExecuteNonQueryAsync(Ct); f._created = true;
            (f._databaseId, f._databaseGuid) = await f.Identity(master);
            await f.Receipt("created");
            create.Parameters.Clear(); create.CommandText = $"ALTER DATABASE [{f._name}] SET AUTO_CLOSE OFF; ALTER DATABASE [{f._name}] SET COMPATIBILITY_LEVEL=160; ALTER DATABASE [{f._name}] SET ALLOW_SNAPSHOT_ISOLATION ON;";
            await create.ExecuteNonQueryAsync(Ct);
            await using var db = f.Db(); await db.Database.MigrateAsync(Ct);
            Assert.Equal(6, (await db.Database.GetAppliedMigrationsAsync(Ct)).Count());
            var hash = f.Hasher.Hash(f.Password);
            foreach (var code in FixedRoleCodes.All)
            {
                var role = new Role { RoleCode = code, RoleName = code }; db.Roles.Add(role); await db.SaveChangesAsync(Ct);
                var account = f.Account(code, role.RoleId, hash); db.Accounts.Add(account); await db.SaveChangesAsync(Ct); f.Accounts.Add(code, account.AccountId);
            }
            var adminRole = await db.Roles.SingleAsync(r => r.RoleCode == "Admin", Ct);
            var second = f.Account("Admin2", adminRole.RoleId, hash); db.Accounts.Add(second); await db.SaveChangesAsync(Ct); f.Accounts.Add("Admin2", second.AccountId);
            db.StudentProfiles.Add(new StudentProfile { AccountId = f.Accounts["Student"], DateOfBirth = new(2000, 1, 1) }); await db.SaveChangesAsync(Ct);
            return f;
        }
        catch { await f.DisposeAsync(); throw; }
    }
    private Account Account(string role, int roleId, string hash) => new() { RoleId = roleId, FullName = "Synthetic " + role,
        Email = role.ToLowerInvariant() + "@example.invalid", PasswordHash = hash, Status = "ACTIVE", SecurityVersion = 1, CreatedAt = Clock.GetUtcNow().UtcDateTime };
    internal async Task<LoginGrant> Grant(string role, string? password = null)
    {
        var result = await Login.ExecuteAsync(LoginInput.Parse(JsonSerializer.Serialize(new { email = role.ToLowerInvariant() + "@example.invalid", password = password ?? Password })), Ct);
        Assert.True(result.Accepted, "Synthetic login must succeed."); return result.Grant!;
    }
    internal VerifiedSessionToken Token(LoginGrant grant) => Validator.Validate(grant.CookieToken) ?? throw new InvalidOperationException("Synthetic issued token must validate.");
    internal StudentSignupInput SignupInput() => StudentSignupInput.Parse(JsonSerializer.Serialize(new {
        fullName = "Synthetic Signup", dateOfBirth = "2000-01-01", email = "signup@example.invalid", phone = "0912345678", password = Password, address = (string?)null }));
    internal string NextPassword() => new TemporaryPasswordGenerator(new PasswordPolicy(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist")))).Generate();
    internal PasswordChangeInput ChangeInput(LoginGrant grant, string next) => PasswordChangeInput.Parse(JsonSerializer.Serialize(new {
        currentPassword = Password, newPassword = next, expectedVersions = new { account = grant.Context.AccountVersion } }));
    internal static AdminLockInput LockInput(int target, string version, Guid request) => AdminLockInput.Parse(target, request,
        JsonSerializer.Serialize(new { expectedVersions = new { account = version } }), 4096);
    internal async Task Restricted(string role = "Student")
    {
        await using var db = Db(); var account = await db.Accounts.SingleAsync(a => a.AccountId == Accounts[role], Ct);
        account.MustChangePassword = true; account.TempIssuedAt = Clock.GetUtcNow().UtcDateTime; account.TempExpiresAt = Clock.GetUtcNow().AddHours(24).UtcDateTime;
        await db.SaveChangesAsync(Ct);
    }
    internal async Task<AuditGate> HoldAudit()
    {
        var connection = new SqlConnection(Connection);
        try
        {
            await connection.OpenAsync(Ct); var tx = (SqlTransaction)await connection.BeginTransactionAsync(Ct);
            var gate = new AuditGate(connection, tx);
            try
            {
                await using var command = connection.CreateCommand(); command.Transaction = tx; command.CommandTimeout = 5;
                command.CommandText = "SELECT COUNT(*) FROM dbo.AuditLog WITH(TABLOCKX,HOLDLOCK);"; await command.ExecuteScalarAsync(Ct); return gate;
            }
            catch { await gate.DisposeAsync(); throw; }
        }
        catch { await connection.DisposeAsync(); throw; }
    }
    internal sealed class AuditGate(SqlConnection connection, SqlTransaction tx) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        { try { await tx.RollbackAsync(CancellationToken.None); } finally { await tx.DisposeAsync(); await connection.DisposeAsync(); } }
    }
    private void ValidateName()
    {
        if (_name.Length != Prefix.Length + 32 || !_name.StartsWith(Prefix, StringComparison.Ordinal) || !Guid.TryParseExact(_name[Prefix.Length..], "N", out _))
            throw new InvalidOperationException("Invalid scratch identifier.");
    }
    private async Task<(int Id, Guid Guid)> Identity(SqlConnection master)
    {
        await using var cmd = master.CreateCommand(); cmd.CommandTimeout = 10;
        cmd.CommandText = "SELECT d.database_id,r.database_guid FROM sys.databases d JOIN sys.database_recovery_status r ON r.database_id=d.database_id WHERE d.name=@name;";
        cmd.Parameters.AddWithValue("@name", _name); await using var reader = await cmd.ExecuteReaderAsync(CancellationToken.None);
        if (!await reader.ReadAsync(CancellationToken.None) || reader.IsDBNull(1)) throw new InvalidOperationException("Scratch ownership identity unavailable.");
        return (reader.GetInt32(0), reader.GetGuid(1));
    }
    private Task Receipt(string state)
    {
        var directory = Environment.GetEnvironmentVariable("EC_V01_E_RESULTS") ?? throw new InvalidOperationException("Ignored ownership receipt directory required.");
        Directory.CreateDirectory(directory);
        return File.AppendAllTextAsync(Path.Combine(directory, "E-scratch-ownership.jsonl"), JsonSerializer.Serialize(new { name = _name, databaseId = _databaseId, databaseGuid = _databaseGuid, state }) + Environment.NewLine);
    }
    public async ValueTask DisposeAsync()
    {
        if (!_created) return; ValidateName();
        if (_databaseId <= 4 || _databaseGuid == Guid.Empty) throw new InvalidOperationException("Refuse cleanup without captured ownership.");
        await using var master = new SqlConnection(Master); await master.OpenAsync(CancellationToken.None);
        var actual = await Identity(master);
        if (actual != (_databaseId, _databaseGuid)) throw new InvalidOperationException("Refuse scratch ownership mismatch.");
        // Pooling is disabled for every fixture connection, and all scopes dispose before this finally.
        await using var drop = master.CreateCommand(); drop.CommandTimeout = 30;
        drop.CommandText = $"IF NOT EXISTS(SELECT 1 FROM sys.databases d JOIN sys.database_recovery_status r ON r.database_id=d.database_id WHERE d.name=@name AND d.database_id=@id AND r.database_guid=@guid) THROW 51000,'Ownership mismatch',1; DROP DATABASE [{_name}];";
        drop.Parameters.AddWithValue("@name", _name); drop.Parameters.AddWithValue("@id", _databaseId); drop.Parameters.AddWithValue("@guid", _databaseGuid);
        await drop.ExecuteNonQueryAsync(CancellationToken.None); _created = false; await Receipt("dropped");
    }
}

internal sealed class OwnedDetailTrace(Func<Task>? beforeFinal = null) : DbConnectionInterceptor
{
    internal readonly List<(Guid Context, IsolationLevel Isolation)> Transactions = [];
    private int _opens;
    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData data, CancellationToken ct = default)
    { if (++_opens == 3 && beforeFinal is not null) await beforeFinal(); }
    internal DbTransactionInterceptor TransactionObserver => new Observer(this);
    private sealed class Observer(OwnedDetailTrace trace) : DbTransactionInterceptor
    {
        public override ValueTask<DbTransaction> TransactionStartedAsync(DbConnection connection, TransactionEndEventData data, DbTransaction result, CancellationToken ct = default)
        { trace.Transactions.Add((data.Context!.ContextId.InstanceId, result.IsolationLevel)); return new(result); }
    }
}

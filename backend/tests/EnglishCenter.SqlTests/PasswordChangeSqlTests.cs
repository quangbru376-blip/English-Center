namespace EnglishCenter.SqlTests;

using System.Security.Cryptography;
using System.Text.Json;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Auth;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public class PasswordChangeSqlTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private sealed class Setup
    {
        internal readonly AuthTestSupport.Clock Time = new(new(SqlFixture.Instant));
        internal BusinessClock Clock => new(Time);
        internal readonly SecurityKey Key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
        internal string Current = AuthTestSupport.Password(), Next = AuthTestSupport.Password();
        internal VerifiedSessionToken Proof = null!;
        internal int OtherAccountId;
        internal async Task Initialize(SqlFixture f, bool restricted = false)
        {
            f.Teacher.PasswordHash = AuthTestSupport.Hasher().Hash(Current);
            if (restricted) { f.Teacher.MustChangePassword = true; f.Teacher.TempIssuedAt = SqlFixture.Instant.AddMinutes(20).AddHours(-24); f.Teacher.TempExpiresAt = SqlFixture.Instant.AddMinutes(20); }
            var other = new Account { RoleId = f.Teacher.RoleId, Email = "other-password-fixture@example.invalid", FullName = "Synthetic other", PasswordHash = AuthTestSupport.Hasher().Hash(AuthTestSupport.Password()), Status = "ACTIVE", SecurityVersion = 1 };
            f.Context.Accounts.Add(other); await f.Context.SaveChangesAsync(Ct); OtherAccountId = other.AccountId;
            var window = restricted ? SessionIssuanceWindow.Restricted(Clock.GetUtcNow(), Clock.GetUtcNow().AddMinutes(20)) : SessionIssuanceWindow.Normal(Clock.GetUtcNow());
            var sid = RandomNumberGenerator.GetBytes(32);
            f.Context.AuthSessions.Add(new() { Sid = sid, AccountId = f.Teacher.AccountId, SecurityVersion = f.Teacher.SecurityVersion, SessionScope = window.Scope, IssuedAt = SqlFixture.Instant, ExpiresAt = window.AbsoluteExpiresAt.UtcDateTime, LastActivityAt = SqlFixture.Instant });
            f.Context.AuthSessions.Add(new() { Sid = RandomNumberGenerator.GetBytes(32), AccountId = f.Teacher.AccountId, SecurityVersion = f.Teacher.SecurityVersion, SessionScope = window.Scope, IssuedAt = SqlFixture.Instant, ExpiresAt = window.AbsoluteExpiresAt.UtcDateTime, LastActivityAt = SqlFixture.Instant });
            f.Context.AuthSessions.Add(new() { Sid = RandomNumberGenerator.GetBytes(32), AccountId = OtherAccountId, SecurityVersion = 1, SessionScope = "NORMAL", IssuedAt = SqlFixture.Instant, ExpiresAt = SqlFixture.Instant.AddHours(2), LastActivityAt = SqlFixture.Instant });
            await f.Context.SaveChangesAsync(Ct);
            var jwt = new JwtSessionIssuer(new(Key, "HS256"), "fixture", "fixture").Issue(f.Teacher.AccountId, f.Teacher.SecurityVersion, sid, window);
            Proof = new JwtSessionValidator([Key], ["HS256"], "fixture", "fixture", Clock, 4096).Validate(jwt)!;
        }
        internal PasswordChangeInput Input(SqlFixture f, string? current = null, string? next = null, string? rv = null) => PasswordChangeInput.Parse(JsonSerializer.Serialize(new { currentPassword = current ?? Current, newPassword = next ?? Next, expectedVersions = new { account = rv ?? RowVersionCodec.Encode(f.Teacher.RowVersion) } }));
        internal PasswordChangeService Service(SqlAuthDeadlineScope scope) => new(new SqlPasswordChangeRepository(scope, new()), AuthTestSupport.Hasher(), Clock);
    }
    [Theory]
    [InlineData("Teacher", false)]
    [InlineData("Student", false)]
    [InlineData("Admin", false)]
    [InlineData("Admissions", false)]
    [InlineData("AcademicAffairs", false)]
    [InlineData("Accountant", false)]
    [InlineData("Director", false)]
    [InlineData("Teacher", true)]
    public async Task AtomicHashVersionTemporaryClearAllOwnSessionsAndAudit(string role, bool restricted)
    {
        await using var f = await SqlFixture.CreateAsync(); var setup = new Setup(); await setup.Initialize(f, restricted);
        if (!await f.Context.Roles.AnyAsync(r => r.RoleCode == role, Ct)) { f.Context.Roles.Add(new Role { RoleCode = role, RoleName = role }); await f.Context.SaveChangesAsync(Ct); }
        f.Teacher.RoleId = f.Context.Roles.Local.Single(r => r.RoleCode == role).RoleId; await f.Context.SaveChangesAsync(Ct);
        var before = f.Teacher.RowVersion.ToArray(); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, Ct);
        await setup.Service(scope).ChangeAsync(setup.Proof, setup.Input(f), Ct);
        var account = await f.Context.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == f.Teacher.AccountId, Ct);
        Assert.Equal(2, account.SecurityVersion); Assert.False(account.MustChangePassword); Assert.Null(account.TempIssuedAt); Assert.Null(account.TempExpiresAt); Assert.Equal("ACTIVE", account.Status); Assert.False(before.SequenceEqual(account.RowVersion));
        Assert.NotEqual(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed, AuthTestSupport.Hasher().Verify(account.PasswordHash, setup.Next)); Assert.Equal(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed, AuthTestSupport.Hasher().Verify(account.PasswordHash, setup.Current));
        var sessions = await f.Context.AuthSessions.AsNoTracking().ToListAsync(Ct); Assert.All(sessions.Where(s => s.AccountId == account.AccountId), s => Assert.NotNull(s.RevokedAt)); Assert.Null(sessions.Single(s => s.AccountId == setup.OtherAccountId).RevokedAt); Assert.All(sessions, s => Assert.Equal(SqlFixture.Instant, s.LastActivityAt));
        var audit = await f.Context.AuditLogs.AsNoTracking().SingleAsync(Ct); Assert.Equal("AUTH.PASSWORD_CHANGE", audit.Action); Assert.Equal("MANDATORY", audit.AuditCategory); Assert.Equal(account.AccountId, audit.ActorAccountId); Assert.DoesNotContain(setup.Current, audit.Details!); Assert.DoesNotContain(setup.Next, audit.Details!); Assert.DoesNotContain(account.PasswordHash, audit.Details!);
        var service = new CurrentSessionService(new SqlCurrentSessionRepository(scope), setup.Clock); Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, (await Assert.ThrowsAsync<ApiErrorException>(() => service.ReadAsync(setup.Proof, false, Ct))).Code);
        await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Theory]
    [InlineData("wrong", ApiErrorCode.VALIDATION_ERROR)]
    [InlineData("policy", ApiErrorCode.VALIDATION_ERROR)]
    [InlineData("stale", ApiErrorCode.CONFLICT)]
    [InlineData("locked", ApiErrorCode.AUTHENTICATION_REQUIRED)]
    [InlineData("version", ApiErrorCode.AUTHENTICATION_REQUIRED)]
    [InlineData("revoked", ApiErrorCode.AUTHENTICATION_REQUIRED)]
    [InlineData("idle", ApiErrorCode.AUTHENTICATION_REQUIRED)]
    [InlineData("absolute", ApiErrorCode.AUTHENTICATION_REQUIRED)]
    [InlineData("restricted", ApiErrorCode.AUTHENTICATION_REQUIRED)]
    public async Task InvalidCurrentFactsOrInputMakeNoMutation(string fault, ApiErrorCode code)
    {
        await using var f = await SqlFixture.CreateAsync(); var setup = new Setup(); await setup.Initialize(f, fault == "restricted");
        var input = setup.Input(f, fault == "wrong" ? "wrong password" : null, fault == "policy" ? File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist", "10k-most-common.txt")).First(s => s.Length >= 15) : null, fault == "stale" ? "AAECAwQFBgc=" : null);
        if (fault == "locked") await f.ExecuteAsync($"UPDATE dbo.Account SET status='LOCKED' WHERE account_id={f.Teacher.AccountId};");
        if (fault == "version") await f.ExecuteAsync($"UPDATE dbo.Account SET security_version=2 WHERE account_id={f.Teacher.AccountId};");
        if (fault == "revoked") await f.ExecuteAsync($"UPDATE dbo.AuthSession SET revoked_at={SqlFixture.Instant} WHERE sid={setup.Proof.CopySid()};");
        if (fault is "idle" or "absolute" or "restricted") setup.Time.Now = setup.Time.Now.AddMinutes(fault == "idle" ? 30 : fault == "restricted" ? 10 : 120);
        var hash = f.Teacher.PasswordHash; await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, Ct);
        Assert.Equal(code, (await Assert.ThrowsAsync<ApiErrorException>(() => setup.Service(scope).ChangeAsync(setup.Proof, input, Ct))).Code);
        Assert.Equal(hash, (await f.Context.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == f.Teacher.AccountId, Ct)).PasswordHash); Assert.Equal(0, await f.Context.AuditLogs.CountAsync(Ct)); await AuthTestSupport.EmptyAfterRollback(f);
    }
    private sealed class FaultAfterSave(SqlPasswordChangeRepository inner, bool cancel) : IPasswordChangeRepository
    {
        public Task<Account?> LockAccountAsync(int id, CancellationToken ct) => inner.LockAccountAsync(id, ct);
        public Task<AuthSession?> LockSessionAsync(byte[] sid, CancellationToken ct) => inner.LockSessionAsync(sid, ct);
        public Task<string?> RoleCodeAsync(int id, CancellationToken ct) => inner.RoleCodeAsync(id, ct);
        public Task SaveSessionAsync(CancellationToken ct) => inner.SaveSessionAsync(ct);
        public Task<IReadOnlyList<AuthSession>> LockAllSessionsAsync(int id, CancellationToken ct) => inner.LockAllSessionsAsync(id, ct);
        public async Task SaveChangeAsync(AuditEntry e, CancellationToken ct) { await inner.SaveChangeAsync(e, ct); if (cancel) throw new OperationCanceledException(ct); throw new MandatoryAuditException(); }
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FailureAfterActualSqlSaveAndAuditStillRollsBackAll(bool cancel)
    {
        await using var f = await SqlFixture.CreateAsync(); var setup = new Setup(); await setup.Initialize(f); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, Ct);
        var service = new PasswordChangeService(new FaultAfterSave(new(scope, new()), cancel), AuthTestSupport.Hasher(), setup.Clock);
        if (cancel) await Assert.ThrowsAsync<OperationCanceledException>(() => service.ChangeAsync(setup.Proof, setup.Input(f), Ct)); else await Assert.ThrowsAsync<MandatoryAuditException>(() => service.ChangeAsync(setup.Proof, setup.Input(f), Ct));
        Assert.Equal(1, await f.Context.AuditLogs.CountAsync(Ct)); Assert.Equal(2, (await f.Context.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == f.Teacher.AccountId, Ct)).SecurityVersion); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Theory]
    [InlineData("AuthSession")]
    [InlineData("Account")]
    [InlineData("AuditLog")]
    public async Task RuntimeDeniedMutationOrMandatoryAuditCannotComplete(string table)
    {
        await using var f = await SqlFixture.CreateAsync(); var setup = new Setup(); await setup.Initialize(f);
        await f.ExecuteAsync($"CREATE USER D12_fixture_user_0 WITHOUT LOGIN; ALTER ROLE ec_app_runtime ADD MEMBER D12_fixture_user_0;");
        if (table == "AuditLog") await f.ExecuteAsync($"DENY INSERT ON dbo.AuditLog TO D12_fixture_user_0;"); else if (table == "Account") await f.ExecuteAsync($"DENY UPDATE ON dbo.Account TO D12_fixture_user_0;"); else await f.ExecuteAsync($"DENY UPDATE ON dbo.AuthSession TO D12_fixture_user_0;");
        await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, Ct); await f.ExecuteAsync($"EXECUTE AS USER='D12_fixture_user_0';");
        try { await Assert.ThrowsAnyAsync<Exception>(() => setup.Service(scope).ChangeAsync(setup.Proof, setup.Input(f), Ct)); }
        finally { await f.ExecuteAsync($"REVERT;"); }
        await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Fact]
    public async Task SecurityVersionMaximumFailsClosedWithNoAccountOrSessionEffects()
    {
        await using var f = await SqlFixture.CreateAsync(); f.Teacher.SecurityVersion = long.MaxValue; await f.Context.SaveChangesAsync(Ct);
        var setup = new Setup(); await setup.Initialize(f); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, Ct);
        Assert.Equal(ApiErrorCode.STATE_CONFLICT, (await Assert.ThrowsAsync<ApiErrorException>(() => setup.Service(scope).ChangeAsync(setup.Proof, setup.Input(f), Ct))).Code);
        Assert.Equal(long.MaxValue, (await f.Context.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == f.Teacher.AccountId, Ct)).SecurityVersion); Assert.All(await f.Context.AuthSessions.AsNoTracking().ToListAsync(Ct), s => Assert.Null(s.RevokedAt)); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Fact]
    public async Task HeldAccountAndEveryOwnSidPreventConcurrentSessionOrPasswordChanges()
    {
        await using var f = await SqlFixture.CreateAsync(); var setup = new Setup(); await setup.Initialize(f); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, Ct);
        var repo = new SqlPasswordChangeRepository(scope, new()); await repo.LockAccountAsync(f.Teacher.AccountId, Ct); await repo.LockAllSessionsAsync(f.Teacher.AccountId, Ct);
        await using var other = new EnglishCenter.Api.Infrastructure.EnglishCenterDbContext(new DbContextOptionsBuilder<EnglishCenter.Api.Infrastructure.EnglishCenterDbContext>().UseSqlServer(f.Context.Database.GetConnectionString(), sql => sql.UseCompatibilityLevel(160)).Options);
        await other.Database.OpenConnectionAsync(Ct); await using var tx = await other.Database.BeginTransactionAsync(Ct); await using var otherScope = await SqlAuthDeadlineScope.EnterAsync(other, 10, 100, Ct); var competing = new SqlPasswordChangeRepository(otherScope, new());
        Assert.Equal(1222, (await Assert.ThrowsAsync<SqlException>(() => competing.LockAccountAsync(f.Teacher.AccountId, Ct))).Number); Assert.Equal(1222, (await Assert.ThrowsAsync<SqlException>(() => competing.LockAllSessionsAsync(f.Teacher.AccountId, Ct))).Number);
        await tx.RollbackAsync(Ct); Assert.Equal(0, await f.Context.AuditLogs.CountAsync(Ct)); await AuthTestSupport.EmptyAfterRollback(f);
    }

}

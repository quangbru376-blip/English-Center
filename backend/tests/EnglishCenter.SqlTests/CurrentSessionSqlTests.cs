namespace EnglishCenter.SqlTests;

using System.Security.Cryptography;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure;
using EnglishCenter.Api.Infrastructure.Auth;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Auth;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Xunit;

public class CurrentSessionSqlTests
{
    private sealed class Proof
    {
        internal readonly AuthTestSupport.Clock Time = new(new(SqlFixture.Instant));
        internal BusinessClock Clock => new(Time);
        private readonly SecurityKey _key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
        internal async Task<VerifiedSessionToken> Add(SqlFixture fixture, bool restricted = false)
        {
            if (restricted)
            { fixture.Teacher.MustChangePassword = true; fixture.Teacher.TempIssuedAt = SqlFixture.Instant.AddMinutes(20).AddHours(-24); fixture.Teacher.TempExpiresAt = SqlFixture.Instant.AddMinutes(20); await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken); }
            var window = restricted ? SessionIssuanceWindow.Restricted(Clock.GetUtcNow(), new DateTimeOffset(fixture.Teacher.TempExpiresAt!.Value)) : SessionIssuanceWindow.Normal(Clock.GetUtcNow());
            var sid = RandomNumberGenerator.GetBytes(32); fixture.Context.AuthSessions.Add(new() { Sid = sid, AccountId = fixture.Teacher.AccountId, SessionScope = window.Scope, SecurityVersion = fixture.Teacher.SecurityVersion, IssuedAt = window.IssuedAt.UtcDateTime, ExpiresAt = window.AbsoluteExpiresAt.UtcDateTime, LastActivityAt = window.IssuedAt.UtcDateTime });
            await fixture.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
            return Token(fixture.Teacher.AccountId, fixture.Teacher.SecurityVersion, sid, window);
        }
        internal VerifiedSessionToken Token(int id, long version, byte[] sid, SessionIssuanceWindow window)
        {
            var encoded = new JwtSessionIssuer(new(_key, "HS256"), "fixture", "fixture").Issue(id, version, sid, window);
            var verified = new JwtSessionValidator([_key], ["HS256"], "fixture", "fixture", Clock, 4096).Validate(encoded); Assert.NotNull(verified); return verified;
        }
    }
    private static CurrentSessionService Service(SqlAuthDeadlineScope scope, Proof proof) => new(new SqlCurrentSessionRepository(scope), proof.Clock);
    [Theory]
    [InlineData("Admin")]
    [InlineData("Admissions")]
    [InlineData("AcademicAffairs")]
    [InlineData("Teacher")]
    [InlineData("Accountant")]
    [InlineData("Director")]
    [InlineData("Student")]
    public async Task SevenRolesReadFreshCurrentContactRoleAndVersionWithoutAnySessionWrite(string role)
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var token = await p.Add(f); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken);
        var session = await f.Context.AuthSessions.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken); var old = session.RowVersion.ToArray();
        await f.ExecuteAsync($"UPDATE dbo.Role SET role_code={role} WHERE role_id={f.Teacher.RoleId}; UPDATE dbo.Account SET full_name={"Synthetic current name"} WHERE account_id={f.Teacher.AccountId};");
        var current = await Service(scope, p).ReadAsync(token, false, TestContext.Current.CancellationToken); Assert.Equal(role, current.Context.Role); Assert.Equal("Synthetic current name", current.Context.FullName); Assert.Equal(Convert.ToBase64String(f.Teacher.RowVersion), current.Context.AccountVersion); Assert.Equal("2026-10-05T00:30:00.000Z", current.Context.EffectiveExpiresAt);
        var after = await f.Context.AuthSessions.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken); Assert.True(old.SequenceEqual(after.RowVersion)); Assert.Equal(session.LastActivityAt, after.LastActivityAt); Assert.Null(after.RevokedAt); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Theory]
    [InlineData("LOCKED")]
    [InlineData("DISABLED")]
    [InlineData("account-version")]
    [InlineData("session-version")]
    [InlineData("revoked")]
    [InlineData("scope-mismatch")]
    [InlineData("missing-session")]
    [InlineData("missing-account")]
    public async Task FreshSqlInvalidationOverridesLoadedEntitiesAndCryptographicProof(string mode)
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var token = await p.Add(f); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken);
        if (mode is "LOCKED" or "DISABLED") await f.ExecuteAsync($"UPDATE dbo.Account SET status={mode} WHERE account_id={f.Teacher.AccountId};");
        if (mode == "account-version") await f.ExecuteAsync($"UPDATE dbo.Account SET security_version=2 WHERE account_id={f.Teacher.AccountId};");
        if (mode == "session-version") await f.ExecuteAsync($"UPDATE dbo.AuthSession SET security_version=2 WHERE sid={token.CopySid()};");
        if (mode == "revoked") await f.ExecuteAsync($"UPDATE dbo.AuthSession SET revoked_at={SqlFixture.Instant} WHERE sid={token.CopySid()};");
        if (mode == "scope-mismatch") await f.ExecuteAsync($"UPDATE dbo.AuthSession SET session_scope={"PASSWORD_CHANGE"} WHERE sid={token.CopySid()};");
        if (mode == "missing-session") token = p.Token(f.Teacher.AccountId, 1, RandomNumberGenerator.GetBytes(32), SessionIssuanceWindow.Normal(p.Clock.GetUtcNow()));
        if (mode == "missing-account") token = p.Token(f.Teacher.AccountId + 1000000, 1, token.CopySid(), SessionIssuanceWindow.Normal(p.Clock.GetUtcNow()));
        var error = await Assert.ThrowsAsync<ApiErrorException>(() => Service(scope, p).ReadAsync(token, false, TestContext.Current.CancellationToken)); Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, error.Code); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Theory]
    [InlineData(30, false)]
    [InlineData(120, true)]
    public async Task RawEqualityAtIdleOrAbsoluteDeadlineCannotReturnContextOrResurrectActivity(int minutes, bool touchEarlier)
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var token = await p.Add(f); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken); var service = Service(scope, p);
        if (touchEarlier) await f.ExecuteAsync($"UPDATE dbo.AuthSession SET last_activity_at={SqlFixture.Instant.AddMinutes(110)} WHERE sid={token.CopySid()};");
        p.Time.Now = new DateTimeOffset(SqlFixture.Instant).AddMinutes(minutes).AddTicks(-1); Assert.NotNull(await service.ReadAsync(token, false, TestContext.Current.CancellationToken)); p.Time.Now = p.Time.Now.AddTicks(1);
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, (await Assert.ThrowsAsync<ApiErrorException>(() => service.ReadAsync(token, false, TestContext.Current.CancellationToken))).Code);
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, (await Assert.ThrowsAsync<ApiErrorException>(() => service.TouchApprovedInteractiveActivityAsync(token, TestContext.Current.CancellationToken))).Code);
        var session = await f.Context.AuthSessions.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken); Assert.Equal(touchEarlier ? SqlFixture.Instant.AddMinutes(110) : SqlFixture.Instant, session.LastActivityAt); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Fact]
    public async Task AccountSidRelationAndMustChangeStateCannotBeSelectedOrUpgradedByToken()
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var token = await p.Add(f); var other = new Account { RoleId = f.Teacher.RoleId, Email = "other@example.invalid", FullName = "Synthetic other", PasswordHash = AuthTestSupport.Hasher().Hash(AuthTestSupport.Password()), Status = "ACTIVE", SecurityVersion = 1, CreatedAt = SqlFixture.Instant }; f.Context.Accounts.Add(other); await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken); var selector = p.Token(other.AccountId, 1, token.CopySid(), SessionIssuanceWindow.Normal(p.Clock.GetUtcNow())); Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, (await Assert.ThrowsAsync<ApiErrorException>(() => Service(scope, p).ReadAsync(selector, false, TestContext.Current.CancellationToken))).Code);
        await f.ExecuteAsync($"UPDATE dbo.Account SET must_change_password=1,temp_issued_at={SqlFixture.Instant.AddHours(-23)},temp_expires_at={SqlFixture.Instant.AddHours(1)} WHERE account_id={f.Teacher.AccountId};"); Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, (await Assert.ThrowsAsync<ApiErrorException>(() => Service(scope, p).ReadAsync(token, false, TestContext.Current.CancellationToken))).Code); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Fact]
    public async Task RestrictedContextAndLogoutAllowedButBusinessAndActivityForbiddenWithNoUpgrade()
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var token = await p.Add(f, true); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken); var service = Service(scope, p);
        var current = await service.ReadAsync(token, false, TestContext.Current.CancellationToken); Assert.Equal("PASSWORD_CHANGE", current.Context.Scope);
        Assert.Equal(ApiErrorCode.FORBIDDEN, (await Assert.ThrowsAsync<ApiErrorException>(() => service.ReadAsync(token, true, TestContext.Current.CancellationToken))).Code); Assert.Equal(ApiErrorCode.FORBIDDEN, (await Assert.ThrowsAsync<ApiErrorException>(() => service.TouchApprovedInteractiveActivityAsync(token, TestContext.Current.CancellationToken))).Code);
        await f.ExecuteAsync($"UPDATE dbo.Account SET must_change_password=0,temp_issued_at=NULL,temp_expires_at=NULL WHERE account_id={f.Teacher.AccountId};"); Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, (await Assert.ThrowsAsync<ApiErrorException>(() => service.ReadAsync(token, false, TestContext.Current.CancellationToken))).Code); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RestrictedDeadlineCapsTenMinutesAndRemainingTemporaryLifetime(bool shorten)
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var token = await p.Add(f, true); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken);
        if (shorten) await f.ExecuteAsync($"UPDATE dbo.Account SET temp_issued_at={SqlFixture.Instant.AddMinutes(5).AddHours(-24)},temp_expires_at={SqlFixture.Instant.AddMinutes(5)} WHERE account_id={f.Teacher.AccountId};");
        p.Time.Now = new DateTimeOffset(SqlFixture.Instant).AddMinutes(shorten ? 5 : 10).AddTicks(-1); var current = await Service(scope, p).ReadAsync(token, false, TestContext.Current.CancellationToken); Assert.Equal(p.Time.Now.AddTicks(1), current.EffectiveDeadline);
        p.Time.Now = p.Time.Now.AddTicks(1); Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, (await Assert.ThrowsAsync<ApiErrorException>(() => Service(scope, p).RevokeCurrentAsync(token, TestContext.Current.CancellationToken))).Code); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Fact]
    public async Task TemporaryBundleCannotExtendTwentyFourHourCredentialLimit()
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var token = await p.Add(f, true); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken);
        var error = await Assert.ThrowsAsync<SqlException>(() => f.ExecuteAsync($"UPDATE dbo.Account SET temp_issued_at={SqlFixture.Instant.AddHours(-24).AddMinutes(5)},temp_expires_at={SqlFixture.Instant.AddMinutes(20)} WHERE account_id={f.Teacher.AccountId};")); Assert.Equal(547, error.Number);
        var current = await Service(scope, p).ReadAsync(token, false, TestContext.Current.CancellationToken); Assert.Equal(new DateTimeOffset(SqlFixture.Instant).AddMinutes(10), current.EffectiveDeadline); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Fact]
    public async Task ApprovedInteractiveActivityExtendsOnlyIdleWhileMeAndThrottleDoNotWrite()
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var token = await p.Add(f); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken); var service = Service(scope, p);
        await f.ExecuteAsync($"UPDATE dbo.Account SET failed_login_count=5,failure_window_started_at={SqlFixture.Instant},login_throttled_until={SqlFixture.Instant.AddMinutes(15)} WHERE account_id={f.Teacher.AccountId};"); Assert.NotNull(await service.ReadAsync(token, false, TestContext.Current.CancellationToken));
        p.Time.Now = new DateTimeOffset(SqlFixture.Instant).AddMinutes(20); await service.TouchApprovedInteractiveActivityAsync(token, TestContext.Current.CancellationToken); p.Time.Now = p.Time.Now.AddMinutes(20); var current = await service.ReadAsync(token, false, TestContext.Current.CancellationToken); Assert.Equal("2026-10-05T00:50:00.000Z", current.Context.EffectiveExpiresAt);
        var session = await f.Context.AuthSessions.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken); Assert.Equal(SqlFixture.Instant.AddMinutes(20), session.LastActivityAt); Assert.Equal(SqlFixture.Instant.AddHours(2), session.ExpiresAt); Assert.Equal(1, session.SecurityVersion); Assert.Null(session.RevokedAt); Assert.Equal(5, f.Teacher.FailedLoginCount); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LogoutRevokesOnlyCurrentSidAndOtherDeviceRemainsValid(bool restricted)
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var current = await p.Add(f, restricted); var other = await p.Add(f, restricted); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken); var service = Service(scope, p);
        var proof = await service.RevokeCurrentAsync(current, TestContext.Current.CancellationToken); Assert.Equal(f.Teacher.AccountId, proof.VerifiedActor); Assert.NotNull(await service.ReadAsync(other, false, TestContext.Current.CancellationToken)); Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, (await Assert.ThrowsAsync<ApiErrorException>(() => service.RevokeCurrentAsync(current, TestContext.Current.CancellationToken))).Code);
        var sessions = await f.Context.AuthSessions.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken); Assert.Equal(1, sessions.Count(s => s.RevokedAt is not null)); Assert.All(sessions, s => Assert.Equal(SqlFixture.Instant, s.LastActivityAt)); Assert.Equal(1, f.Teacher.SecurityVersion); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Fact]
    public async Task RealRuntimeDeniedSessionUpdateCannotFakeLogoutAndEverythingRollsBack()
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var token = await p.Add(f); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken);
        await SqlRows.ReadAsync(f, "IF DATABASE_PRINCIPAL_ID('D12_fixture_user_0') IS NOT NULL THROW 51912,'Fixture exists',1; CREATE USER D12_fixture_user_0 WITHOUT LOGIN; ALTER ROLE ec_app_runtime ADD MEMBER D12_fixture_user_0; DENY UPDATE ON dbo.AuthSession TO ec_app_runtime; EXECUTE AS USER='D12_fixture_user_0';");
        try { await Assert.ThrowsAsync<DbUpdateException>(() => Service(scope, p).RevokeCurrentAsync(token, TestContext.Current.CancellationToken)); } finally { await SqlRows.ReadAsync(f, "REVERT;"); }
        var session = await f.Context.AuthSessions.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken); Assert.Null(session.RevokedAt); Assert.Equal(SqlFixture.Instant, session.LastActivityAt); await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FaultOrCancellationAfterRealSessionUpdateStillRollsBackWholeCallerUnit(bool cancel)
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var token = await p.Add(f); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken); var service = new CurrentSessionService(new FaultAfterSave(new SqlCurrentSessionRepository(scope), cancel), p.Clock);
        if (cancel) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RevokeCurrentAsync(token, TestContext.Current.CancellationToken)); else await Assert.ThrowsAsync<InvalidOperationException>(() => service.RevokeCurrentAsync(token, TestContext.Current.CancellationToken));
        var stored = await f.Context.AuthSessions.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken); Assert.NotNull(stored.RevokedAt);
        await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Fact]
    public async Task CompetingConnectionCannotBypassHeldAccountThenSessionLocks()
    {
        await using var f = await SqlFixture.CreateAsync(); var p = new Proof(); var token = await p.Add(f); await using var scope = await SqlAuthDeadlineScope.EnterAsync(f.Context, 10, 200, TestContext.Current.CancellationToken); Assert.NotNull(await Service(scope, p).ReadAsync(token, false, TestContext.Current.CancellationToken));
        await using var other = new EnglishCenterDbContext(new DbContextOptionsBuilder<EnglishCenterDbContext>().UseSqlServer(f.Context.Database.GetConnectionString(), sql => sql.UseCompatibilityLevel(160)).Options); await other.Database.OpenConnectionAsync(TestContext.Current.CancellationToken); await using var tx = await other.Database.BeginTransactionAsync(TestContext.Current.CancellationToken); await using var competing = await SqlAuthDeadlineScope.EnterAsync(other, 10, 100, TestContext.Current.CancellationToken);
        var error = await Assert.ThrowsAsync<SqlException>(() => new SqlCurrentSessionRepository(competing).LockAccountAsync(f.Teacher.AccountId, TestContext.Current.CancellationToken)); Assert.Equal(1222, error.Number);
        var sessionError = await Assert.ThrowsAsync<SqlException>(() => new SqlCurrentSessionRepository(competing).LockSessionAsync(token.CopySid(), TestContext.Current.CancellationToken)); Assert.Equal(1222, sessionError.Number); Assert.Null((await f.Context.AuthSessions.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken)).RevokedAt);
    }
    private sealed class FaultAfterSave(ICurrentSessionRepository inner, bool cancel) : ICurrentSessionRepository
    {
        public Task<Account?> LockAccountAsync(int id, CancellationToken ct) => inner.LockAccountAsync(id, ct);
        public Task<AuthSession?> LockSessionAsync(byte[] sid, CancellationToken ct) => inner.LockSessionAsync(sid, ct);
        public Task<string?> RoleCodeAsync(int id, CancellationToken ct) => inner.RoleCodeAsync(id, ct);
        public async Task SaveSessionAsync(CancellationToken ct)
        {
            await inner.SaveSessionAsync(ct);
            if (cancel) throw new OperationCanceledException();
            throw new InvalidOperationException("Synthetic after-write fault.");
        }
    }
}

namespace EnglishCenter.Tests.Api.Integration;

using System.Net;
using EnglishCenter.Api.Common.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class IdentitySqlHostingTests
{
    private static CancellationToken Ct => IntegrationSqlHost.Ct;
    private const string OptIn = "V01 HTTP/SQL requires EC_V01_SQL=1 and approved isolated rollback scope.";
    [Theory(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    [InlineData("Student")][InlineData("Admissions")][InlineData("AcademicAffairs")][InlineData("Teacher")]
    [InlineData("Accountant")][InlineData("Director")][InlineData("Admin")]
    public async Task LoginMeLogout_UsesFreshSqlForEachRole(string role)
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var client = f.Client();
        var login = await f.Login(client, role); IdentitySqlAssertions.Context(login, f.Accounts[role], role, "NORMAL");
        var before = await f.Db.AuthSessions.AsNoTracking().SingleAsync(Ct); f.Advance(TimeSpan.FromMinutes(2));
        using var me = await f.Send(client, HttpMethod.Get, "/api/v1/auth/me"); Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        IdentitySqlAssertions.Context(await IntegrationSqlHost.Json(me), f.Accounts[role], role, "NORMAL");
        var after = await f.Db.AuthSessions.AsNoTracking().SingleAsync(Ct); Assert.Equal(before.LastActivityAt, after.LastActivityAt);
        using var logout = await f.Send(client, HttpMethod.Post, "/api/v1/auth/logout", csrf: await f.Csrf(client));
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.NotNull((await f.Db.AuthSessions.AsNoTracking().SingleAsync(Ct)).RevokedAt);
        using var rejected = await f.Send(client, HttpMethod.Get, "/api/v1/auth/me"); Assert.Equal(HttpStatusCode.Unauthorized, rejected.StatusCode);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task Signup_CreatesAccountProfileAndAuditWithoutSessionOrAutoLogin()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var client = f.Client();
        using var response = await f.Send(client, HttpMethod.Post, "/api/v1/auth/register",
            new { fullName = "Synthetic signup", dateOfBirth = "2000-01-01", email = "new@example.invalid", phone = "0912345678", password = f.Password }, await f.Csrf(client));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.False(response.Headers.TryGetValues("Set-Cookie", out var cookies) && cookies.Any(c => c.StartsWith(SafeCookiePolicy.AuthCookieName + "=", StringComparison.Ordinal)));
        Assert.Equal(8, await f.Db.Accounts.CountAsync(Ct)); Assert.Equal(2, await f.Db.StudentProfiles.CountAsync(Ct));
        Assert.Equal(1, await f.Db.AuditLogs.CountAsync(Ct)); Assert.Equal(0, await f.Db.AuthSessions.CountAsync(Ct));
        using var me = await f.Send(client, HttpMethod.Get, "/api/v1/auth/me"); Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }
    [Theory(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    [InlineData(false)][InlineData(true)]
    public async Task PasswordChange_NormalAndRestrictedRevokesAllAndRequiresNewLogin(bool restricted)
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); if (restricted) await f.Restricted("Teacher");
        using var first = f.Client(); using var second = f.Client();
        var context = await f.Login(first, "Teacher"); await f.Login(second, "Teacher");
        IdentitySqlAssertions.Context(context, f.Accounts["Teacher"], "Teacher", restricted ? "PASSWORD_CHANGE" : "NORMAL");
        var old = await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == f.Accounts["Teacher"], Ct);
        var next = f.Password + "Z";
        using var fresh = await f.Send(first, HttpMethod.Get, "/api/v1/auth/me"); Assert.Equal(HttpStatusCode.OK, fresh.StatusCode);
        var proof = await IntegrationSqlHost.Json(fresh);
        using var response = await f.Send(first, HttpMethod.Post, "/api/v1/auth/change-password",
            new { currentPassword = f.Password, newPassword = next, expectedVersions = new { account = proof.GetProperty("accountVersion").GetString() } }, await f.Csrf(first));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updated = await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == old.AccountId, Ct);
        Assert.Equal(old.SecurityVersion + 1, updated.SecurityVersion); Assert.False(updated.MustChangePassword); Assert.Null(updated.TempExpiresAt);
        Assert.Equal(0, await f.Db.AuthSessions.CountAsync(s => s.RevokedAt == null, Ct)); Assert.Equal(1, await f.Db.AuditLogs.CountAsync(Ct));
        using var invalid = await f.Send(second, HttpMethod.Get, "/api/v1/auth/me"); Assert.Equal(HttpStatusCode.Unauthorized, invalid.StatusCode);
        using var loginClient = f.Client();
        using var login = await f.Send(loginClient, HttpMethod.Post, "/api/v1/auth/login", new { email = "teacher@example.invalid", password = next }, await f.Csrf(loginClient));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode); IdentitySqlAssertions.Context(await IntegrationSqlHost.Json(login), old.AccountId, "Teacher", "NORMAL");
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task MissingCsrfAndWrongCurrent_DoNotMutateAccount()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var client = f.Client(); var context = await f.Login(client, "Student");
        var before = await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == f.Accounts["Student"], Ct);
        var body = new { currentPassword = f.Password + "wrong", newPassword = f.Password + "Z", expectedVersions = new { account = context.GetProperty("accountVersion").GetString() } };
        using var denied = await f.Send(client, HttpMethod.Post, "/api/v1/auth/change-password", body); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using var wrong = await f.Send(client, HttpMethod.Post, "/api/v1/auth/change-password", body, await f.Csrf(client)); Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);
        var after = await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == before.AccountId, Ct);
        Assert.Equal(before.SecurityVersion, after.SecurityVersion); Assert.True(before.PasswordHash == after.PasswordHash);
        Assert.Equal(0, await f.Db.AuditLogs.CountAsync(Ct));
        using var me = await f.Send(client, HttpMethod.Get, "/api/v1/auth/me"); Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task PostWriteFault_RollsSignupBackBeforeHttpSuccess()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var client = f.Client(); var csrf = await f.Csrf(client); f.FailAfterWrite = true;
        using var response = await f.Send(client, HttpMethod.Post, "/api/v1/auth/register",
            new { fullName = "Synthetic failed signup", dateOfBirth = "2000-01-01", email = "failed@example.invalid", phone = "0912345678", password = f.Password }, csrf);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(7, await f.Db.Accounts.CountAsync(Ct)); Assert.Equal(1, await f.Db.StudentProfiles.CountAsync(Ct)); Assert.Equal(0, await f.Db.AuditLogs.CountAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task Logout_OnlyRevokesCurrentSidAndOtherProfileSurvives()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var first = f.Client(); using var second = f.Client();
        await f.Login(first, "Teacher"); await f.Login(second, "Teacher");
        using var logout = await f.Send(first, HttpMethod.Post, "/api/v1/auth/logout", csrf: await f.Csrf(first)); Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(1, await f.Db.AuthSessions.CountAsync(s => s.RevokedAt == null, Ct));
        using var me = await f.Send(second, HttpMethod.Get, "/api/v1/auth/me"); Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task StalePasswordVersion_IsRejectedWithoutRevocation()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var client = f.Client(); var proof = await f.Login(client, "Student");
        var account = await f.Db.Accounts.SingleAsync(a => a.AccountId == f.Accounts["Student"], Ct); account.FullName = "Synthetic changed name"; await f.Db.SaveChangesAsync(Ct);
        using var response = await f.Send(client, HttpMethod.Post, "/api/v1/auth/change-password",
            new { currentPassword = f.Password, newPassword = f.Password + "Z", expectedVersions = new { account = proof.GetProperty("accountVersion").GetString() } }, await f.Csrf(client));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode); Assert.Equal(1, await f.Db.AuthSessions.CountAsync(s => s.RevokedAt == null, Ct)); Assert.Equal(0, await f.Db.AuditLogs.CountAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task DuplicateSignup_ReturnsConflictWithoutSecondAccountProfileOrAudit()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var client = f.Client(); var csrf = await f.Csrf(client);
        var body = new { fullName = "Synthetic signup", dateOfBirth = "2000-01-01", email = "new@example.invalid", phone = "0912345678", password = f.Password };
        using var first = await f.Send(client, HttpMethod.Post, "/api/v1/auth/register", body, csrf); Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        using var duplicate = await f.Send(client, HttpMethod.Post, "/api/v1/auth/register", body, csrf); Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(8, await f.Db.Accounts.CountAsync(Ct)); Assert.Equal(2, await f.Db.StudentProfiles.CountAsync(Ct)); Assert.Equal(1, await f.Db.AuditLogs.CountAsync(Ct));
    }
}

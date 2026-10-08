namespace EnglishCenter.Tests.Api.Integration;

using System.Net;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class AdminSqlHostingTests
{
    private const string OptIn = "V01 HTTP/SQL requires EC_V01_SQL=1 and approved isolated rollback scope.";
    private static CancellationToken Ct => IntegrationSqlHost.Ct;
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task Lock_RevokeAuditMarkerReplayAndLookupShareSqlState()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var admin = f.Client(); using var target = f.Client();
        await f.Login(target, "Teacher"); await f.Login(admin, "Admin"); var id = f.Accounts["Teacher"];
        using var detail = await f.Send(admin, HttpMethod.Get, $"/api/v1/admin/accounts/{id}"); Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        var view = await IntegrationSqlHost.Json(detail); AdminSqlAssertions.Detail(view, id, "Teacher", "ACTIVE");
        var version = view.GetProperty("version").GetString(); var key = Guid.NewGuid();
        var body = new { expectedVersions = new { account = version } }; var csrf = await f.Csrf(admin);
        using var result = await f.Send(admin, HttpMethod.Post, $"/api/v1/admin/accounts/{id}/lock", body, csrf, key);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode); var command = await IntegrationSqlHost.Json(result);
        Assert.Equal(new[] { "account", "operationId" }, command.EnumerateObject().Select(p => p.Name).Order().ToArray());
        AdminSqlAssertions.Detail(command.GetProperty("account"), id, "Teacher", "LOCKED"); var operation = command.GetProperty("operationId").GetGuid();
        var locked = await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == id, Ct); Assert.Equal("LOCKED", locked.Status); Assert.Equal(2, locked.SecurityVersion);
        Assert.Equal(0, await f.Db.AuthSessions.CountAsync(s => s.AccountId == id && s.RevokedAt == null, Ct));
        Assert.Equal(1, await f.Db.AuditLogs.CountAsync(Ct)); Assert.Equal(1, await f.Db.ReplayOperations.CountAsync(Ct));
        using var oldTarget = await f.Send(target, HttpMethod.Get, "/api/v1/auth/me"); Assert.Equal(HttpStatusCode.Unauthorized, oldTarget.StatusCode);
        var actorActivity = (await f.Db.AuthSessions.AsNoTracking().SingleAsync(s => s.AccountId == f.Accounts["Admin"], Ct)).LastActivityAt;
        f.Advance(TimeSpan.FromMinutes(1));
        using var replay = await f.Send(admin, HttpMethod.Post, $"/api/v1/admin/accounts/{id}/lock", body, csrf, key);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode); var replayBody = await IntegrationSqlHost.Json(replay);
        Assert.Equal(operation, replayBody.GetProperty("operationId").GetGuid());
        Assert.Equal(actorActivity, (await f.Db.AuthSessions.AsNoTracking().SingleAsync(s => s.AccountId == f.Accounts["Admin"], Ct)).LastActivityAt);
        var replayAccount = await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == id, Ct);
        Assert.Equal(locked.SecurityVersion, replayAccount.SecurityVersion); Assert.Equal(locked.RowVersion, replayAccount.RowVersion);
        Assert.Equal(1, await f.Db.AuditLogs.CountAsync(Ct)); Assert.Equal(1, await f.Db.ReplayOperations.CountAsync(Ct));
        using var lookup = await f.Send(admin, HttpMethod.Get, $"/api/v1/admin/command-results/{key:D}?action=ADMIN.ACCOUNT.LOCK");
        Assert.Equal(HttpStatusCode.OK, lookup.StatusCode); AdminSqlAssertions.Completion(await IntegrationSqlHost.Json(lookup), id, operation);
    }
    [Theory(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    [InlineData("Student")][InlineData("Admissions")][InlineData("AcademicAffairs")][InlineData("Teacher")][InlineData("Accountant")][InlineData("Director")]
    public async Task NonAdminRoles_DetailLockAndLookupAreForbidden(string role)
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var client = f.Client(); await f.Login(client, role); var id = f.Accounts["Teacher"];
        using var detail = await f.Send(client, HttpMethod.Get, $"/api/v1/admin/accounts/{id}"); Assert.Equal(HttpStatusCode.Forbidden, detail.StatusCode);
        using var command = await f.Send(client, HttpMethod.Post, $"/api/v1/admin/accounts/{id}/lock", new { expectedVersions = new { account = "AAAAAAAAAAE=" } }, await f.Csrf(client), Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Forbidden, command.StatusCode);
        using var lookup = await f.Send(client, HttpMethod.Get, $"/api/v1/admin/command-results/{Guid.NewGuid():D}?action=ADMIN.ACCOUNT.LOCK"); Assert.Equal(HttpStatusCode.Forbidden, lookup.StatusCode);
        Assert.Equal(0, await f.Db.AuditLogs.CountAsync(Ct)); Assert.Equal(0, await f.Db.ReplayOperations.CountAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task RestrictedAdmin_CannotEnterPilot()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); await f.Restricted("Admin"); using var client = f.Client(); await f.Login(client, "Admin");
        using var detail = await f.Send(client, HttpMethod.Get, $"/api/v1/admin/accounts/{f.Accounts["Teacher"]}"); Assert.Equal(HttpStatusCode.Forbidden, detail.StatusCode);
        using var command = await f.Send(client, HttpMethod.Post, $"/api/v1/admin/accounts/{f.Accounts["Teacher"]}/lock", new { expectedVersions = new { account = "AAAAAAAAAAE=" } }, await f.Csrf(client), Guid.NewGuid()); Assert.Equal(HttpStatusCode.Forbidden, command.StatusCode);
        Assert.Equal(0, await f.Db.ReplayOperations.CountAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task StaleVersionAndSelfGuard_DoNotCompleteCommand()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var client = f.Client(); var proof = await f.Login(client, "Admin"); var csrf = await f.Csrf(client);
        using var stale = await f.Send(client, HttpMethod.Post, $"/api/v1/admin/accounts/{f.Accounts["Teacher"]}/lock", new { expectedVersions = new { account = "AAAAAAAAAAA=" } }, csrf, Guid.NewGuid()); Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        using var self = await f.Send(client, HttpMethod.Post, $"/api/v1/admin/accounts/{f.Accounts["Admin"]}/lock", new { expectedVersions = new { account = proof.GetProperty("accountVersion").GetString() } }, csrf, Guid.NewGuid()); Assert.Equal(HttpStatusCode.Conflict, self.StatusCode);
        Assert.Equal(0, await f.Db.ReplayOperations.CountAsync(Ct)); Assert.Equal(0, await f.Db.AuditLogs.CountAsync(Ct));
        Assert.Equal("ACTIVE", (await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == f.Accounts["Admin"], Ct)).Status);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task MissingCsrfAndInvalidUuid_DoNotMutateOrAppend()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var client = f.Client(); await f.Login(client, "Admin"); var id = f.Accounts["Teacher"];
        using var detail = await f.Send(client, HttpMethod.Get, $"/api/v1/admin/accounts/{id}"); var view = await IntegrationSqlHost.Json(detail);
        var body = new { expectedVersions = new { account = view.GetProperty("version").GetString() } };
        using var denied = await f.Send(client, HttpMethod.Post, $"/api/v1/admin/accounts/{id}/lock", body, key: Guid.NewGuid()); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using var invalid = await f.Send(client, HttpMethod.Post, $"/api/v1/admin/accounts/{id}/lock", body, await f.Csrf(client), Guid.Empty); Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(0, await f.Db.ReplayOperations.CountAsync(Ct)); Assert.Equal(0, await f.Db.AuditLogs.CountAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task PostWriteFault_RollsStatusSessionsAuditAndMarkerBack()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var admin = f.Client(); using var target = f.Client(); await f.Login(target, "Teacher"); await f.Login(admin, "Admin");
        var id = f.Accounts["Teacher"]; using var detail = await f.Send(admin, HttpMethod.Get, $"/api/v1/admin/accounts/{id}"); var view = await IntegrationSqlHost.Json(detail); var csrf = await f.Csrf(admin); f.FailAfterWrite = true;
        using var failed = await f.Send(admin, HttpMethod.Post, $"/api/v1/admin/accounts/{id}/lock", new { expectedVersions = new { account = view.GetProperty("version").GetString() } }, csrf, Guid.NewGuid()); Assert.Equal(HttpStatusCode.InternalServerError, failed.StatusCode);
        var account = await f.Db.Accounts.AsNoTracking().SingleAsync(a => a.AccountId == id, Ct); Assert.Equal("ACTIVE", account.Status); Assert.Equal(1, account.SecurityVersion);
        Assert.Equal(1, await f.Db.AuthSessions.CountAsync(s => s.AccountId == id && s.RevokedAt == null, Ct));
        Assert.Equal(0, await f.Db.AuditLogs.CountAsync(Ct)); Assert.Equal(0, await f.Db.ReplayOperations.CountAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(IntegrationSqlGate.Enabled), SkipType = typeof(IntegrationSqlGate))]
    public async Task UnmatchedLookup_Returns404WithoutEffects()
    {
        await using var f = await IntegrationSqlHost.CreateAsync(); using var client = f.Client(); await f.Login(client, "Admin");
        using var result = await f.Send(client, HttpMethod.Get, $"/api/v1/admin/command-results/{Guid.NewGuid():D}?action=ADMIN.ACCOUNT.LOCK"); Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(0, await f.Db.ReplayOperations.CountAsync(Ct));
    }
}

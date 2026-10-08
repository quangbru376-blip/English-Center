namespace EnglishCenter.Tests.Api.Integration;

using System.Data;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Administration;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Services.Administration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class OwnedSqlExecutorTests
{
    private static CancellationToken Ct => OwnedSqlFixture.Ct;
    [Fact(SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate), Skip = "Explicit owned SQL opt-in required.")]
    public async Task Signup_commits_account_profile_and_mandatory_audit_without_session()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(); await f.Signup.ExecuteAsync(f.SignupInput(), Ct);
        await using var db = f.Db(); var account = await db.Accounts.SingleAsync(a => a.Email == "signup@example.invalid", Ct);
        Assert.Equal(9, await db.Accounts.CountAsync(Ct)); Assert.Equal(2, await db.StudentProfiles.CountAsync(Ct));
        Assert.Equal(account.AccountId, (await db.StudentProfiles.SingleAsync(p => p.AccountId == account.AccountId, Ct)).AccountId);
        Assert.Equal(0, await db.AuthSessions.CountAsync(Ct)); Assert.Equal("ACTIVE", account.Status); Assert.Equal(1, account.SecurityVersion);
        Assert.True(f.Hasher.Verify(account.PasswordHash, f.Password) != PasswordVerificationResult.Failed, "Committed credential must verify.");
        var audit = await db.AuditLogs.SingleAsync(Ct); Assert.Equal("STUDENT.ACCOUNT.CREATE", audit.Action); Assert.Equal("MANDATORY", audit.AuditCategory);
        Assert.Null(audit.ActorAccountId); Assert.Equal(account.AccountId.ToString(System.Globalization.CultureInfo.InvariantCulture), audit.EntityId);
        Assert.Equal("SUCCESS", audit.Result); Assert.NotEqual(Guid.Empty, audit.OperationId); Assert.Null(audit.SecurityExpiresAt); Assert.Null(audit.IpAddress);
        using var json = System.Text.Json.JsonDocument.Parse(audit.Details!);
        Assert.Equal(new[] { "new" }, json.RootElement.EnumerateObject().Select(p => p.Name).Order().ToArray());
        var projection = json.RootElement.GetProperty("new");
        Assert.Equal(new[] { "mustChangePassword", "roleId", "status" }, projection.EnumerateObject().Select(p => p.Name).Order().ToArray());
        Assert.Equal(account.RoleId, projection.GetProperty("roleId").GetInt32()); Assert.Equal("ACTIVE", projection.GetProperty("status").GetString());
        Assert.False(projection.GetProperty("mustChangePassword").GetBoolean());
    }
    [Fact(SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate), Skip = "Explicit owned SQL opt-in required.")]
    public async Task Signup_two_clients_same_contacts_commit_only_once()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(); var input = f.SignupInput();
        var outcomes = await Race(() => f.Signup.ExecuteAsync(input, Ct), () => f.Signup.ExecuteAsync(input, Ct));
        Assert.Single(outcomes, o => o.Error is null); Assert.Single(outcomes, o => o.Error == ApiErrorCode.CONFLICT);
        await using var db = f.Db(); Assert.Equal(9, await db.Accounts.CountAsync(Ct)); Assert.Equal(2, await db.StudentProfiles.CountAsync(Ct));
        Assert.Equal(1, await db.AuditLogs.CountAsync(Ct)); Assert.Equal(0, await db.AuthSessions.CountAsync(Ct));
    }
    [Fact(SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate), Skip = "Explicit owned SQL opt-in required.")]
    public async Task Login_me_logout_commit_and_security_history_is_independent()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(); var first = await f.Grant("Student"); var second = await f.Grant("Student");
        var token = f.Token(first); Assert.Equal(f.Accounts["Student"], (await f.Current.ReadAsync(token, true, Ct)).Context.AccountId);
        await f.Logout.ExecuteAsync(token, Ct); await Denied(() => f.Current.ReadAsync(token, true, Ct));
        Assert.Equal(f.Accounts["Student"], (await f.Current.ReadAsync(f.Token(second), true, Ct)).Context.AccountId);
        await using var db = f.Db(); var sessions = await db.AuthSessions.ToListAsync(Ct);
        Assert.Equal(2, sessions.Count); Assert.Single(sessions, s => s.RevokedAt is not null); Assert.Single(sessions, s => s.RevokedAt is null);
        var rows = await db.AuditLogs.OrderBy(a => a.AuditId).ToListAsync(Ct); Assert.Equal(3, rows.Count);
        OwnedAuditAssertions.Security(rows[0], "AUTH.LOGIN", f.Accounts["Student"], true, f.Clock.GetUtcNow().UtcDateTime);
        OwnedAuditAssertions.Security(rows[1], "AUTH.LOGIN", f.Accounts["Student"], true, f.Clock.GetUtcNow().UtcDateTime);
        OwnedAuditAssertions.Security(rows[2], "AUTH.LOGOUT", f.Accounts["Student"], true, f.Clock.GetUtcNow().UtcDateTime);
        Assert.Equal(0, f.Incidents.GetFailureCount(SecurityAuditKind.Login)); Assert.Equal(0, f.Incidents.GetFailureCount(SecurityAuditKind.Logout));
    }
    [Theory(SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate), Skip = "Explicit owned SQL opt-in required.")]
    [InlineData(false)] [InlineData(true)]
    public async Task Password_commits_new_credential_and_revokes_all_own_sessions(bool restricted)
    {
        await using var f = await OwnedSqlFixture.CreateAsync(); if (restricted) await f.Restricted();
        var first = await f.Grant("Student"); var second = await f.Grant("Student"); var next = f.NextPassword();
        await f.Change.ExecuteAsync(f.Token(first), f.ChangeInput(first, next), Ct);
        await Denied(() => f.Current.ReadAsync(f.Token(first), false, Ct)); await Denied(() => f.Current.ReadAsync(f.Token(second), false, Ct));
        await using (var db = f.Db())
        {
            var account = await db.Accounts.SingleAsync(a => a.AccountId == f.Accounts["Student"], Ct);
            Assert.Equal(2, account.SecurityVersion); Assert.False(account.MustChangePassword); Assert.Null(account.TempIssuedAt); Assert.Null(account.TempExpiresAt);
            Assert.True(f.Hasher.Verify(account.PasswordHash, next) != PasswordVerificationResult.Failed, "New credential must verify.");
            Assert.True(f.Hasher.Verify(account.PasswordHash, f.Password) == PasswordVerificationResult.Failed, "Old credential must fail.");
            var sessions = await db.AuthSessions.ToListAsync(Ct); Assert.Equal(2, sessions.Count); Assert.All(sessions, s => Assert.NotNull(s.RevokedAt));
            OwnedAuditAssertions.PasswordChange(await db.AuditLogs.SingleAsync(a => a.Action == "AUTH.PASSWORD_CHANGE", Ct), account.AccountId, 1, 2, restricted);
        }
        var deliberate = await f.Grant("Student", next); Assert.Equal("NORMAL", deliberate.Context.Scope);
        await using var fresh = f.Db(); Assert.Equal(1, await fresh.AuthSessions.CountAsync(s => s.RevokedAt == null, Ct));
    }
    [Fact(SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate), Skip = "Explicit owned SQL opt-in required.")]
    public async Task Mandatory_audit_real_failure_rolls_back_prior_password_writes()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(); var grant = await f.Grant("Student"); var next = f.NextPassword();
        await using (var gate = await f.HoldAudit())
            await Assert.ThrowsAsync<MandatoryAuditException>(() => f.Change.ExecuteAsync(f.Token(grant), f.ChangeInput(grant, next), Ct));
        await using var db = f.Db(); var account = await db.Accounts.SingleAsync(a => a.AccountId == f.Accounts["Student"], Ct);
        Assert.Equal(1, account.SecurityVersion); Assert.False(account.MustChangePassword);
        Assert.True(f.Hasher.Verify(account.PasswordHash, f.Password) != PasswordVerificationResult.Failed, "Rollback must preserve original credential.");
        Assert.True(f.Hasher.Verify(account.PasswordHash, next) == PasswordVerificationResult.Failed, "Uncommitted credential must fail.");
        Assert.Null((await db.AuthSessions.SingleAsync(Ct)).RevokedAt); Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.Action == "AUTH.PASSWORD_CHANGE", Ct));
        await f.Current.ReadAsync(f.Token(grant), true, Ct);
    }
    [Fact(SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate), Skip = "Explicit owned SQL opt-in required.")]
    public async Task Security_audit_timeout_cannot_undo_owned_login_or_logout_commit()
    {
        await using var f = await OwnedSqlFixture.CreateAsync();
        await using (var gate = await f.HoldAudit())
        {
            var grant = await f.Grant("Student");
            await using (var db = f.Db()) Assert.Equal(1, await db.AuthSessions.CountAsync(s => s.RevokedAt == null, Ct));
            await f.Logout.ExecuteAsync(f.Token(grant), Ct);
            await using (var db = f.Db()) Assert.NotNull((await db.AuthSessions.SingleAsync(Ct)).RevokedAt);
        }
        await using var fresh = f.Db(); Assert.Equal(0, await fresh.AuditLogs.CountAsync(Ct));
        Assert.Equal(1, f.Incidents.GetFailureCount(SecurityAuditKind.Login)); Assert.Equal(1, f.Incidents.GetFailureCount(SecurityAuditKind.Logout));
    }
    [Fact(SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate), Skip = "Explicit owned SQL opt-in required.")]
    public async Task Detail_actual_executor_uses_separate_snapshot_between_live_guards()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(); var admin = await f.Grant("Admin"); var trace = new OwnedDetailTrace();
        var executor = new SqlAdminAccountExecutor(() => f.Db(trace, trace.TransactionObserver), f.Clock, 10, 1000);
        var detail = await executor.DetailAsync(f.Token(admin), f.Accounts["Student"], Ct); Assert.Equal("ACTIVE", detail.Status);
        Assert.Equal(new[] { IsolationLevel.ReadCommitted, IsolationLevel.Snapshot, IsolationLevel.ReadCommitted }, trace.Transactions.Select(t => t.Isolation).ToArray());
        Assert.Equal(3, trace.Transactions.Select(t => t.Context).Distinct().Count());
        await using var db = f.Db(); Assert.Equal(0, await db.ReplayOperations.CountAsync(Ct)); Assert.Equal(1, await db.AuditLogs.CountAsync(Ct));
    }
    [Fact(SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate), Skip = "Explicit owned SQL opt-in required.")]
    public async Task Detail_final_live_guard_rejects_revocation_after_buffered_snapshot()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(); var first = await f.Grant("Admin"); var second = await f.Grant("Admin2");
        var trace = new OwnedDetailTrace(async () => await f.Admin.LockAsync(f.Token(second), OwnedSqlFixture.LockInput(f.Accounts["Admin"], first.Context.AccountVersion, Guid.NewGuid()), Ct));
        var executor = new SqlAdminAccountExecutor(() => f.Db(trace, trace.TransactionObserver), f.Clock, 10, 1000);
        await Denied(() => executor.DetailAsync(f.Token(first), f.Accounts["Student"], Ct));
        Assert.Equal(new[] { IsolationLevel.ReadCommitted, IsolationLevel.Snapshot, IsolationLevel.ReadCommitted }, trace.Transactions.Select(t => t.Isolation).ToArray());
        Assert.Equal(3, trace.Transactions.Select(t => t.Context).Distinct().Count());
        await using var db = f.Db(); Assert.Equal("LOCKED", (await db.Accounts.SingleAsync(a => a.AccountId == f.Accounts["Admin"], Ct)).Status);
        Assert.Equal("ACTIVE", (await db.Accounts.SingleAsync(a => a.AccountId == f.Accounts["Student"], Ct)).Status);
        Assert.Equal(1, await db.ReplayOperations.CountAsync(Ct)); Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.Action == "ADMIN.ACCOUNT.LOCK", Ct));
    }
    [Fact(SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate), Skip = "Explicit owned SQL opt-in required.")]
    public async Task Same_uuid_two_clients_share_one_committed_lock_operation()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(); var admin = await f.Grant("Admin"); await f.Grant("Student"); await f.Grant("Student"); var target = await f.Admin.DetailAsync(f.Token(admin), f.Accounts["Student"], Ct);
        var input = OwnedSqlFixture.LockInput(target.Id, target.Version, Guid.NewGuid());
        var outcomes = await Race(() => f.Admin.LockAsync(f.Token(admin), input, Ct), () => f.Admin.LockAsync(f.Token(admin), input, Ct));
        Assert.All(outcomes, o => Assert.Null(o.Error)); Assert.Equal(outcomes[0].Value!.OperationId, outcomes[1].Value!.OperationId);
        await AssertLock(f, outcomes[0].Value!.OperationId);
    }
    [Fact(SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate), Skip = "Explicit owned SQL opt-in required.")]
    public async Task Different_uuid_same_rv_two_clients_commit_one_lock()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(); var admin = await f.Grant("Admin"); await f.Grant("Student"); await f.Grant("Student"); var target = await f.Admin.DetailAsync(f.Token(admin), f.Accounts["Student"], Ct);
        var outcomes = await Race(() => f.Admin.LockAsync(f.Token(admin), OwnedSqlFixture.LockInput(target.Id, target.Version, Guid.NewGuid()), Ct),
            () => f.Admin.LockAsync(f.Token(admin), OwnedSqlFixture.LockInput(target.Id, target.Version, Guid.NewGuid()), Ct));
        Assert.Single(outcomes, o => o.Error is null); Assert.Single(outcomes, o => o.Error == ApiErrorCode.CONFLICT);
        await AssertLock(f, outcomes.Single(o => o.Error is null).Value!.OperationId);
    }
    [Fact(SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate), Skip = "Explicit owned SQL opt-in required.")]
    public async Task Password_and_current_logout_two_clients_leave_one_serialized_outcome()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(); var first = await f.Grant("Student"); await f.Grant("Student"); var next = f.NextPassword();
        var outcomes = await Race(async () => { await f.Change.ExecuteAsync(f.Token(first), f.ChangeInput(first, next), Ct); return true; },
            async () => { await f.Logout.ExecuteAsync(f.Token(first), Ct); return true; });
        Assert.Single(outcomes, o => o.Error is null); Assert.Single(outcomes, o => o.Error == ApiErrorCode.AUTHENTICATION_REQUIRED);
        await using var db = f.Db(); var account = await db.Accounts.SingleAsync(a => a.AccountId == f.Accounts["Student"], Ct);
        var sessions = await db.AuthSessions.ToListAsync(Ct); Assert.Equal(2, sessions.Count);
        if (outcomes[0].Error is null)
        {
            Assert.Equal(2, account.SecurityVersion); Assert.All(sessions, s => Assert.NotNull(s.RevokedAt));
            Assert.True(f.Hasher.Verify(account.PasswordHash, next) != PasswordVerificationResult.Failed, "Winning password commit must persist.");
            OwnedAuditAssertions.PasswordChange(await db.AuditLogs.SingleAsync(a => a.Action == "AUTH.PASSWORD_CHANGE", Ct), account.AccountId, 1, 2, false);
            Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.Action == "AUTH.LOGOUT", Ct));
        }
        else
        {
            Assert.Equal(1, account.SecurityVersion); Assert.Single(sessions, s => s.RevokedAt is null); Assert.Single(sessions, s => s.RevokedAt is not null);
            Assert.True(f.Hasher.Verify(account.PasswordHash, f.Password) != PasswordVerificationResult.Failed, "Winning logout must preserve credential.");
            Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.Action == "AUTH.PASSWORD_CHANGE", Ct));
            OwnedAuditAssertions.Security(await db.AuditLogs.SingleAsync(a => a.Action == "AUTH.LOGOUT", Ct), "AUTH.LOGOUT", account.AccountId, true, f.Clock.GetUtcNow().UtcDateTime);
        }
        await Denied(() => f.Current.ReadAsync(f.Token(first), true, Ct));
    }
    private static async Task AssertLock(OwnedSqlFixture f, Guid operation)
    {
        await using var db = f.Db(); var account = await db.Accounts.SingleAsync(a => a.AccountId == f.Accounts["Student"], Ct);
        Assert.Equal("LOCKED", account.Status); Assert.Equal(2, account.SecurityVersion);
        var marker = await db.ReplayOperations.SingleAsync(Ct); var audit = await db.AuditLogs.SingleAsync(a => a.Action == "ADMIN.ACCOUNT.LOCK", Ct);
        Assert.Equal(operation, marker.OperationId); Assert.Equal(operation, audit.OperationId); Assert.Equal("MANDATORY", audit.AuditCategory);
        Assert.Equal(f.Accounts["Admin"], marker.ActorAccountId); Assert.Equal("SUCCESS", audit.Result);
        var targetSessions = await db.AuthSessions.Where(s => s.AccountId == account.AccountId).ToListAsync(Ct);
        Assert.Equal(2, targetSessions.Count); Assert.All(targetSessions, s => Assert.NotNull(s.RevokedAt));
        Assert.Equal(1, await db.AuthSessions.CountAsync(s => s.AccountId == f.Accounts["Admin"] && s.RevokedAt == null, Ct));
    }
    private sealed record Outcome<T>(T? Value, ApiErrorCode? Error);
    private static async Task<Outcome<T>[]> Race<T>(Func<Task<T>> first, Func<Task<T>> second)
    {
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var arrivals = 0;
        async Task<Outcome<T>> Run(Func<Task<T>> action)
        {
            if (Interlocked.Increment(ref arrivals) == 2) ready.SetResult(); await start.Task;
            try { return new(await action(), null); } catch (ApiErrorException error) { return new(default, error.Code); }
        }
        var a = Task.Run(() => Run(first), Ct); var b = Task.Run(() => Run(second), Ct);
        await ready.Task.WaitAsync(TimeSpan.FromSeconds(5), Ct); start.SetResult();
        return await Task.WhenAll(a, b).WaitAsync(TimeSpan.FromSeconds(25), Ct);
    }
    private static async Task Denied<T>(Func<Task<T>> action)
    { var error = await Assert.ThrowsAsync<ApiErrorException>(action); Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, error.Code); }
}

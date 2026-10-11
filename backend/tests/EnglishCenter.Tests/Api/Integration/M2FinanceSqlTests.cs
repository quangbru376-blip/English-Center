namespace EnglishCenter.Tests.Api.Integration;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Data.Common;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Infrastructure.Admissions;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Infrastructure.Finance;
using EnglishCenter.Api.Infrastructure.StudentInvoices;
using EnglishCenter.Api.Services.Enrollment;
using EnglishCenter.Api.Services.Finance;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M2FinanceSqlTests
{
    private const string OptIn = "Assigned M2 owned scratch SQL only; identity-paired cleanup.";
    private static CancellationToken Ct => OwnedSqlFixture.Ct;
    private static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web);
    private static SqlFinanceExecutor Ex(OwnedSqlFixture f) => new(() => f.Db(), f.Clock, 10, 1000);
    internal static async Task<int> Seed(OwnedSqlFixture f)
    {
        var reg = await M1AdmissionsSqlTests.Confirmed(f); var token = f.Token(await f.Grant("Admissions"));
        var ex = new SqlAdmissionsExecutor(() => f.Db(), f.Clock, 10, 1000); var preview = await ex.ApprovalAsync(token, reg, Ct);
        var result = await ex.CommandAsync(token, 41, reg, Guid.NewGuid(), JsonSerializer.Serialize(preview.Manifest, Wire), 65536, Ct);
        return result.Registration.Invoice!.Id;
    }
    private static async Task<JsonObject> Collect(OwnedSqlFixture f, VerifiedSessionToken token, int id)
    {
        var p = await Ex(f).PreviewAsync(token, id, Ct); Assert.True(p.Eligible);
        var b = JsonSerializer.SerializeToNode(p.Manifest, Wire)!.AsObject(); b["amount"] = p.Amount; b["method"] = "CASH"; return b;
    }
    private static JsonObject Correct(FinanceCommandResult result, string method = "TRANSFER", string? reference = "  Abc  ") => JsonSerializer.SerializeToNode(new
    { method, transactionReference = reference, reason = "  Đối chiếu\n lại.  ", expectedVersions = new { payment = result.Payment.Version, invoice = result.Invoice.Versions.Invoice } }, Wire)!.AsObject();
    private static async Task<ApiErrorCode> Error(Func<Task> work) => (await Assert.ThrowsAsync<ApiErrorException>(work)).Code;
    private sealed class ExpireAfterProjection(M1AdmissionsSqlTests.Clock clock, DateTimeOffset expiry) : DbCommandInterceptor
    {
        public bool Fired { get; private set; }
        public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData data, DbDataReader reader, CancellationToken ct = default)
        {
            if (!Fired && command.CommandText.Contains("dbo.vw_FinanceInvoiceFacts", StringComparison.Ordinal)) { clock.Now = expiry; Fired = true; }
            return ValueTask.FromResult(reader);
        }
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task FinalProjectionDeadlineRollsBackCollectedPaymentAuditAndCompletion()
    {
        var time = new M1AdmissionsSqlTests.Clock(); await using var f = await OwnedSqlFixture.CreateAsync(time); var id = await Seed(f);
        var token = f.Token(await f.Grant("Accountant")); var body = await Collect(f, token, id); var key = Guid.NewGuid();
        var cutoff = new ExpireAfterProjection(time, time.Now.AddHours(1)); var executor = new SqlFinanceExecutor(() => f.Db(cutoff), f.Clock, 10, 1000);
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, await Error(() => executor.CommandAsync(token, 51, id, key, body.ToJsonString(), 65536, Ct))); Assert.True(cutoff.Fired);
        await using var db = f.Db(); Assert.Empty(await db.Payments.ToListAsync(Ct)); Assert.Equal("UNPAID", (await db.Invoices.SingleAsync(Ct)).Status);
        Assert.False(await db.ReplayOperations.AnyAsync(x => x.RequestId == key, Ct)); Assert.Empty(await db.AuditLogs.Where(x => x.Action.StartsWith("FINANCE.")).ToListAsync(Ct));
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task ConcurrentIdenticalKeyReturnsSameOperationAndOnePayment()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock()); var id = await Seed(f); var token = f.Token(await f.Grant("Accountant"));
        var body = (await Collect(f, token, id)).ToJsonString(); var key = Guid.NewGuid(); var ex = Ex(f);
        var pair = await Task.WhenAll(ex.CommandAsync(token, 51, id, key, body, 65536, Ct), ex.CommandAsync(token, 51, id, key, body, 65536, Ct));
        Assert.Single(pair, x => !x.Replayed); Assert.Single(pair, x => x.Replayed); Assert.Equal(pair[0].Result.OperationId, pair[1].Result.OperationId);
        await using var db = f.Db(); Assert.Equal(1, await db.Payments.CountAsync(Ct)); Assert.Equal(1, await db.ReplayOperations.CountAsync(x => x.RequestId == key, Ct));
        Assert.Equal(2, await db.AuditLogs.CountAsync(x => x.OperationId == pair[0].Result.OperationId, Ct));
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task CollectAndCorrectionCommitAtomicExactViewsReplayAuditAndStudentPrivacy()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock()); var id = await Seed(f); var t = f.Token(await f.Grant("Accountant")); var ex = Ex(f);
        await using (var db = f.Db()) { var i = await db.Invoices.SingleAsync(Ct); i.Amount = 123456789012345678m; await db.SaveChangesAsync(Ct); }
        var body = await Collect(f, t, id); var key = Guid.NewGuid(); var first = await ex.CommandAsync(t, 51, id, key, body.ToJsonString(), 65536, Ct);
        Assert.False(first.Replayed); Assert.Equal("PAID", first.Result.Invoice.Status); Assert.Equal("123456789012345678", first.Result.Payment.AmountPaid); Assert.Null(first.Result.Payment.TransactionReference);
        var repeated = await ex.CommandAsync(t, 51, id, key, body.ToJsonString(), 65536, Ct); Assert.True(repeated.Replayed); Assert.Equal(first.Result.OperationId, repeated.Result.OperationId);
        var correction = Correct(first.Result); var correctionKey = Guid.NewGuid(); var fixedResult = await ex.CommandAsync(t, 52, first.Result.Payment.Id, correctionKey, correction.ToJsonString(), 65536, Ct);
        Assert.Equal("Abc", fixedResult.Result.Payment.TransactionReference); Assert.Equal("Đối chiếu\n lại.", fixedResult.Result.Payment.LastCorrection!.Reason);
        Assert.Equal(first.Result.Payment.PaidAt, fixedResult.Result.Payment.PaidAt); Assert.Equal(first.Result.Payment.ConfirmedBy, fixedResult.Result.Payment.ConfirmedBy); Assert.Equal(first.Result.Payment.AmountPaid, fixedResult.Result.Payment.AmountPaid);
        Assert.Equal(first.Result.Invoice.Versions.Invoice, fixedResult.Result.Invoice.Versions.Invoice); Assert.NotEqual(first.Result.Payment.Version, fixedResult.Result.Payment.Version);
        Assert.Equal(fixedResult.Result.OperationId, (await ex.CommandAsync(t, 52, first.Result.Payment.Id, correctionKey, correction.ToJsonString(), 65536, Ct)).Result.OperationId);
        var replayCurrent = await ex.CommandAsync(t, 51, id, key, body.ToJsonString(), 65536, Ct); Assert.Equal("TRANSFER", replayCurrent.Result.Payment.Method);
        var completion = await ex.LookupAsync(t, "FINANCE.INVOICE.COLLECT", key, Ct); Assert.Equal(2, completion.Target.Count); Assert.Equal(id, completion.Target["invoiceId"]);
        var student = f.Token(await f.Grant("Student")); var own = await new SqlStudentInvoiceReader(() => f.Db(), f.Clock, 10, 1000).DetailAsync(student, id, Ct);
        Assert.Equal("PAID", own.Status); Assert.DoesNotContain("transactionReference", JsonSerializer.Serialize(own, Wire));
        await using var verify = f.Db(); Assert.Equal(1, await verify.Payments.CountAsync(Ct)); Assert.Equal(2, await verify.ReplayOperations.CountAsync(x => x.ActionCode.StartsWith("FINANCE."), Ct));
        var audits = await verify.AuditLogs.Where(x => x.Action.StartsWith("FINANCE.")).ToArrayAsync(Ct); Assert.Equal(3, audits.Length);
        Assert.Equal(2, audits.Count(x => x.OperationId == first.Result.OperationId)); Assert.Contains(audits, x => x.OperationId == fixedResult.Result.OperationId);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task StaleParentAbsenceAndAmountPrecedenceNoEffects()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock()); var id = await Seed(f); var t = f.Token(await f.Grant("Accountant")); var ex = Ex(f); var body = await Collect(f, t, id);
        var wrong = body.DeepClone().AsObject(); wrong["amount"] = "999"; Assert.Equal(ApiErrorCode.VALIDATION_ERROR, await Error(() => ex.CommandAsync(t, 51, id, Guid.NewGuid(), wrong.ToJsonString(), 65536, Ct)));
        await using (var db = f.Db()) { var i = await db.Invoices.SingleAsync(Ct); i.DueDate = i.DueDate.AddDays(1); await db.SaveChangesAsync(Ct); }
        Assert.Equal(ApiErrorCode.STATE_CONFLICT, await Error(() => ex.CommandAsync(t, 51, id, Guid.NewGuid(), wrong.ToJsonString(), 65536, Ct)));
        body = await Collect(f, t, id); body["expectedSet"]!["registrationId"] = 2147483647;
        Assert.Equal(ApiErrorCode.STATE_CONFLICT, await Error(() => ex.CommandAsync(t, 51, id, Guid.NewGuid(), body.ToJsonString(), 65536, Ct)));
        await using var verify = f.Db(); Assert.Empty(await verify.Payments.ToListAsync(Ct)); Assert.Equal("UNPAID", (await verify.Invoices.SingleAsync(Ct)).Status); Assert.Empty(await verify.ReplayOperations.Where(x => x.ActionCode.StartsWith("FINANCE.")).ToListAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task DifferentIntentSameKeyDuplicateAndNoopCorrectionRefused()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock()); var id = await Seed(f); var t = f.Token(await f.Grant("Accountant")); var ex = Ex(f); var b = await Collect(f, t, id); var key = Guid.NewGuid();
        var r = await ex.CommandAsync(t, 51, id, key, b.ToJsonString(), 65536, Ct); b["transactionReference"] = "changed";
        Assert.Equal(ApiErrorCode.IDEMPOTENCY_CONTENT_MISMATCH, await Error(() => ex.CommandAsync(t, 51, id, key, b.ToJsonString(), 65536, Ct)));
        Assert.Equal(ApiErrorCode.STATE_CONFLICT, await Error(() => ex.CommandAsync(t, 51, id, Guid.NewGuid(), b.ToJsonString(), 65536, Ct)));
        Assert.Equal(ApiErrorCode.VALIDATION_ERROR, await Error(() => ex.CommandAsync(t, 52, r.Result.Payment.Id, Guid.NewGuid(), Correct(r.Result, "CASH", null).ToJsonString(), 65536, Ct)));
        var p = await ex.PreviewAsync(t, id, Ct); Assert.False(p.Eligible); Assert.Null(p.Manifest);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task MandatoryAuditFailureRollsBackPaymentPaidMarkerAndMetadata()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock()); var id = await Seed(f); var t = f.Token(await f.Grant("Accountant")); var ex = Ex(f); var body = await Collect(f, t, id);
        await using (var hold = await f.HoldAudit()) await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(() => ex.CommandAsync(t, 51, id, Guid.NewGuid(), body.ToJsonString(), 65536, Ct));
        await using (var db = f.Db()) { Assert.Empty(await db.Payments.ToListAsync(Ct)); Assert.Equal("UNPAID", (await db.Invoices.SingleAsync(Ct)).Status); Assert.Empty(await db.ReplayOperations.Where(x => x.ActionCode.StartsWith("FINANCE.")).ToListAsync(Ct)); }
        var r = await ex.CommandAsync(t, 51, id, Guid.NewGuid(), body.ToJsonString(), 65536, Ct);
        await using (var hold = await f.HoldAudit()) await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(() => ex.CommandAsync(t, 52, r.Result.Payment.Id, Guid.NewGuid(), Correct(r.Result).ToJsonString(), 65536, Ct));
        var actual = await ex.DetailAsync(t, id, Ct); Assert.Equal(r.Result.Payment, actual.Payment);
        await using var verify = f.Db(); Assert.Equal(1, await verify.ReplayOperations.CountAsync(x => x.ActionCode.StartsWith("FINANCE."), Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task ConcurrentCollectionsAndCorrectionsHaveOneWinner()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock()); var id = await Seed(f); var t = f.Token(await f.Grant("Accountant")); var ex = Ex(f); var b = await Collect(f, t, id);
        async Task<FinanceCommandOutcome?> Attempt(int api, int target, JsonObject body) { try { return await ex.CommandAsync(t, api, target, Guid.NewGuid(), body.ToJsonString(), 65536, Ct); } catch (ApiErrorException e) { Assert.Equal(ApiErrorCode.STATE_CONFLICT, e.Code); return null; } }
        var pair = await Task.WhenAll(Attempt(51, id, b), Attempt(51, id, b)); var winner = Assert.Single(pair, x => x is not null)!;
        var corrections = await Task.WhenAll(Attempt(52, winner.Result.Payment.Id, Correct(winner.Result, "TRANSFER", "one")), Attempt(52, winner.Result.Payment.Id, Correct(winner.Result, "TRANSFER", "two")));
        Assert.Single(corrections, x => x is not null); await using var verify = f.Db(); Assert.Equal(1, await verify.Payments.CountAsync(Ct)); Assert.Equal(2, await verify.ReplayOperations.CountAsync(x => x.ActionCode.StartsWith("FINANCE."), Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task CollectVersusAdmissionsCancellationSerializesBothDomains()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock()); var id = await Seed(f); var t = f.Token(await f.Grant("Accountant")); var a = f.Token(await f.Grant("Admissions")); var ex = Ex(f); var b = await Collect(f, t, id);
        await using var db = f.Db(); var reg = (await db.Invoices.SingleAsync(Ct)).RegistrationId; var adm = new SqlAdmissionsExecutor(() => f.Db(), f.Clock, 10, 1000);
        var p = await adm.CancellationAsync(a, reg, Ct); var cancel = JsonSerializer.SerializeToNode(p.Manifest, Wire)!.AsObject(); cancel["reason"] = "Synthetic cancellation";
        async Task<bool> CollectTask() { try { await ex.CommandAsync(t, 51, id, Guid.NewGuid(), b.ToJsonString(), 65536, Ct); return true; } catch (ApiErrorException e) { Assert.Equal(ApiErrorCode.STATE_CONFLICT, e.Code); return false; } }
        async Task<bool> CancelTask() { try { await adm.CommandAsync(a, 44, reg, Guid.NewGuid(), cancel.ToJsonString(), 65536, Ct); return true; } catch (ApiErrorException e) { Assert.Equal(ApiErrorCode.STATE_CONFLICT, e.Code); return false; } }
        var result = await Task.WhenAll(CollectTask(), CancelTask()); Assert.Single(result, x => x); db.ChangeTracker.Clear(); var invoice = await db.Invoices.SingleAsync(Ct);
        Assert.Equal(result[0] ? "PAID" : "CANCELLED", invoice.Status); Assert.Equal(result[0] ? 1 : 0, await db.Payments.CountAsync(Ct)); Assert.Equal(result[0] ? "ADMITTED" : "CANCELLED", (await db.Registrations.SingleAsync(Ct)).Status);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task SearchVW06FN06CurrentContactsLiteralFiltersPagingDatesAndDebt()
    {
        var time = new M1AdmissionsSqlTests.Clock(); await using var f = await OwnedSqlFixture.CreateAsync(time); var id = await Seed(f); var t = f.Token(await f.Grant("Accountant")); var ex = Ex(f);
        await using (var db = f.Db()) { var i = await db.Invoices.SingleAsync(Ct); i.DueDate = f.Clock.GetToday(); var a = await db.Accounts.SingleAsync(x => x.AccountId == f.Accounts["Student"], Ct); a.FullName = "Current %_[name]"; await db.SaveChangesAsync(Ct); }
        var now = await ex.SearchAsync(t, FinanceSearchFilter.Parse("{\"overdueOnly\":true}"), Ct); Assert.Empty(now.Items);
        time.Now = time.Now.AddDays(1); t = f.Token(await f.Grant("Accountant")); var overdue = await ex.SearchAsync(t, FinanceSearchFilter.Parse("{\"name\":\"%_[\",\"phone\":\"+84 912-345-678\",\"invoiceNumber\":\"HD-\",\"overdueOnly\":true,\"pageSize\":1}"), Ct);
        Assert.Equal(1, overdue.TotalCount); Assert.Equal(1, Assert.Single(overdue.Items).OverdueDays); Assert.Equal("Current %_[name]", overdue.Items[0].Student.FullName);
        Assert.Empty((await ex.SearchAsync(t, FinanceSearchFilter.Parse("{\"invoiceNumber\":\"HD-%\"}"), Ct)).Items);
        Assert.Empty((await ex.SearchAsync(t, FinanceSearchFilter.Parse("{\"status\":\"PAID\",\"overdueOnly\":true}"), Ct)).Items);
        Assert.Equal(1, (await ex.SearchAsync(t, FinanceSearchFilter.Parse("{\"issueFromDate\":\"2026-10-10\",\"issueToDate\":\"2026-10-10\",\"page\":2147483647}"), Ct)).TotalCount);
        await using (var db = f.Db()) { var i = await db.Invoices.SingleAsync(Ct); i.IssuedAt = new DateTime(9999, 12, 31, 16, 59, 59, DateTimeKind.Utc); await db.SaveChangesAsync(Ct); }
        Assert.Equal(1, (await ex.SearchAsync(t, FinanceSearchFilter.Parse("{\"issueToDate\":\"9999-12-31\"}"), Ct)).TotalCount);
        await using (var db = f.Db()) { var i = await db.Invoices.SingleAsync(Ct); i.IssuedAt = new DateTime(9999, 12, 31, 17, 0, 0, DateTimeKind.Utc); await db.SaveChangesAsync(Ct); }
        Assert.Equal(0, (await ex.SearchAsync(t, FinanceSearchFilter.Parse("{\"issueToDate\":\"9999-12-31\"}"), Ct)).TotalCount);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task CurrentRoleRevocationRestrictedAndLookupCallerAreFresh()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock()); var id = await Seed(f); var t = f.Token(await f.Grant("Accountant")); var ex = Ex(f); var b = await Collect(f, t, id); var key = Guid.NewGuid();
        await ex.CommandAsync(t, 51, id, key, b.ToJsonString(), 65536, Ct);
        var teacher = f.Token(await f.Grant("Teacher")); Assert.Equal(ApiErrorCode.FORBIDDEN, await Error(() => ex.DetailAsync(teacher, id, Ct)));
        Assert.Equal(ApiErrorCode.RESULT_NOT_CONFIRMED, await Error(() => ex.LookupAsync(t, "FINANCE.INVOICE.COLLECT", Guid.NewGuid(), Ct)));
        await f.Restricted("Accountant"); Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, await Error(() => ex.LookupAsync(t, "FINANCE.INVOICE.COLLECT", key, Ct)));
        t = f.Token(await f.Grant("Accountant")); Assert.Equal(ApiErrorCode.FORBIDDEN, await Error(() => ex.LookupAsync(t, "FINANCE.INVOICE.COLLECT", key, Ct)));
        await using (var db = f.Db()) { var account = await db.Accounts.SingleAsync(x => x.AccountId == t.AccountId, Ct); account.Status = "LOCKED"; await db.SaveChangesAsync(Ct); }
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED, await Error(() => ex.CommandAsync(t, 51, id, key, b.ToJsonString(), 65536, Ct)));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task CookieCsrfHttp201Replay200ExactDtosAndMetadata200()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock()); var id = await Seed(f); await using var host = await M1HttpSqlTests.Host.Create(f); await host.Login("Accountant"); var root = $"/api/v1/finance/invoices/{id}";
        using var preview = await host.Send(HttpMethod.Post, root + "/collection-preview"); Assert.Equal(HttpStatusCode.OK, preview.StatusCode); var json = await preview.Content.ReadFromJsonAsync<JsonElement>(Ct);
        var b = JsonNode.Parse(json.GetProperty("manifest").GetRawText())!.AsObject(); b["amount"] = json.GetProperty("amount").GetString(); b["method"] = "TRANSFER"; b["transactionReference"] = "Abc"; var key = Guid.NewGuid();
        using var csrf = await host.Send(HttpMethod.Post, root + "/collect", b, key, false); Assert.Equal(HttpStatusCode.Forbidden, csrf.StatusCode);
        using var noKey = await host.Send(HttpMethod.Post, root + "/collect", b); Assert.Equal(HttpStatusCode.BadRequest, noKey.StatusCode);
        using var first = await host.Send(HttpMethod.Post, root + "/collect", b, key); Assert.Equal(HttpStatusCode.Created, first.StatusCode); Assert.Contains("no-store", first.Headers.CacheControl!.ToString());
        var r = await first.Content.ReadFromJsonAsync<FinanceCommandResult>(Wire, Ct); Assert.NotNull(r);
        using var replay = await host.Send(HttpMethod.Post, root + "/collect", b, key); Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        var correctionKey = Guid.NewGuid(); using var corrected = await host.Send(HttpMethod.Put, $"/api/v1/finance/payments/{r.Payment.Id}/metadata", Correct(r, "CASH", null), correctionKey); Assert.Equal(HttpStatusCode.OK, corrected.StatusCode);
        var c = await corrected.Content.ReadFromJsonAsync<JsonElement>(Ct); Assert.Equal(3, c.EnumerateObject().Count()); Assert.Equal(16, c.GetProperty("invoice").EnumerateObject().Count()); Assert.Equal(9, c.GetProperty("payment").EnumerateObject().Count()); Assert.Equal(JsonValueKind.Null, c.GetProperty("payment").GetProperty("transactionReference").ValueKind);
        using var lookup = await host.Send(HttpMethod.Get, $"/api/v1/finance/command-results/{key:D}?action=FINANCE.INVOICE.COLLECT"); Assert.Equal(HttpStatusCode.OK, lookup.StatusCode); var completion = await lookup.Content.ReadFromJsonAsync<JsonElement>(Ct); Assert.Equal(4, completion.EnumerateObject().Count()); Assert.Equal(2, completion.GetProperty("target").EnumerateObject().Count());
        using var search = await host.Send(HttpMethod.Post, "/api/v1/finance/invoices/search", new { status = "PAID" }); Assert.Equal(HttpStatusCode.OK, search.StatusCode); Assert.Equal(1, (await search.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("totalCount").GetInt32());
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task HttpRoleFirstStrictBoundariesAndNoFinanceFieldsForStudent()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new M1AdmissionsSqlTests.Clock()); var id = await Seed(f); await using var host = await M1HttpSqlTests.Host.Create(f);
        foreach (var role in new[] { "Student", "Admissions", "AcademicAffairs", "Teacher", "Director", "Admin" }) { await host.Login(role); using var denied = await host.Send(HttpMethod.Get, "/api/v1/finance/invoices/malformed"); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode); }
        await host.Login("Accountant"); using var piiQuery = await host.Send(HttpMethod.Post, "/api/v1/finance/invoices/search?email=private", new { }); Assert.Equal(HttpStatusCode.BadRequest, piiQuery.StatusCode);
        using var missing = await host.Send(HttpMethod.Post, "/api/v1/finance/invoices/2147483647/collect", new { }, Guid.NewGuid()); Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var extra = await host.Send(HttpMethod.Get, $"/api/v1/finance/invoices/{id}?status=PAID"); Assert.Equal(HttpStatusCode.BadRequest, extra.StatusCode);
        using var badFamily = await host.Send(HttpMethod.Get, $"/api/v1/finance/command-results/{Guid.NewGuid():D}?action=ADMISSIONS.REGISTRATION.APPROVE"); Assert.Equal(HttpStatusCode.BadRequest, badFamily.StatusCode);
        await f.Restricted("Accountant"); await host.Login("Accountant"); using var restricted = await host.Send(HttpMethod.Get, $"/api/v1/finance/invoices/{id}"); Assert.Equal(HttpStatusCode.Forbidden, restricted.StatusCode);
    }
}

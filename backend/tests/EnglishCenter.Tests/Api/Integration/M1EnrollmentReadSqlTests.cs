namespace EnglishCenter.Tests.Api.Integration;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Infrastructure.Enrollment;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M1EnrollmentReadSqlTests
{
    private const string OptIn = "Owned M1 read/SP02 scratch SQL with identity-checked cleanup.";
    private static CancellationToken Ct => OwnedSqlFixture.Ct;
    private sealed class Clock : TimeProvider { public override DateTimeOffset GetUtcNow() => new(2026, 10, 9, 3, 0, 0, TimeSpan.Zero); }
    private static IQueryCollection Query(string value = "") => new QueryCollection(Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(value));

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task TeacherOwnSessionRosterAndTaskHaveExactProjectionWithoutFinancialFields()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await M1EnrollmentFoundationSqlTests.Seed(f); var token = f.Token(await f.Grant("Teacher")); var reader = new SqlTeacherEntryReader(() => f.Db(), f.Clock, 10, 1000);
        var list = await reader.SearchAsync(token, Query(), Ct); var session = Assert.Single(list.Items); Assert.Equal("CLOSED", session.EffectiveStatus);
        var roster = await reader.RosterAsync(token, session.Id, SessionRosterFilter.Parse("{}"), Ct); var row = Assert.Single(roster.Items); Assert.Equal(ids.Booking, row.BookingId);
        var wire = JsonSerializer.SerializeToElement(row, new JsonSerializerOptions(JsonSerializerDefaults.Web)); Assert.Equal(12, wire.EnumerateObject().Count()); Assert.False(wire.TryGetProperty("invoiceStatus", out _));
        var task = await reader.TaskAsync(token, ids.Booking, Ct); Assert.Contains("GRADE", task.AllowedActions); Assert.Contains("MARK_ABSENT", task.AllowedActions);
        var detail = await reader.DetailAsync(token, session.Id, Ct); Assert.Equal(12, JsonSerializer.SerializeToElement(detail, new JsonSerializerOptions(JsonSerializerDefaults.Web)).EnumerateObject().Count());
        await using var db = f.Db(); Assert.Equal(0, await db.ReplayOperations.CountAsync(Ct)); Assert.Equal(0, await db.AuditLogs.CountAsync(x => x.Action.StartsWith("TEACHER."), Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task ReassignmentRemovesListDetailRosterAndTaskScope()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await M1EnrollmentFoundationSqlTests.Seed(f); var token = f.Token(await f.Grant("Teacher")); var reader = new SqlTeacherEntryReader(() => f.Db(), f.Clock, 10, 1000); var session = (await reader.SearchAsync(token, Query(), Ct)).Items.Single().Id;
        await using (var db = f.Db()) { var replacement = await db.Accounts.SingleAsync(x => x.AccountId == f.Accounts["Admin2"], Ct); replacement.RoleId = await db.Accounts.Where(x => x.AccountId == token.AccountId).Select(x => x.RoleId).SingleAsync(Ct); await db.SaveChangesAsync(Ct); var s = await db.EntryTestSessions.SingleAsync(Ct); s.GraderId = replacement.AccountId; await db.SaveChangesAsync(Ct); }
        Assert.Empty((await reader.SearchAsync(token, Query(), Ct)).Items);
        Assert.Equal(ApiErrorCode.NOT_FOUND, (await Assert.ThrowsAsync<ApiErrorException>(() => reader.DetailAsync(token, session, Ct))).Code);
        Assert.Equal(ApiErrorCode.NOT_FOUND, (await Assert.ThrowsAsync<ApiErrorException>(() => reader.RosterAsync(token, session, SessionRosterFilter.Parse("{}"), Ct))).Code);
        Assert.Equal(ApiErrorCode.NOT_FOUND, (await Assert.ThrowsAsync<ApiErrorException>(() => reader.TaskAsync(token, ids.Booking, Ct))).Code);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task StudentHomeCalendarResultsAndPreviewsRemainCoherentAndReadOnly()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await M1EnrollmentFoundationSqlTests.Seed(f); var token = f.Token(await f.Grant("Student")); var reader = new SqlStudentEnrollmentReader(() => f.Db(), f.Clock, 10, 1000);
        var home = Assert.Single((await reader.HomeAsync(token, Query(), Ct)).Items); Assert.Equal(ids.Registration, home.RegistrationId); Assert.Null(home.NearestEvent); Assert.Null(home.FinalizedReport); Assert.Null(home.UnpaidInvoice);
        var detail = await reader.DetailAsync(token, ids.Registration, Ct); Assert.Equal(17, JsonSerializer.SerializeToElement(detail, new JsonSerializerOptions(JsonSerializerDefaults.Web)).EnumerateObject().Count());
        var current = Assert.Single((await reader.CalendarAsync(token, Query(), Ct)).Items); Assert.Equal("SCHEDULED", current.Status);
        var blocked = await reader.OptionsAsync(token, ids.Registration, Query(), Ct); Assert.False(blocked.Eligible); Assert.Null(blocked.AttemptContext); Assert.Empty(blocked.Items);
        var confirmation = await reader.ConfirmationAsync(token, ids.Registration, Ct); Assert.False(confirmation.Eligible); Assert.Null(confirmation.Manifest);
        var cancellation = await reader.CancellationAsync(token, ids.Registration, Ct); Assert.True(cancellation.Eligible);
        var result = JsonSerializer.SerializeToElement(await reader.ResultsAsync(token, ids.Registration, Ct), new JsonSerializerOptions(JsonSerializerDefaults.Web)); Assert.Equal(6, result.EnumerateObject().Count());
        await using var db = f.Db(); Assert.Equal("CLOSED", (await db.EntryTestSessions.SingleAsync(Ct)).Status); Assert.Null((await db.EntryTestAttempts.SingleAsync(Ct)).Score); Assert.Equal(0, await db.ReplayOperations.CountAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task HistoricalInactiveCourseRemainsVisibleButSelectionDetailRefuses()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await M1EnrollmentFoundationSqlTests.Seed(f); var token = f.Token(await f.Grant("Student")); var reader = new SqlStudentEnrollmentReader(() => f.Db(), f.Clock, 10, 1000);
        int course; await using (var db = f.Db()) { var row = await db.Courses.SingleAsync(Ct); course = row.CourseId; row.Status = "INACTIVE"; await db.SaveChangesAsync(Ct); }
        Assert.Empty((await reader.CoursesAsync(token, Query(), Ct)).Items); Assert.Equal("INACTIVE", (await reader.DetailAsync(token, ids.Registration, Ct)).Course.Status);
        Assert.Equal("INACTIVE", Assert.Single((await reader.HomeAsync(token, Query(), Ct)).Items).Course.Status);
        Assert.Equal(ApiErrorCode.NOT_FOUND, (await Assert.ThrowsAsync<ApiErrorException>(() => reader.CourseAsync(token, course, Ct))).Code);
        Assert.Equal(ApiErrorCode.NOT_FOUND, (await Assert.ThrowsAsync<ApiErrorException>(() => reader.CalendarAsync(token, Query("?registrationId=2147483647"), Ct))).Code);
    }
}

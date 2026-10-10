namespace EnglishCenter.Tests.Api.Integration;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Admissions;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M1EnrollmentHttpSqlTests
{
    private const string OptIn = "M1 Student/Teacher HTTP to owned scratch SQL with cleanup.";
    private static CancellationToken Ct => OwnedSqlFixture.Ct;
    private sealed class Clock : TimeProvider { public override DateTimeOffset GetUtcNow() => new(2026, 10, 10, 3, 0, 0, TimeSpan.Zero); }
    private static async Task<JsonElement> Json(HttpResponseMessage r) => await r.Content.ReadFromJsonAsync<JsonElement>(Ct);
    private static async Task<JsonElement> Get(M1HttpSqlTests.Host host, string path)
    { using var response = await host.Send(HttpMethod.Get, path); Assert.Equal(HttpStatusCode.OK, response.StatusCode); Assert.Contains("no-store", response.Headers.CacheControl!.ToString()); return await Json(response); }
    private static async Task<int> Course(OwnedSqlFixture f)
    { await using var db = f.Db(); var course = new Course { CourseCode = "HTTP_ENROLL", CourseName = "Synthetic", BasePrice = 123456789012345678m, Status = "ACTIVE" }; db.Courses.Add(course); await db.SaveChangesAsync(Ct); return course.CourseId; }
    private static async Task<int> Session(OwnedSqlFixture f)
    { await using var db = f.Db(); var s = new EntryTestSession { StartAt = f.Clock.GetUtcNow().UtcDateTime.AddHours(2), EndAt = f.Clock.GetUtcNow().UtcDateTime.AddHours(3), GraderId = f.Accounts["Teacher"], Capacity = 1, Room = "HTTP_ENROLL", Status = "OPEN" }; db.EntryTestSessions.Add(s); await db.SaveChangesAsync(Ct); return s.SessionId; }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task StudentReadShapesNoStoreOwnershipAndQueryAllowlist()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await M1EnrollmentFoundationSqlTests.Seed(f); await using var host = await M1HttpSqlTests.Host.Create(f); await host.Login("Student");
        Assert.Equal(8, (await Get(host, "/api/v1/students/me/profile")).EnumerateObject().Count());
        Assert.Equal(10, (await Get(host, "/api/v1/students/me/home")).GetProperty("items")[0].EnumerateObject().Count());
        Assert.Equal(5, (await Get(host, "/api/v1/students/me/courses")).GetProperty("items")[0].EnumerateObject().Count());
        Assert.Equal(9, (await Get(host, "/api/v1/students/me/registrations")).GetProperty("items")[0].EnumerateObject().Count());
        Assert.Equal(17, (await Get(host, $"/api/v1/students/me/registrations/{ids.Registration}")).EnumerateObject().Count());
        Assert.Equal(6, (await Get(host, $"/api/v1/students/me/registrations/{ids.Registration}/entry-results")).EnumerateObject().Count());
        Assert.Equal("SCHEDULED", (await Get(host, "/api/v1/students/me/entry-test-bookings")).GetProperty("items")[0].GetProperty("status").GetString());
        using var bad = await host.Send(HttpMethod.Get, "/api/v1/students/me/home?studentId=1"); Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        using var missing = await host.Send(HttpMethod.Get, "/api/v1/students/me/registrations/2147483647"); Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        await host.Login("Teacher"); using var role = await host.Send(HttpMethod.Get, "/api/v1/students/me/profile"); Assert.Equal(HttpStatusCode.Forbidden, role.StatusCode);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task StudentCreateBookAndLookupRequireCsrfUuidAndExactParents()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); await M1EnrollmentFoundationSqlTests.Seed(f); var course = await Course(f); var session = await Session(f); await using var host = await M1HttpSqlTests.Host.Create(f); await host.Login("Student");
        var body = new { courseId = course }; var key = Guid.NewGuid(); const string root = "/api/v1/students/me";
        using var csrf = await host.Send(HttpMethod.Post, root + "/registrations", body, key, false); Assert.Equal(HttpStatusCode.Forbidden, csrf.StatusCode);
        using var absentKey = await host.Send(HttpMethod.Post, root + "/registrations", body); Assert.Equal(HttpStatusCode.BadRequest, absentKey.StatusCode);
        using var created = await host.Send(HttpMethod.Post, root + "/registrations", body, key); Assert.Equal(HttpStatusCode.Created, created.StatusCode); var result = await Json(created); var reg = result.GetProperty("registration").GetProperty("id").GetInt32(); Assert.Equal(2, result.EnumerateObject().Count());
        using var repeated = await host.Send(HttpMethod.Post, root + "/registrations", body, key); Assert.Equal(HttpStatusCode.OK, repeated.StatusCode);
        var options = await Get(host, $"{root}/registrations/{reg}/entry-test-options"); var slot = options.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("id").GetInt32() == session);
        var book = new { targetSessionId = session, attemptContext = options.GetProperty("attemptContext"), expectedVersions = new { registration = options.GetProperty("registrationVersion").GetString(), targetSession = slot.GetProperty("version").GetString() } }; var bookKey = Guid.NewGuid();
        using var booked = await host.Send(HttpMethod.Post, $"{root}/registrations/{reg}/entry-test-bookings", book, bookKey); Assert.Equal(HttpStatusCode.Created, booked.StatusCode); var booking = (await Json(booked)).GetProperty("booking").GetProperty("id").GetInt32();
        using var replay = await host.Send(HttpMethod.Post, $"{root}/registrations/{reg}/entry-test-bookings", book, bookKey); Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        var lookup = await Get(host, $"{root}/command-results/{bookKey:D}?action=STUDENT.ENTRY_BOOKING.CREATE"); Assert.Equal(2, lookup.GetProperty("target").EnumerateObject().Count()); Assert.False(lookup.TryGetProperty("booking", out _));
        var moveOptions = await Get(host, $"{root}/registrations/{reg}/entry-test-options?bookingId={booking}");
        Assert.True(moveOptions.GetProperty("eligible").GetBoolean()); Assert.Equal(9, moveOptions.EnumerateObject().Count());
        Assert.Equal(booking, moveOptions.GetProperty("attemptContext").GetProperty("currentBookingId").GetInt32());
        Assert.DoesNotContain(moveOptions.GetProperty("items").EnumerateArray(), x => x.GetProperty("id").GetInt32() == session);
        using var wrongSource = await host.Send(HttpMethod.Get, $"{root}/registrations/{reg}/entry-test-options?bookingId=2147483647"); Assert.Equal(HttpStatusCode.NotFound, wrongSource.StatusCode);
        using var wrongParent = await host.Send(HttpMethod.Get, $"{root}/registrations/2147483647/entry-test-options?bookingId={booking}"); Assert.Equal(HttpStatusCode.NotFound, wrongParent.StatusCode);
        using var nested = await host.Send(HttpMethod.Post, $"{root}/registrations/2147483647/entry-test-bookings/{booking}/cancel", new { unexpected = true }, Guid.NewGuid()); Assert.Equal(HttpStatusCode.NotFound, nested.StatusCode);
        using var crossFamily = await host.Send(HttpMethod.Get, $"{root}/command-results/{bookKey:D}?action=TEACHER.ENTRY_RESULT.CREATE"); Assert.Equal(HttpStatusCode.Forbidden, crossFamily.StatusCode);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task TeacherScoreCorrectionHttpAndSafeLookupHaveExactProvenance()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await M1EnrollmentFoundationSqlTests.Seed(f); await using var host = await M1HttpSqlTests.Host.Create(f); await host.Login("Teacher"); const string root = "/api/v1/admissions/teachers/me";
        var task = await Get(host, $"{root}/entry-test-bookings/{ids.Booking}"); Assert.Equal(15, task.EnumerateObject().Count()); var body = JsonNode.Parse(task.GetProperty("gradeManifest").GetRawText())!.AsObject(); body["score"] = "0"; var key = Guid.NewGuid();
        using var denied = await host.Send(HttpMethod.Post, $"{root}/entry-test-bookings/{ids.Booking}/score", body, key, false); Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using var graded = await host.Send(HttpMethod.Post, $"{root}/entry-test-bookings/{ids.Booking}/score", body, key); Assert.Equal(HttpStatusCode.OK, graded.StatusCode); Assert.Equal("0.00", (await Json(graded)).GetProperty("task").GetProperty("targetResult").GetProperty("score").GetString());
        task = await Get(host, $"{root}/entry-test-bookings/{ids.Booking}"); var attempt = task.GetProperty("booking").GetProperty("attemptId").GetInt32(); var provenance = task.GetProperty("targetResult"); var correction = JsonNode.Parse(task.GetProperty("correctionManifest").GetRawText())!.AsObject(); correction["score"] = "80.5"; correction["reason"] = "Fixture correction";
        using var corrected = await host.Send(HttpMethod.Put, $"{root}/entry-test-attempts/{attempt}/score", correction, Guid.NewGuid()); Assert.Equal(HttpStatusCode.OK, corrected.StatusCode); var next = (await Json(corrected)).GetProperty("task").GetProperty("targetResult"); Assert.Equal("80.50", next.GetProperty("score").GetString()); Assert.Equal(provenance.GetProperty("gradedAt").GetString(), next.GetProperty("gradedAt").GetString()); Assert.Equal(provenance.GetProperty("originalGraderId").GetInt32(), next.GetProperty("originalGraderId").GetInt32());
        var lookup = await Get(host, $"{root}/command-results/{key:D}?action=TEACHER.ENTRY_RESULT.CREATE"); Assert.Equal(3, lookup.GetProperty("target").EnumerateObject().Count());
        await host.Login("Student"); using var forbidden = await host.Send(HttpMethod.Post, $"{root}/entry-test-bookings/{ids.Booking}/score", new { wrong = true }, Guid.NewGuid()); Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task TeacherAbsenceThenStudentRebookingUsesSameAttemptWithoutScore()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await M1EnrollmentFoundationSqlTests.Seed(f); var session = await Session(f); await using var host = await M1HttpSqlTests.Host.Create(f); await host.Login("Teacher"); const string teacher = "/api/v1/admissions/teachers/me";
        var task = await Get(host, $"{teacher}/entry-test-bookings/{ids.Booking}"); var attempt = task.GetProperty("booking").GetProperty("attemptId").GetInt32();
        using var absence = await host.Send(HttpMethod.Post, $"{teacher}/entry-test-bookings/{ids.Booking}/absence", task.GetProperty("absenceManifest"), Guid.NewGuid()); Assert.Equal(HttpStatusCode.OK, absence.StatusCode); var marked = (await Json(absence)).GetProperty("task"); Assert.Equal(JsonValueKind.Null, marked.GetProperty("relatedAttempts")[0].GetProperty("score").ValueKind); Assert.Equal(JsonValueKind.Null, marked.GetProperty("bandAssignment").ValueKind);
        await host.Login("Student"); const string student = "/api/v1/students/me"; var options = await Get(host, $"{student}/registrations/{ids.Registration}/entry-test-options"); Assert.Equal(attempt, options.GetProperty("attemptContext").GetProperty("attemptId").GetInt32()); var slot = options.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("id").GetInt32() == session);
        var body = new { targetSessionId = session, attemptContext = options.GetProperty("attemptContext"), expectedVersions = new { registration = options.GetProperty("registrationVersion").GetString(), targetSession = slot.GetProperty("version").GetString() } };
        using var rebook = await host.Send(HttpMethod.Post, $"{student}/registrations/{ids.Registration}/entry-test-bookings", body, Guid.NewGuid()); Assert.Equal(HttpStatusCode.Created, rebook.StatusCode); Assert.Equal(attempt, (await Json(rebook)).GetProperty("booking").GetProperty("attemptId").GetInt32());
        await using var db = f.Db(); Assert.Equal(1, await db.EntryTestAttempts.CountAsync(Ct)); Assert.Equal(2, await db.EntryTestBookings.CountAsync(Ct));
    }
}

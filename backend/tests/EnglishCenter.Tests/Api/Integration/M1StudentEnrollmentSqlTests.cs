namespace EnglishCenter.Tests.Api.Integration;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Infrastructure.Operator;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed partial class M1StudentEnrollmentSqlTests
{
    private const string OptIn = "Owned Student M1 scratch SQL, existing migrations, identity-checked cleanup.";
    private static CancellationToken Ct => OwnedSqlFixture.Ct;
    private static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web);
    private sealed class Clock : TimeProvider { public override DateTimeOffset GetUtcNow() => new(2026, 10, 10, 3, 0, 0, TimeSpan.Zero); }
    private static SqlStudentEnrollmentExecutor Executor(OwnedSqlFixture f) => new(() => f.Db(), f.Clock, 10, 1000);
    private static async Task<int> Seed(OwnedSqlFixture f)
    {
        await using var db = f.Db(); await LocalBandReferenceSeed.SeedAsync(db, Ct);
        var account = await db.Accounts.SingleAsync(x => x.AccountId == f.Accounts["Student"], Ct); account.Phone = "0912345678";
        var course = new Course { CourseCode = "M1_ENROLL", CourseName = "Synthetic", BasePrice = 1000, Status = "ACTIVE" }; db.Courses.Add(course); await db.SaveChangesAsync(Ct); return course.CourseId;
    }
    private static async Task<int> Session(OwnedSqlFixture f, int hours, int capacity = 20)
    {
        await using var db = f.Db(); var session = new EntryTestSession { StartAt = f.Clock.GetUtcNow().UtcDateTime.AddHours(hours), EndAt = f.Clock.GetUtcNow().UtcDateTime.AddHours(hours + 1), Room = "ENROLL" + hours, Capacity = capacity, GraderId = f.Accounts["Teacher"], Status = "OPEN" };
        db.EntryTestSessions.Add(session); await db.SaveChangesAsync(Ct); return session.SessionId;
    }
    private static async Task<RegistrationState> Read(OwnedSqlFixture f, int reg)
    {
        await using var db = f.Db(); var student = await db.Registrations.Where(x => x.RegistrationId == reg).Select(x => x.StudentId).SingleAsync(Ct);
        var set = await SqlEnrollmentRepository.ReadSet(db, student, [reg], Ct); return new(set, set.Registrations.Single());
    }
    private static async Task<OwnRegistrationResult> Create(OwnedSqlFixture f, VerifiedSessionToken token, int course, Guid? request = null) =>
        (OwnRegistrationResult)await Executor(f).CommandAsync(token, 25, 0, 0, request ?? Guid.NewGuid(), JsonSerializer.Serialize(new { courseId = course }), 65536, Ct);
    private static async Task<JsonObject> BookingBody(OwnedSqlFixture f, RegistrationState state, int session)
    {
        await using var db = f.Db(); var target = await db.EntryTestSessions.SingleAsync(x => x.SessionId == session, Ct);
        return JsonSerializer.SerializeToNode(new { targetSessionId = session, attemptContext = EnrollmentProjection.BookAttempt(state), expectedVersions = new { registration = EnrollmentProjection.Rv(state.Registration.RowVersion), targetSession = EnrollmentProjection.Rv(target.RowVersion) } }, Wire)!.AsObject();
    }
    private static string CancelBody(RegistrationState s, int booking, int? target = null, string? targetVersion = null)
    {
        var b = s.Booking(booking); var v = new JsonObject { ["registration"] = EnrollmentProjection.Rv(s.Registration.RowVersion), ["attempt"] = EnrollmentProjection.Rv(s.Attempt(b.AttemptId).RowVersion), ["booking"] = EnrollmentProjection.Rv(b.RowVersion), ["sourceSession"] = EnrollmentProjection.Rv(s.Session(b.SessionId).RowVersion) };
        var body = new JsonObject { ["expectedVersions"] = v }; if (target is {} id) { body["targetSessionId"] = id; v["targetSession"] = targetVersion; } return body.ToJsonString();
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task CreateUniqueUnfinishedAtomicReplayHistoricalCourseAndContentMismatch()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var course = await Seed(f); var token = f.Token(await f.Grant("Student")); var request = Guid.NewGuid();
        var first = await Create(f, token, course, request); Assert.False(first.Replayed);
        var duplicate = await Assert.ThrowsAsync<ApiErrorException>(() => Create(f, token, course)); Assert.Equal(ApiErrorCode.STATE_CONFLICT, duplicate.Code);
        await using (var db = f.Db()) { var c = await db.Courses.SingleAsync(Ct); c.Status = "INACTIVE"; await db.SaveChangesAsync(Ct); }
        var repeated = await Create(f, token, course, request); Assert.True(repeated.Replayed); Assert.Equal(first.OperationId, repeated.OperationId); Assert.Equal("INACTIVE", repeated.Registration.Course.Status);
        var mismatch = await Assert.ThrowsAsync<ApiErrorException>(() => Create(f, token, course + 1, request)); Assert.Equal(ApiErrorCode.IDEMPOTENCY_CONTENT_MISMATCH, mismatch.Code);
        await using var verify = f.Db(); Assert.Equal(1, await verify.Registrations.CountAsync(Ct)); Assert.Equal(1, await verify.ReplayOperations.CountAsync(Ct)); Assert.Equal(1, await verify.AuditLogs.CountAsync(x => x.Action == "STUDENT.REGISTRATION.CREATE", Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task BookCancelRebookAndReschedulePreserveLogicalAttemptAndHistory()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var course = await Seed(f); var token = f.Token(await f.Grant("Student")); var reg = (await Create(f, token, course)).Registration.Id;
        var a = await Session(f, 2); var b = await Session(f, 4); var body = await BookingBody(f, await Read(f, reg), a); var request = Guid.NewGuid();
        var first = (OwnBookingResult)await Executor(f).CommandAsync(token, 30, reg, 0, request, body.ToJsonString(), 65536, Ct); Assert.Equal("TESTING", first.RegistrationStatus);
        var repeat = (OwnBookingResult)await Executor(f).CommandAsync(token, 30, reg, 0, request, body.ToJsonString(), 65536, Ct); Assert.True(repeat.Replayed); Assert.Equal(first.OperationId, repeat.OperationId);
        var state = await Read(f, reg); await Executor(f).CommandAsync(token, 31, reg, first.Booking.Id, Guid.NewGuid(), CancelBody(state, first.Booking.Id), 65536, Ct);
        state = await Read(f, reg); Assert.Equal(first.Booking.AttemptId, EnrollmentProjection.BookAttempt(state)!.AttemptId);
        var rebook = (OwnBookingResult)await Executor(f).CommandAsync(token, 30, reg, 0, Guid.NewGuid(), (await BookingBody(f, state, a)).ToJsonString(), 65536, Ct);
        state = await Read(f, reg); await using var target = f.Db(); var rv = EnrollmentProjection.Rv((await target.EntryTestSessions.SingleAsync(x => x.SessionId == b, Ct)).RowVersion);
        var moved = (OwnRescheduleResult)await Executor(f).CommandAsync(token, 32, reg, rebook.Booking.Id, Guid.NewGuid(), CancelBody(state, rebook.Booking.Id, b, rv), 65536, Ct);
        Assert.Equal("CANCELLED", moved.OldBooking.Status); Assert.Equal("SCHEDULED", moved.NewBooking.Status); Assert.Equal(moved.OldBooking.AttemptId, moved.NewBooking.AttemptId);
        state = await Read(f, reg); Assert.Single(state.Attempts); Assert.Equal(3, state.Bookings.Count); Assert.Single(state.Bookings, x => x.Status == "SCHEDULED");
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task CancelRegistrationUsesExactFutureSetClosesInvoiceAndAllowsNewRegistration()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var course = await Seed(f); var token = f.Token(await f.Grant("Student")); var reg = (await Create(f, token, course)).Registration.Id; var session = await Session(f, 2);
        await Executor(f).CommandAsync(token, 30, reg, 0, Guid.NewGuid(), (await BookingBody(f, await Read(f, reg), session)).ToJsonString(), 65536, Ct);
        await using (var db = f.Db()) { db.Invoices.Add(new() { RegistrationId = reg, InvoiceNumber = "HD-2026-00000001", Amount = 1000, IssuedAt = f.Clock.GetUtcNow().UtcDateTime, DueDate = f.Clock.GetToday(), Status = "UNPAID" }); await db.SaveChangesAsync(Ct); var s = await db.EntryTestSessions.SingleAsync(Ct); s.Status = "CLOSED"; await db.SaveChangesAsync(Ct); }
        var state = await Read(f, reg); var body = JsonSerializer.Serialize(EnrollmentProjection.CancellationManifest(state, f.Clock.GetUtcNow()), Wire); var request = Guid.NewGuid();
        var cancelled = (OwnRegistrationResult)await Executor(f).CommandAsync(token, 28, reg, 0, request, body, 65536, Ct); Assert.Equal("CANCELLED", cancelled.Registration.Status);
        Assert.True(((OwnRegistrationResult)await Executor(f).CommandAsync(token, 28, reg, 0, request, body, 65536, Ct)).Replayed);
        state = await Read(f, reg); Assert.False(state.Registration.IsUnfinished); Assert.Equal("CANCELLED", state.Bookings.Single().Status); Assert.Equal("CANCELLED", state.Invoice!.Status);
        Assert.NotEqual(reg, (await Create(f, token, course)).Registration.Id);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task StudentAuditFailureRollsBackBookingAttemptStatusAndReplay()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var course = await Seed(f); var token = f.Token(await f.Grant("Student")); var reg = (await Create(f, token, course)).Registration.Id; var session = await Session(f, 2);
        var body = await BookingBody(f, await Read(f, reg), session);
        await using (var gate = await f.HoldAudit()) await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(() => Executor(f).CommandAsync(token, 30, reg, 0, Guid.NewGuid(), body.ToJsonString(), 65536, Ct));
        var state = await Read(f, reg); Assert.Empty(state.Bookings); Assert.Empty(state.Attempts); Assert.Equal("PENDING", state.Registration.Status);
        await using var db = f.Db(); Assert.Equal(1, await db.ReplayOperations.CountAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task ConfirmationRequiresBestCompletedBandAndNoScheduledRetake()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var course = await Seed(f); var token = f.Token(await f.Grant("Student")); var reg = (await Create(f, token, course)).Registration.Id; var session = await Session(f, 2);
        var booked = (OwnBookingResult)await Executor(f).CommandAsync(token, 30, reg, 0, Guid.NewGuid(), (await BookingBody(f, await Read(f, reg), session)).ToJsonString(), 65536, Ct);
        await using (var db = f.Db()) { var s = await db.EntryTestSessions.SingleAsync(Ct); s.StartAt = f.Clock.GetUtcNow().UtcDateTime.AddHours(-1); s.EndAt = f.Clock.GetUtcNow().UtcDateTime; s.Status = "CLOSED"; await db.SaveChangesAsync(Ct); }
        var teacher = f.Token(await f.Grant("Teacher")); var state = await Read(f, reg); var grade = JsonSerializer.SerializeToNode(TeacherTaskProjection.Manifest(state, state.Booking(booked.Booking.Id), 70), Wire)!.AsObject(); grade["score"] = "80";
        await new SqlTeacherEntryExecutor(() => f.Db(), f.Clock, 10, 1000).CommandAsync(teacher, 70, booked.Booking.Id, Guid.NewGuid(), grade.ToJsonString(), 65536, Ct);
        state = await Read(f, reg); var manifest = JsonSerializer.Serialize(EnrollmentProjection.ConfirmationManifest(state), Wire);
        var secondSession = await Session(f, 4);
        var second = (OwnBookingResult)await Executor(f).CommandAsync(token, 30, reg, 0, Guid.NewGuid(), (await BookingBody(f, state, secondSession)).ToJsonString(), 65536, Ct);
        Assert.Equal(2, second.Booking.AttemptNumber);
        var blocked = await Assert.ThrowsAsync<ApiErrorException>(() => Executor(f).CommandAsync(token, 36, reg, 0, Guid.NewGuid(), manifest, 65536, Ct));
        Assert.Equal(ApiErrorCode.STATE_CONFLICT, blocked.Code);
        state = await Read(f, reg);
        await Executor(f).CommandAsync(token, 31, reg, second.Booking.Id, Guid.NewGuid(), CancelBody(state, second.Booking.Id), 65536, Ct);
        state = await Read(f, reg);
        Assert.Equal(second.Booking.AttemptId, EnrollmentProjection.BookAttempt(state)!.AttemptId);
        Assert.Equal(booked.Booking.AttemptId, state.Band!.BestAttemptId);
        manifest = JsonSerializer.Serialize(EnrollmentProjection.ConfirmationManifest(state), Wire);
        var confirmed = (OwnRegistrationResult)await Executor(f).CommandAsync(token, 36, reg, 0, Guid.NewGuid(), manifest, 65536, Ct); Assert.Equal("CONFIRMED", confirmed.Registration.Status); Assert.NotNull(confirmed.Registration.AdmissionConfirmedAt); Assert.Null(confirmed.Registration.InvoiceId);
        var late = await Assert.ThrowsAsync<ApiErrorException>(() => new SqlTeacherEntryExecutor(() => f.Db(), f.Clock, 10, 1000).CommandAsync(teacher, 72, booked.Booking.AttemptId, Guid.NewGuid(), "{}", 65536, Ct)); Assert.Equal(ApiErrorCode.VALIDATION_ERROR, late.Code);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task CurrentBookingBlocksAnotherAndSameSessionRescheduleIsValidation()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var course = await Seed(f); var token = f.Token(await f.Grant("Student")); var reg = (await Create(f, token, course)).Registration.Id; var session = await Session(f, 2);
        var stale = await BookingBody(f, await Read(f, reg), session); var booked = (OwnBookingResult)await Executor(f).CommandAsync(token, 30, reg, 0, Guid.NewGuid(), stale.ToJsonString(), 65536, Ct);
        var conflict = await Assert.ThrowsAsync<ApiErrorException>(() => Executor(f).CommandAsync(token, 30, reg, 0, Guid.NewGuid(), stale.ToJsonString(), 65536, Ct)); Assert.Equal(ApiErrorCode.STATE_CONFLICT, conflict.Code);
        var state = await Read(f, reg); var error = await Assert.ThrowsAsync<ApiErrorException>(() => Executor(f).CommandAsync(token, 32, reg, booked.Booking.Id, Guid.NewGuid(), CancelBody(state, booked.Booking.Id, session, EnrollmentProjection.Rv(state.Session(session).RowVersion)), 65536, Ct)); Assert.Equal(ApiErrorCode.VALIDATION_ERROR, error.Code);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task WrongRoleAndNestedOwnershipPrecedeMalformedJson()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); await Seed(f); var student = f.Token(await f.Grant("Student")); var admin = f.Token(await f.Grant("Admin"));
        var role = await Assert.ThrowsAsync<ApiErrorException>(() => Executor(f).CommandAsync(admin, 25, 0, 0, Guid.NewGuid(), "bad-json", 65536, Ct)); Assert.Equal(ApiErrorCode.FORBIDDEN, role.Code);
        var missing = await Assert.ThrowsAsync<ApiErrorException>(() => Executor(f).CommandAsync(student, 31, int.MaxValue, int.MaxValue, Guid.NewGuid(), "bad-json", 65536, Ct)); Assert.Equal(ApiErrorCode.NOT_FOUND, missing.Code);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task FutureDateOfBirthCannotAlterAccountProfileOrAudit()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); await Seed(f); var token = f.Token(await f.Grant("Student")); var executor = new SqlOwnProfileExecutor(() => f.Db(), f.Clock, 10, 1000); var before = await executor.ReadAsync(token, Ct);
        var body = JsonSerializer.Serialize(new { fullName = "Changed", dateOfBirth = "2026-10-11", phone = "0922345678", address = "Changed", expectedVersions = before.Versions }, Wire);
        var error = await Assert.ThrowsAsync<ApiErrorException>(() => executor.UpdateAsync(token, body, Ct)); Assert.Equal(ApiErrorCode.VALIDATION_ERROR, error.Code);
        Assert.Equal(JsonSerializer.Serialize(before, Wire), JsonSerializer.Serialize(await executor.ReadAsync(token, Ct), Wire));
        await using var db = f.Db(); Assert.Equal(0, await db.AuditLogs.CountAsync(x => x.Action == "STUDENT.PROFILE.UPDATE", Ct));
    }
}

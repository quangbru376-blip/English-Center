namespace EnglishCenter.Tests.Api.Integration;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Infrastructure.Operator;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M1EnrollmentFoundationSqlTests
{
    private const string OptIn = "Owned M1 enrollment scratch SQL, existing migrations, identity-checked cleanup.";
    private static CancellationToken Ct => OwnedSqlFixture.Ct;
    private static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web);
    private sealed class Clock : TimeProvider { public override DateTimeOffset GetUtcNow() => new(2026, 10, 9, 3, 0, 0, TimeSpan.Zero); }
    private static SqlOwnProfileExecutor Profiles(OwnedSqlFixture f) => new(() => f.Db(), f.Clock, 10, 1000);
    internal static async Task<(int Registration, int Booking)> Seed(OwnedSqlFixture f)
    {
        await using var db = f.Db(); await LocalBandReferenceSeed.SeedAsync(db, Ct);
        var student = await db.StudentProfiles.SingleAsync(Ct); var account = await db.Accounts.SingleAsync(x => x.AccountId == student.AccountId, Ct); account.Phone = "0912345678";
        var course = new Course { CourseCode = "M1_STUDENT", CourseName = "Synthetic", BasePrice = 1000, Status = "ACTIVE" }; db.Courses.Add(course); await db.SaveChangesAsync(Ct);
        var reg = new Registration { StudentId = student.StudentId, CourseId = course.CourseId, Status = "TESTING", CreatedAt = f.Clock.GetUtcNow().UtcDateTime.AddDays(-1) }; db.Registrations.Add(reg); await db.SaveChangesAsync(Ct);
        var session = new EntryTestSession { StartAt = f.Clock.GetUtcNow().UtcDateTime.AddHours(-1), EndAt = f.Clock.GetUtcNow().UtcDateTime, Room = "M1_STUDENT", Capacity = 20, GraderId = f.Accounts["Teacher"], Status = "CLOSED" }; db.EntryTestSessions.Add(session); await db.SaveChangesAsync(Ct);
        var attempt = new EntryTestAttempt { RegistrationId = reg.RegistrationId, AttemptNumber = 1 }; db.EntryTestAttempts.Add(attempt); await db.SaveChangesAsync(Ct);
        var booking = new EntryTestBooking { AttemptId = attempt.AttemptId, SessionId = session.SessionId, Status = "SCHEDULED", BookedAt = f.Clock.GetUtcNow().UtcDateTime.AddHours(-2) }; db.EntryTestBookings.Add(booking); await db.SaveChangesAsync(Ct);
        return (reg.RegistrationId, booking.BookingId);
    }
    private static async Task<RegistrationState> Read(OwnedSqlFixture f, int reg)
    {
        await using var db = f.Db(); var student = await db.Registrations.Where(x => x.RegistrationId == reg).Select(x => x.StudentId).SingleAsync(Ct);
        var set = await SqlEnrollmentRepository.ReadSet(db, student, [reg], Ct); return new(set, set.Registrations.Single());
    }
    private static EnrollmentInput Input(RegistrationState state, int booking, int api, string? score = null, string? reason = null, Guid? request = null)
    {
        var body = JsonSerializer.SerializeToNode(TeacherTaskProjection.Manifest(state, state.Booking(booking), api), Wire)!.AsObject();
        if (api != 71) body["score"] = score; if (api == 72) body["reason"] = reason;
        return EnrollmentInput.Parse(api, 0, api == 72 ? state.Booking(booking).AttemptId : booking, request ?? Guid.NewGuid(), body.ToJsonString(), 65536);
    }
    private static async Task<TeacherCommandResult> Command(OwnedSqlFixture f, EnrollmentInput input)
    {
        var token = f.Token(await f.Grant("Teacher"));
        return await new SqlEnrollmentTransactions(() => f.Db(), f.Clock, 10, 1000).Write((s, c) => new TeacherEntryService(new(s), new SqlReplayRepository(s, 1000), f.Clock).CommandAsync(token, input, c), Ct);
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task Profile_AtomicTwoTokensUniquePhoneNoSecurityChangeOrReplay()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); await Seed(f); var token = f.Token(await f.Grant("Student")); var executor = Profiles(f);
        var old = await executor.ReadAsync(token, Ct); var versions = JsonSerializer.SerializeToElement(old.Versions, Wire);
        var body = JsonSerializer.Serialize(new { fullName = "Updated fixture", dateOfBirth = "2001-02-03", phone = "0922345678", address = "Fixture address", expectedVersions = versions });
        var updated = await executor.UpdateAsync(token, body, Ct); Assert.Equal("Updated fixture", updated.FullName); Assert.Equal("2001-02-03", updated.DateOfBirth); Assert.Equal(old.Email, updated.Email);
        await Assert.ThrowsAsync<ApiErrorException>(() => executor.UpdateAsync(token, body, Ct));
        await using (var duplicate = f.Db()) { var staff = await duplicate.Accounts.SingleAsync(x => x.AccountId == f.Accounts["Teacher"], Ct); staff.Phone = "0932345678"; await duplicate.SaveChangesAsync(Ct); }
        var duplicateBody = JsonSerializer.Serialize(new { fullName = "Must rollback unique phone", dateOfBirth = "2002-02-03", phone = "0932345678", address = "Must rollback", expectedVersions = updated.Versions }, Wire);
        var conflict = await Assert.ThrowsAsync<ApiErrorException>(() => executor.UpdateAsync(token, duplicateBody, Ct)); Assert.Equal(ApiErrorCode.CONFLICT, conflict.Code);
        Assert.Equal(JsonSerializer.Serialize(updated, Wire), JsonSerializer.Serialize(await executor.ReadAsync(token, Ct), Wire));
        await using var db = f.Db(); var account = await db.Accounts.SingleAsync(x => x.AccountId == token.AccountId, Ct);
        Assert.Equal(1, account.SecurityVersion); Assert.False(account.MustChangePassword); Assert.Equal(0, await db.AuthSessions.CountAsync(x => x.AccountId == token.AccountId && x.RevokedAt != null, Ct)); Assert.Equal(0, await db.ReplayOperations.CountAsync(Ct));
        var audit = await db.AuditLogs.SingleAsync(x => x.Action == "STUDENT.PROFILE.UPDATE", Ct); Assert.DoesNotContain("Updated fixture", audit.Details!); Assert.DoesNotContain("Fixture address", audit.Details!); Assert.DoesNotContain("0922345678", audit.Details!);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task Profile_AuditFailureRollsBackBothRows()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); await Seed(f); var token = f.Token(await f.Grant("Student")); var executor = Profiles(f); var before = await executor.ReadAsync(token, Ct);
        var body = JsonSerializer.Serialize(new { fullName = "Must rollback", dateOfBirth = "2001-02-03", phone = "0922345678", address = "Must rollback", expectedVersions = before.Versions }, Wire);
        await using (var gate = await f.HoldAudit()) await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(() => executor.UpdateAsync(token, body, Ct));
        var after = await executor.ReadAsync(token, Ct); Assert.Equal(JsonSerializer.Serialize(before, Wire), JsonSerializer.Serialize(after, Wire));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task InitialZeroCorrectionPreservesProvenanceReplayNoEffectsAndEqualityNoMarker()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await Seed(f); var state = await Read(f, ids.Registration);
        var input = Input(state, ids.Booking, 70, "0"); var first = await Command(f, input); Assert.Equal("BAND_ASSIGNED", first.Task.RegistrationStatus); Assert.Equal("COMPLETED", first.Task.Booking.Status); Assert.Equal("0.00", first.Task.RelatedAttempts.Single().Score);
        Assert.Equal(first.OperationId, (await Command(f, input)).OperationId);
        state = await Read(f, ids.Registration); var initial = state.Attempts.Single(); var grader = initial.GradedBy; var gradedAt = initial.GradedAt; var completed = initial.CompletedBookingId;
        var correction = Input(state, ids.Booking, 72, "80.50", "Fixture correction"); await Command(f, correction);
        state = await Read(f, ids.Registration); Assert.Equal(80.50m, state.Attempts.Single().Score); Assert.Equal(grader, state.Attempts.Single().GradedBy); Assert.Equal(gradedAt, state.Attempts.Single().GradedAt); Assert.Equal(completed, state.Attempts.Single().CompletedBookingId);
        Assert.Equal(state.Attempts.Single().AttemptId, state.Band!.BestAttemptId);
        await Assert.ThrowsAsync<ApiErrorException>(() => Command(f, Input(state, ids.Booking, 72, "80.5", "No change")));
        await using var db = f.Db(); Assert.Equal(2, await db.ReplayOperations.CountAsync(Ct)); Assert.Equal(1, await db.EntryTestAttempts.CountAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task AbsencePreservesLogicalAttemptAndNullScoreReplayIsReadOnly()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await Seed(f); var input = Input(await Read(f, ids.Registration), ids.Booking, 71);
        var first = await Command(f, input); Assert.Equal("ABSENT", first.Task.Booking.Status); Assert.NotNull(first.Task.Absence); Assert.Null(first.Task.RelatedAttempts.Single().Score); Assert.Null(first.Task.BandAssignment);
        Assert.Equal(first.OperationId, (await Command(f, input)).OperationId); var state = await Read(f, ids.Registration); Assert.NotNull(EnrollmentProjection.BookAttempt(state)); Assert.Null(state.Attempts.Single().GradedBy); Assert.Null(state.Attempts.Single().CompletedBookingId);
        await using var db = f.Db(); Assert.Equal(1, await db.ReplayOperations.CountAsync(Ct)); Assert.Equal(1, await db.AuditLogs.CountAsync(x => x.Action == "TEACHER.ENTRY_BOOKING.ABSENCE", Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task TeacherAuditFailureRollsBackResultBandRegistrationAndCompletion()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await Seed(f); var input = Input(await Read(f, ids.Registration), ids.Booking, 70, "80");
        await using (var gate = await f.HoldAudit()) await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(() => Command(f, input));
        var state = await Read(f, ids.Registration); Assert.Null(state.Attempts.Single().Score); Assert.Null(state.Band); Assert.Equal("SCHEDULED", state.Booking(ids.Booking).Status); Assert.Equal("TESTING", state.Registration.Status);
        await using var db = f.Db(); Assert.Equal(0, await db.ReplayOperations.CountAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task TeacherScopePrecedesMalformedBodyAndReassignedHistoryIsDenied()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await Seed(f);
        var executor = new SqlTeacherEntryExecutor(() => f.Db(), f.Clock, 10, 1000);
        var teacher = f.Token(await f.Grant("Teacher")); var admin = f.Token(await f.Grant("Admin"));
        var forbidden = await Assert.ThrowsAsync<ApiErrorException>(() => executor.CommandAsync(admin, 70, ids.Booking, Guid.NewGuid(), "not-json", 65536, Ct)); Assert.Equal(ApiErrorCode.FORBIDDEN, forbidden.Code);
        var missing = await Assert.ThrowsAsync<ApiErrorException>(() => executor.CommandAsync(teacher, 70, int.MaxValue, Guid.NewGuid(), "not-json", 65536, Ct)); Assert.Equal(ApiErrorCode.NOT_FOUND, missing.Code);
        var state = await Read(f, ids.Registration); await Command(f, Input(state, ids.Booking, 70, "80")); state = await Read(f, ids.Registration);
        await using (var db = f.Db()) { var next = await db.Accounts.SingleAsync(x => x.AccountId == f.Accounts["Admin2"], Ct); next.RoleId = await db.Accounts.Where(x => x.AccountId == f.Accounts["Teacher"]).Select(x => x.RoleId).SingleAsync(Ct); await db.SaveChangesAsync(Ct); var session = await db.EntryTestSessions.SingleAsync(Ct); session.GraderId = next.AccountId; await db.SaveChangesAsync(Ct); }
        var outside = await Assert.ThrowsAsync<ApiErrorException>(() => executor.CommandAsync(teacher, 72, state.Attempts.Single().AttemptId, Guid.NewGuid(), "not-json", 65536, Ct)); Assert.Equal(ApiErrorCode.NOT_FOUND, outside.Code);
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task ConcurrentInitialGradesHaveOneCompleteResultAndOneStaleConflict()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var ids = await Seed(f); var state = await Read(f, ids.Registration);
        var body = JsonSerializer.SerializeToNode(TeacherTaskProjection.Manifest(state, state.Booking(ids.Booking), 70), Wire)!.AsObject(); body["score"] = "80";
        var token1 = f.Token(await f.Grant("Teacher")); var token2 = f.Token(await f.Grant("Teacher")); var executor = new SqlTeacherEntryExecutor(() => f.Db(), f.Clock, 10, 1000);
        async Task<string> Run(EnglishCenter.Api.Common.Security.VerifiedSessionToken token)
        { try { await executor.CommandAsync(token, 70, ids.Booking, Guid.NewGuid(), body.ToJsonString(), 65536, Ct); return "SUCCESS"; } catch (ApiErrorException e) { return e.Code.ToString(); } }
        var results = await Task.WhenAll(Run(token1), Run(token2)); Assert.Single(results, x => x == "SUCCESS"); Assert.Single(results, x => x == ApiErrorCode.STATE_CONFLICT.ToString());
        await using var db = f.Db(); Assert.Equal(1, await db.ReplayOperations.CountAsync(Ct)); Assert.Equal(1, await db.BandAssignments.CountAsync(Ct)); Assert.Equal(1, await db.AuditLogs.CountAsync(x => x.Action == "TEACHER.ENTRY_RESULT.CREATE", Ct));
    }
}

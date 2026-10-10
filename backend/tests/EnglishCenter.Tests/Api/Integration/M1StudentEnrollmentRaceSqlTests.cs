namespace EnglishCenter.Tests.Api.Integration;

using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

public sealed partial class M1StudentEnrollmentSqlTests
{
    private sealed class AdvanceClock : TimeProvider
    { public DateTimeOffset Now { get; set; } = new(2026, 10, 10, 3, 0, 0, TimeSpan.Zero); public override DateTimeOffset GetUtcNow() => Now; }
    private sealed class AdvanceAfterReplay(AdvanceClock clock, DateTimeOffset target) : DbCommandInterceptor
    {
        private bool fired;
        public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData data, DbDataReader reader, CancellationToken ct = default)
        {
            if (!fired && command.CommandText.Contains("INSERT", StringComparison.Ordinal) && command.CommandText.Contains("[ReplayOperation]", StringComparison.Ordinal)) { clock.Now = target; fired = true; }
            return ValueTask.FromResult(reader);
        }
    }
    private static async Task<string> Outcome(Func<Task<object>> work)
    { try { await work(); return "SUCCESS"; } catch (ApiErrorException e) { return e.Code.ToString(); } }

    [Theory(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    [InlineData(false)][InlineData(true)]
    public async Task StudentDecisionAndTeacherResultSerializeWithoutPartialEffects(bool confirmation)
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock());
        var course = await Seed(f);
        var student = f.Token(await f.Grant("Student"));
        var teacher = f.Token(await f.Grant("Teacher"));
        var reg = (await Create(f, student, course)).Registration.Id;
        var session = await Session(f, 2);
        var booked = (OwnBookingResult)await Executor(f).CommandAsync(student, 30, reg, 0, Guid.NewGuid(), (await BookingBody(f, await Read(f, reg), session)).ToJsonString(), 65536, Ct);
        await using (var db = f.Db())
        {
            var s = await db.EntryTestSessions.SingleAsync(Ct);
            s.StartAt = f.Clock.GetUtcNow().UtcDateTime.AddHours(-1);
            s.EndAt = f.Clock.GetUtcNow().UtcDateTime;
            s.Status = "CLOSED";
            await db.SaveChangesAsync(Ct);
        }
        var executor = new SqlTeacherEntryExecutor(() => f.Db(), f.Clock, 10, 1000);
        var state = await Read(f, reg);
        if (confirmation)
        {
            var initial = JsonSerializer.SerializeToNode(TeacherTaskProjection.Manifest(state, state.Booking(booked.Booking.Id), 70), Wire)!.AsObject();
            initial["score"] = "80";
            await executor.CommandAsync(teacher, 70, booked.Booking.Id, Guid.NewGuid(), initial.ToJsonString(), 65536, Ct);
            state = await Read(f, reg);
        }
        var teacherApi = confirmation ? 72 : 70;
        var teacherBody = JsonSerializer.SerializeToNode(TeacherTaskProjection.Manifest(state, state.Booking(booked.Booking.Id), teacherApi), Wire)!.AsObject();
        teacherBody["score"] = confirmation ? "90" : "80";
        if (confirmation) teacherBody["reason"] = "Synthetic correction race";
        var studentBody = JsonSerializer.Serialize(confirmation ? EnrollmentProjection.ConfirmationManifest(state) : EnrollmentProjection.CancellationManifest(state, f.Clock.GetUtcNow()), Wire);
        var originalGradedAt = state.Attempts.Single().GradedAt;
        var results = await Task.WhenAll(
            Outcome(() => Executor(f).CommandAsync(student, confirmation ? 36 : 28, reg, 0, Guid.NewGuid(), studentBody, 65536, Ct)),
            Outcome(async () => await executor.CommandAsync(teacher, teacherApi, confirmation ? booked.Booking.AttemptId : booked.Booking.Id, Guid.NewGuid(), teacherBody.ToJsonString(), 65536, Ct)));
        Assert.Single(results, x => x == "SUCCESS");
        Assert.Single(results, x => x == ApiErrorCode.STATE_CONFLICT.ToString());
        state = await Read(f, reg);
        await using var verify = f.Db();
        Assert.Equal(confirmation ? 4 : 3, await verify.ReplayOperations.CountAsync(Ct));
        if (results[0] == "SUCCESS")
        {
            Assert.Equal(confirmation ? "CONFIRMED" : "CANCELLED", state.Registration.Status);
            Assert.Equal(confirmation ? 80m : (decimal?)null, state.Attempts.Single().Score);
            Assert.Equal(confirmation ? "COMPLETED" : "SCHEDULED", state.Bookings.Single().Status);
        }
        else
        {
            Assert.Equal("BAND_ASSIGNED", state.Registration.Status);
            Assert.Equal(confirmation ? 90m : 80m, state.Attempts.Single().Score);
            Assert.Equal("COMPLETED", state.Bookings.Single().Status);
            Assert.NotNull(state.Band);
            Assert.Null(state.Registration.AdmissionConfirmedAt);
            Assert.Null(state.Registration.CancelledAt);
        }
        if (confirmation) Assert.Equal(originalGradedAt, state.Attempts.Single().GradedAt);
        Assert.Equal(0, await verify.Invoices.CountAsync(Ct));
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task ConcurrentRegistrationCreationHasExactlyOneUnfinishedAndOneCompletion()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var course = await Seed(f); var first = f.Token(await f.Grant("Student")); var second = f.Token(await f.Grant("Student")); var body = JsonSerializer.Serialize(new { courseId = course });
        var results = await Task.WhenAll(Outcome(() => Executor(f).CommandAsync(first, 25, 0, 0, Guid.NewGuid(), body, 65536, Ct)), Outcome(() => Executor(f).CommandAsync(second, 25, 0, 0, Guid.NewGuid(), body, 65536, Ct)));
        Assert.Single(results, x => x == "SUCCESS"); Assert.Single(results, x => x == ApiErrorCode.STATE_CONFLICT.ToString());
        await using var db = f.Db(); Assert.Equal(1, await db.Registrations.CountAsync(x => x.IsUnfinished, Ct)); Assert.Equal(1, await db.ReplayOperations.CountAsync(Ct));
    }
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task TwoStudentsCompeteForLastSeatWithoutSessionVersionChange()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock()); var course = await Seed(f); var token1 = f.Token(await f.Grant("Student"));
        await using (var seed = f.Db())
        {
            var original = await seed.Accounts.SingleAsync(x => x.AccountId == token1.AccountId, Ct);
            var other = new Account { RoleId = original.RoleId, FullName = "Second fixture", Email = "secondstudent@example.invalid", Phone = "0922345678", PasswordHash = original.PasswordHash, Status = "ACTIVE", SecurityVersion = 1, CreatedAt = f.Clock.GetUtcNow().UtcDateTime };
            seed.Accounts.Add(other); await seed.SaveChangesAsync(Ct); seed.StudentProfiles.Add(new StudentProfile { AccountId = other.AccountId, DateOfBirth = new(2000, 1, 1) }); await seed.SaveChangesAsync(Ct);
        }
        var grant = await f.Login.ExecuteAsync(LoginInput.Parse(JsonSerializer.Serialize(new { email = "secondstudent@example.invalid", password = f.Password })), Ct); Assert.True(grant.Accepted); var token2 = f.Token(grant.Grant!);
        var reg1 = (await Create(f, token1, course)).Registration.Id; var reg2 = (await Create(f, token2, course)).Registration.Id; var session = await Session(f, 2, 1);
        var body1 = await BookingBody(f, await Read(f, reg1), session); var body2 = await BookingBody(f, await Read(f, reg2), session);
        var results = await Task.WhenAll(Outcome(() => Executor(f).CommandAsync(token1, 30, reg1, 0, Guid.NewGuid(), body1.ToJsonString(), 65536, Ct)), Outcome(() => Executor(f).CommandAsync(token2, 30, reg2, 0, Guid.NewGuid(), body2.ToJsonString(), 65536, Ct)));
        Assert.Single(results, x => x == "SUCCESS"); Assert.Single(results, x => x == ApiErrorCode.STATE_CONFLICT.ToString());
        await using var db = f.Db(); Assert.Equal(1, await db.EntryTestBookings.CountAsync(x => x.SessionId == session && x.Status == "SCHEDULED", Ct)); Assert.Equal(1, await db.EntryTestAttempts.CountAsync(Ct));
        Assert.Equal(body1["expectedVersions"]!["targetSession"]!.GetValue<string>(), EnrollmentProjection.Rv((await db.EntryTestSessions.SingleAsync(Ct)).RowVersion));
    }
    [Theory(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    [InlineData(false)][InlineData(true)]
    public async Task FinalClockCrossingStartRollsBackBookingOrRegistrationCancellation(bool cancellation)
    {
        var time = new AdvanceClock(); await using var f = await OwnedSqlFixture.CreateAsync(time); var course = await Seed(f); var token = f.Token(await f.Grant("Student")); var reg = (await Create(f, token, course)).Registration.Id;
        int session; await using (var db = f.Db()) { var s = new EntryTestSession { StartAt = time.Now.UtcDateTime.AddMinutes(1), EndAt = time.Now.UtcDateTime.AddMinutes(2), Room = "CLOCK", Capacity = 20, GraderId = f.Accounts["Teacher"], Status = "OPEN" }; db.EntryTestSessions.Add(s); await db.SaveChangesAsync(Ct); session = s.SessionId; }
        if (cancellation) await Executor(f).CommandAsync(token, 30, reg, 0, Guid.NewGuid(), (await BookingBody(f, await Read(f, reg), session)).ToJsonString(), 65536, Ct);
        var before = await Read(f, reg); var body = cancellation ? JsonSerializer.Serialize(EnrollmentProjection.CancellationManifest(before, time.Now), Wire) : (await BookingBody(f, before, session)).ToJsonString();
        var interceptor = new AdvanceAfterReplay(time, time.Now.AddMinutes(1)); var executor = new SqlStudentEnrollmentExecutor(() => f.Db(interceptor), f.Clock, 10, 1000);
        var error = await Assert.ThrowsAsync<ApiErrorException>(() => executor.CommandAsync(token, cancellation ? 28 : 30, reg, 0, Guid.NewGuid(), body, 65536, Ct)); Assert.Equal(ApiErrorCode.STATE_CONFLICT, error.Code);
        var after = await Read(f, reg); Assert.Equal(before.Registration.Status, after.Registration.Status); Assert.Equal(before.Registration.RowVersion, after.Registration.RowVersion); Assert.Equal(before.Bookings.Count, after.Bookings.Count); Assert.Equal(before.Attempts.Count, after.Attempts.Count); if(cancellation)Assert.Equal("SCHEDULED", after.Bookings.Single().Status);
        await using var verify = f.Db(); Assert.Equal(cancellation ? 2 : 1, await verify.ReplayOperations.CountAsync(Ct)); Assert.Equal(0, await verify.AuditLogs.CountAsync(x => x.Action == "STUDENT.REGISTRATION.CANCEL", Ct));
    }
}

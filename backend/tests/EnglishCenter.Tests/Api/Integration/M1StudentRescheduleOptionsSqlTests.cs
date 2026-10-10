namespace EnglishCenter.Tests.Api.Integration;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Infrastructure.Enrollment;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed partial class M1StudentEnrollmentSqlTests
{
    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task RescheduleOptionsBindOwnCurrentBookingAndExcludeOnlyReplacedInterval()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock());
        var course = await Seed(f); var token = f.Token(await f.Grant("Student"));
        var reg = (await Create(f, token, course)).Registration.Id;
        var source = await Session(f, 2); var target = await Session(f, 2);
        await using (var db = f.Db())
        {
            var teacher = await db.Accounts.SingleAsync(x => x.AccountId == f.Accounts["Teacher"], Ct);
            var other = await db.Accounts.SingleAsync(x => x.AccountId == f.Accounts["Director"], Ct);
            other.RoleId = teacher.RoleId;
            var slot = await db.EntryTestSessions.SingleAsync(x => x.SessionId == target, Ct);
            slot.GraderId = other.AccountId; slot.Room = "REPLACEMENT";
            await db.SaveChangesAsync(Ct);
        }
        var booked = (OwnBookingResult)await Executor(f).CommandAsync(token, 30, reg, 0, Guid.NewGuid(), (await BookingBody(f, await Read(f, reg), source)).ToJsonString(), 65536, Ct);
        var reader = new SqlStudentEnrollmentReader(() => f.Db(), f.Clock, 10, 1000);
        Assert.False((await reader.OptionsAsync(token, reg, QueryCollection.Empty, Ct)).Eligible);
        var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { ["bookingId"] = booked.Booking.Id.ToString() });
        var options = await reader.OptionsAsync(token, reg, query, Ct);
        Assert.True(options.Eligible); Assert.Equal(booked.Booking.Id, options.AttemptContext!.CurrentBookingId);
        Assert.Equal(booked.Booking.AttemptId, options.AttemptContext.AttemptId);
        Assert.DoesNotContain(options.Items, x => x.Id == source); Assert.Contains(options.Items, x => x.Id == target);
        var missing = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { ["bookingId"] = int.MaxValue.ToString() });
        Assert.Equal(ApiErrorCode.NOT_FOUND, (await Assert.ThrowsAsync<ApiErrorException>(() => reader.OptionsAsync(token, reg, missing, Ct))).Code);
        await using (var db = f.Db()) { var slot = await db.EntryTestSessions.SingleAsync(x => x.SessionId == source, Ct); slot.Status = "CLOSED"; await db.SaveChangesAsync(Ct); }
        var blocked = await reader.OptionsAsync(token, reg, query, Ct);
        Assert.False(blocked.Eligible); Assert.Null(blocked.AttemptContext); Assert.Empty(blocked.Items);
    }

    [Fact(Skip = OptIn, SkipUnless = nameof(OwnedSqlGate.Enabled), SkipType = typeof(OwnedSqlGate))]
    public async Task FullTargetAfterOptionsReadPreservesOriginalBookingAndAttempt()
    {
        await using var f = await OwnedSqlFixture.CreateAsync(new Clock());
        var course = await Seed(f); var token = f.Token(await f.Grant("Student"));
        var reg = (await Create(f, token, course)).Registration.Id;
        var source = await Session(f, 2); var target = await Session(f, 4, 1);
        var booked = (OwnBookingResult)await Executor(f).CommandAsync(token, 30, reg, 0, Guid.NewGuid(), (await BookingBody(f, await Read(f, reg), source)).ToJsonString(), 65536, Ct);
        var before = await Read(f, reg);
        await using var db = f.Db();
        var slotVersion = EnrollmentProjection.Rv((await db.EntryTestSessions.SingleAsync(x => x.SessionId == target, Ct)).RowVersion);
        var secondCourse = new Course { CourseCode = "SECOND", CourseName = "Synthetic", BasePrice = 1000, Status = "ACTIVE" };
        db.Courses.Add(secondCourse); await db.SaveChangesAsync(Ct);
        var otherReg = (await Create(f, token, secondCourse.CourseId)).Registration.Id;
        await Executor(f).CommandAsync(token, 30, otherReg, 0, Guid.NewGuid(), (await BookingBody(f, await Read(f, otherReg), target)).ToJsonString(), 65536, Ct);
        var error = await Assert.ThrowsAsync<ApiErrorException>(() => Executor(f).CommandAsync(token, 32, reg, booked.Booking.Id, Guid.NewGuid(), CancelBody(before, booked.Booking.Id, target, slotVersion), 65536, Ct));
        Assert.Equal(ApiErrorCode.STATE_CONFLICT, error.Code);
        var after = await Read(f, reg); Assert.Single(after.Bookings); Assert.Single(after.Attempts);
        Assert.Equal("SCHEDULED", after.Bookings.Single().Status); Assert.Equal(before.Bookings.Single().RowVersion, after.Bookings.Single().RowVersion);
        Assert.Equal(0, await db.AuditLogs.CountAsync(x => x.Action == "STUDENT.ENTRY_BOOKING.RESCHEDULE", Ct));
    }
}

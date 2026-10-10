namespace EnglishCenter.Tests.Common;

using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Services.Enrollment;
using Xunit;

public sealed partial class M1EnrollmentContractTests
{
    private static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web);
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 3, 0, 0, TimeSpan.Zero);
    private static byte[] Version(byte value) => [0, 0, 0, 0, 0, 0, 0, value];
    private static RegistrationState State(bool result = false, string bookingStatus = "SCHEDULED", bool second = false)
    {
        var student = new StudentProfile { StudentId = 1, AccountId = 2, DateOfBirth = new(2000, 1, 1), RowVersion = Version(1) };
        var account = new Account { AccountId = 2, FullName = "Student fixture", Email = "fixture@example.invalid", Phone = "0912345678", RowVersion = Version(2) };
        var reg = new Registration { RegistrationId = 3, StudentId = 1, CourseId = 4, Status = result ? "BAND_ASSIGNED" : "TESTING", CreatedAt = Now.UtcDateTime.AddDays(-1), RowVersion = Version(3) };
        var course = new Course { CourseId = 4, CourseCode = "C1", CourseName = "Course fixture", Status = "ACTIVE" };
        var session = new EntryTestSession { SessionId = 5, GraderId = 6, Status = "CLOSED", StartAt = Now.UtcDateTime.AddHours(-1), EndAt = Now.UtcDateTime, Room = "R1", RowVersion = Version(5) };
        var attempt = new EntryTestAttempt { AttemptId = 7, RegistrationId = 3, AttemptNumber = 1, RowVersion = Version(7), Score = result ? 0m : null, GradedAt = result ? Now.UtcDateTime : null, GradedBy = result ? 6 : null, CompletedBookingId = result ? 8 : null };
        var booking = new EntryTestBooking { BookingId = 8, AttemptId = 7, SessionId = 5, Status = result ? "COMPLETED" : bookingStatus, BookedAt = Now.UtcDateTime.AddHours(-2), RowVersion = Version(8) };
        var band = new Band { BandId = 9, BandCode = "B1", MinScore = 0m };
        BandAssignment[] assigned = result ? [new() { BandAssignmentId = 10, RegistrationId = 3, BestAttemptId = 7, BandId = 9, AssignedAt = Now.UtcDateTime, RowVersion = Version(10) }] : [];
        EntryTestAttempt[] attempts = second ? [attempt, new() { AttemptId = 11, RegistrationId = 3, AttemptNumber = 2, RowVersion = Version(11) }] : [attempt];
        return new(new(student, account, [course], [reg], [session], attempts, [booking], assigned, [band], [], [], [], []), reg);
    }

    private static string ProfileJson() => "{\"fullName\":\" Fixture name \",\"dateOfBirth\":\"2000-01-01\",\"phone\":\"+84912345678\",\"address\":null,\"expectedVersions\":{\"account\":\"AAAAAAAAAAE=\",\"profile\":\"AAAAAAAAAAI=\"}}";
    [Fact]
    public void ProfileFullSetNormalizesAndKeepsOpaqueCopies()
    {
        var input = OwnProfileInput.Parse(ProfileJson()); Assert.Equal("Fixture name", input.FullName); Assert.Equal("0912345678", input.Phone); Assert.Null(input.Address);
        var copy = input.CopyAccountVersion(); copy[7] = 99; Assert.Equal(1, input.CopyAccountVersion()[7]); Assert.Equal(nameof(OwnProfileInput), input.ToString());
    }
    [Theory]
    [InlineData("email")][InlineData("accountId")][InlineData("studentId")][InlineData("securityVersion")][InlineData("operationId")]
    public void ProfileRejectsImmutableOrAuthorityFields(string field)
    { var body = JsonNode.Parse(ProfileJson())!.AsObject(); body[field] = 1; Assert.Throws<ApiErrorException>(() => OwnProfileInput.Parse(body.ToJsonString())); }
    [Theory]
    [InlineData("fullName")][InlineData("dateOfBirth")][InlineData("phone")][InlineData("address")][InlineData("expectedVersions")]
    public void ProfileRequiresEveryEditableFieldIncludingNullableAddress(string field)
    { var body = JsonNode.Parse(ProfileJson())!.AsObject(); body.Remove(field); Assert.Throws<ApiErrorException>(() => OwnProfileInput.Parse(body.ToJsonString())); }
    [Fact]
    public void ProfileRejectsDuplicateNestedVersion()
    { Assert.Throws<ApiErrorException>(() => OwnProfileInput.Parse(ProfileJson().Replace("\"profile\":", "\"account\":\"AAAAAAAAAAE=\",\"profile\":"))); }
    [Theory]
    [InlineData("not-base64")][InlineData("AAAA")][InlineData("AAAAAAAAAAA")]
    public void ProfileRejectsNonRowVersions(string token)
    { Assert.Throws<ApiErrorException>(() => OwnProfileInput.Parse(ProfileJson().Replace("AAAAAAAAAAI=", token))); }

    [Fact]
    public void TeacherEndBoundaryIsInclusiveAndClosedDoesNotDisableGrade()
    {
        var s = State(); Assert.False(TeacherTaskProjection.CanInitial(s, s.Booking(8), Now.AddTicks(-1)));
        Assert.True(TeacherTaskProjection.CanInitial(s, s.Booking(8), Now)); Assert.True(TeacherTaskProjection.CanInitial(s, s.Booking(8), Now.AddDays(30)));
    }
    [Theory]
    [InlineData("CONFIRMED")][InlineData("ADMITTED")][InlineData("REJECTED")][InlineData("CANCELLED")]
    public void SealedRegistrationDisablesAllTeacherMutations(string status)
    { var s = State(); s.Registration.Status = status; Assert.False(TeacherTaskProjection.CanInitial(s, s.Booking(8), Now)); Assert.Empty(TeacherTaskProjection.Detail(s, s.Booking(8), 6, Now).AllowedActions); }
    [Fact]
    public void CancelledSessionDisablesGradeWithoutDeletingHistory()
    { var s = State(true); s.Session(5).Status = "CANCELLED"; var task = TeacherTaskProjection.Detail(s, s.Booking(8), 6, Now); Assert.NotNull(task.TargetResult); Assert.Empty(task.AllowedActions); Assert.Null(task.CorrectionManifest); }
    [Fact]
    public void LostAssignmentDeniesCompletedHistoryBeforeProjection()
    { var s = State(true); s.Session(5).GraderId = 99; var error = Assert.Throws<ApiErrorException>(() => TeacherTaskProjection.Detail(s, s.Booking(8), 6, Now)); Assert.Equal(ApiErrorCode.NOT_FOUND, error.Code); }
    [Fact]
    public void ZeroIsCompletedResultAndAbsenceIsNotZero()
    {
        var completed = TeacherTaskProjection.Detail(State(true), State(true).Booking(8), 6, Now);
        Assert.Equal("0.00", JsonSerializer.SerializeToElement(completed.TargetResult, Wire).GetProperty("score").GetString());
        var absent = State(false, "ABSENT"); absent.Booking(8).AbsentAt = Now.UtcDateTime; absent.Booking(8).AbsentBy = 6;
        var dto = TeacherTaskProjection.Detail(absent, absent.Booking(8), 6, Now); Assert.Null(dto.TargetResult); Assert.NotNull(dto.Absence); Assert.Null(dto.RelatedAttempts[0].Score); Assert.NotNull(EnrollmentProjection.BookAttempt(absent));
    }
    [Fact]
    public void CancellationAndAbsenceReuseLogicalAttempt()
    { foreach (var status in new[] { "ABSENT", "CANCELLED" }) { var s = State(false, status); var context = EnrollmentProjection.BookAttempt(s); Assert.Equal(1, context!.AttemptNumber); Assert.Equal(7, context.AttemptId); } }
    [Fact]
    public void SecondAttemptRequiresFirstCompletedAndNeverCreatesThird()
    {
        var s = State(true); Assert.Equal(2, EnrollmentProjection.BookAttempt(s)!.AttemptNumber);
        var two = State(true, second: true); Assert.Equal(11, EnrollmentProjection.BookAttempt(two)!.AttemptId);
        var second = two.Attempt(11); second.Score = 80m; second.CompletedBookingId = 12; Assert.Null(EnrollmentProjection.BookAttempt(two));
    }
    [Fact]
    public void ScheduledRetakePreventsConfirmationEvenClosed()
    {
        var s = State(true); Assert.True(EnrollmentProjection.CanConfirm(s));
        s.Booking(8).Status = "SCHEDULED"; Assert.False(EnrollmentProjection.CanConfirm(s)); Assert.Null(EnrollmentProjection.BookAttempt(s));
    }
    [Fact]
    public void EqualCompletedScoresSelectAttemptOne()
    {
        var s = State(true, second: true); var second = s.Attempt(11); second.Score = 0m; second.CompletedBookingId = 12;
        var next = new EntryTestBooking { BookingId = 12, AttemptId = 11, SessionId = 5, Status = "COMPLETED", RowVersion = Version(12) };
        var state = new RegistrationState(s.Set with { Bookings = [s.Booking(8), next] }, s.Registration);
        Assert.Equal(7, TeacherTaskProjection.BestCompleted(state).AttemptId);
        next.Status = "ABSENT"; second.Score = 100m; Assert.Equal(7, TeacherTaskProjection.BestCompleted(state).AttemptId);
    }
    [Fact]
    public void CorrectionRequiresOriginalAndCurrentTeacherAndPreservesSourceProvenance()
    {
        var s = State(true); Assert.True(TeacherTaskProjection.CanCorrect(s, s.Booking(8), 6, Now));
        var dto = TeacherTaskProjection.Detail(s, s.Booking(8), 6, Now); var provenance = JsonSerializer.SerializeToElement(dto.TargetResult, Wire);
        Assert.Equal(6, provenance.GetProperty("originalGraderId").GetInt32());
        s.Attempt(7).GradedBy = 99; Assert.False(TeacherTaskProjection.CanCorrect(s, s.Booking(8), 6, Now));
        Assert.NotNull(TeacherTaskProjection.Detail(s, s.Booking(8), 6, Now).TargetResult);
    }
    [Theory]
    [InlineData(70)][InlineData(71)][InlineData(72)]
    public void ServerManifestBindsExistingCanonicalSchema(int api)
    {
        var s = State(api == 72); var body = JsonSerializer.SerializeToNode(TeacherTaskProjection.Manifest(s, s.Booking(8), api), Wire)!.AsObject();
        if (api != 71) body["score"] = "80.5"; if (api == 72) body["reason"] = "Fixture correction";
        var input = EnrollmentInput.Parse(api, 0, api == 72 ? 7 : 8, Guid.NewGuid(), body.ToJsonString(), 16384);
        using var envelope = JsonDocument.Parse(input.Intent.CopyBytes());
        TeacherTaskProjection.CheckManifest(s, s.Booking(8), envelope.RootElement.GetProperty("body"), api);
        Assert.Equal(nameof(EnrollmentInput), input.ToString());
    }
    [Fact]
    public void AbsenceManifestHasNoBandAuthority()
    {
        var s = State(); var manifest = JsonSerializer.SerializeToElement(TeacherTaskProjection.Manifest(s, s.Booking(8), 71), Wire);
        Assert.False(manifest.GetProperty("expectedVersions").TryGetProperty("bandAssignment", out _));
        Assert.False(manifest.GetProperty("expectedSet").TryGetProperty("bandAssignmentId", out _));
    }
    [Fact]
    public void FullAttemptManifestRejectsChildOnlyStalenessAndMissingMember()
    {
        var s = State(false, second: true); var doc = JsonSerializer.SerializeToElement(TeacherTaskProjection.Manifest(s, s.Booking(8), 70), Wire);
        s.Attempt(11).RowVersion = Version(99); Assert.Throws<ApiErrorException>(() => TeacherTaskProjection.CheckManifest(s, s.Booking(8), doc, 70));
        s.Attempt(11).RowVersion = Version(11); var subset = JsonNode.Parse(doc.GetRawText())!.AsObject(); subset["expectedVersions"]!["relatedAttempts"]!.AsArray().RemoveAt(1);
        Assert.Throws<ApiErrorException>(() => TeacherTaskProjection.CheckManifest(s, s.Booking(8), JsonSerializer.SerializeToElement(subset), 70));
    }
    [Fact]
    public void TeacherDtoNeverSerializesFinanceOrProfileEntities()
    {
        var s = State(true); var dto = JsonSerializer.SerializeToElement(TeacherTaskProjection.Detail(s, s.Booking(8), 6, Now), Wire);
        var json = dto.GetRawText(); Assert.DoesNotContain("invoice", json, StringComparison.OrdinalIgnoreCase); Assert.DoesNotContain("dateOfBirth", json); Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(15, dto.EnumerateObject().Count());
    }
    [Fact]
    public void ReadOnlyEffectiveCloseDoesNotMutateSession()
    { var s = State(); s.Session(5).Status = "OPEN"; Assert.Equal("CLOSED", EnrollmentProjection.Session(s.Session(5), Now).EffectiveStatus); Assert.Equal("OPEN", s.Session(5).Status); }
}

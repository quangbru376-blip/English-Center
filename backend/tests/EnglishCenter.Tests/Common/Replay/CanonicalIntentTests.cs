namespace EnglishCenter.Tests.Common.Replay;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using Xunit;

public class CanonicalIntentTests
{
    private static CanonicalIntent Bind(int api, string? json = null) => CanonicalIntent.Bind(api, json ?? CanonicalFixtures.For(api), CanonicalActionRegistry.ForApi(api).TargetKeys.ToDictionary(k => k, _ => 1), 65536);
    public static TheoryData<int> Apis => new(CanonicalActionRegistry.All.Select(a => a.ApiId));
    [Theory]
    [MemberData(nameof(Apis))]
    public void EachAcceptedActionHasAnExactShapeAndRejectsUnknownMissingAndDuplicateFields(int api)
    {
        var source = CanonicalFixtures.For(api); var intent = Bind(api, source); Assert.Equal(32, intent.CopyDigest().Length);
        var altered = JsonNode.Parse(source)!.AsObject(); altered["actor"] = 1; Assert.Throws<ApiErrorException>(() => Bind(api, altered.ToJsonString()));
        altered = JsonNode.Parse(source)!.AsObject(); var first = altered.First().Key; altered.Remove(first); Assert.Throws<ApiErrorException>(() => Bind(api, altered.ToJsonString()));
        using var doc = JsonDocument.Parse(source); var value = doc.RootElement.GetProperty(first).GetRawText(); Assert.Throws<ApiErrorException>(() => Bind(api, source[..^1] + ",\"" + first + "\":" + value + "}"));
        var envelope = JsonNode.Parse(intent.CopyBytes())!; Assert.Equal(CanonicalActionRegistry.ForApi(api).Code, envelope["action"]!.GetValue<string>()); Assert.Equal("EC-C1", envelope["contract"]!.GetValue<string>());
        var bytes = intent.CopyBytes(); bytes[0] = 0; Assert.Equal((byte)'{', intent.CopyBytes()[0]); var digest = intent.CopyDigest(); digest[0] ^= 255; Assert.Equal(SHA256.HashData(intent.CopyBytes()), intent.CopyDigest()); Assert.Equal("CanonicalIntent", intent.ToString());
    }
    [Theory]
    [InlineData(25, 93, "4faede809c38071f3a7c46f03466cd09a83e02f0c7d22ba63cc12c9f66c12542")]
    [InlineData(9, 141, "5c14cad40c163d9da7f78016fbedae387ef6c5df3b5306b7dc6001b69f9c6de5")]
    [InlineData(8, 162, "d08e9ffbc57a80bdbe1496c583febff7b7515a43fde33ef7218cc8d49718fe56")]
    public void FrozenAcceptedLiteralByteLengthsAndHashes(int api, int size, string hash)
    {
        var targets = CanonicalActionRegistry.ForApi(api).TargetKeys.ToDictionary(k => k, _ => 42);
        var intent = CanonicalIntent.Bind(api, CanonicalFixtures.For(api), targets, 65536); Assert.Equal(size, intent.CopyBytes().Length); Assert.Equal(hash, Convert.ToHexStringLower(intent.CopyDigest()));
    }
    [Fact]
    public void FrozenF5BusinessDifferenceAndPropertyOrderEquality()
    {
        var first = Bind(25); var same = Bind(25, " { \"courseId\" : 7 } "); Assert.Equal(first.CopyBytes(), same.CopyBytes()); var other = Bind(25, "{\"courseId\":8}"); Assert.Equal("a823e7339a2161e37d829a56623eac2e31bbb2062912e9a7658ed4a3f7842c0c", Convert.ToHexStringLower(other.CopyDigest())); Assert.NotEqual(first.CopyDigest(), other.CopyDigest());
    }
    [Fact]
    public void NormalizedTextPhoneScoreSetsAndOptionalPresenceFollowAcceptedRules()
    {
        var staff = JsonNode.Parse(CanonicalFixtures.For(8))!.AsObject(); staff["fullName"] = " Nguye\u0302\u0303n An "; staff["email"] = " DEMO.TEACHER@EXAMPLE.TEST "; staff.Remove("phone"); Assert.Equal(Bind(8).CopyBytes(), Bind(8, staff.ToJsonString()).CopyBytes());
        staff["phone"] = "+84 912 345 678"; var p1 = Bind(8, staff.ToJsonString()); staff["phone"] = "0912345678"; Assert.Equal(p1.CopyBytes(), Bind(8, staff.ToJsonString()).CopyBytes());
        var grade = JsonNode.Parse(CanonicalFixtures.For(139))!.AsObject(); grade["scores"]![0]!["score"] = "80.5"; Assert.Equal(Bind(139).CopyBytes(), Bind(139, grade.ToJsonString()).CopyBytes()); grade["teacherComment"] = null; Assert.NotEqual(Bind(139).CopyDigest(), Bind(139, grade.ToJsonString()).CopyDigest());
        var attendance = JsonNode.Parse(CanonicalFixtures.For(133))!.AsObject(); attendance["changes"]!.AsArray().Add(new JsonObject { ["classAssignmentId"] = 2, ["present"] = 1 }); attendance["expectedSet"]!["assignmentIds"]!.AsArray().Add(2); attendance["expectedSet"]!["unrecordedAssignmentIds"]!.AsArray().Add(2);
        var original = Bind(133, attendance.ToJsonString()); foreach (var key in new[] { "assignmentIds", "unrecordedAssignmentIds" }) { var a = attendance["expectedSet"]![key]!.AsArray(); var items = a.Select(n => n!.DeepClone()).Reverse().ToArray(); a.Clear(); foreach (var n in items) a.Add(n); }
        var changes = attendance["changes"]!.AsArray(); var values = changes.Select(n => n!.DeepClone()).Reverse().ToArray(); changes.Clear(); foreach (var n in values) changes.Add(n); Assert.Equal(original.CopyBytes(), Bind(133, attendance.ToJsonString()).CopyBytes());
    }
    [Theory]
    [InlineData("{\"courseId\":1e0}")]
    [InlineData("{\"courseId\":1.0}")]
    [InlineData("{\"courseId\":\"7\"}")]
    [InlineData("{\"courseId\":null}")]
    [InlineData("{\"courseId\":true}")]
    [InlineData("{\"courseId\":0}")]
    public void CoercionInvalidScalarNeverProducesIntent(string input) => Assert.Throws<ApiErrorException>(() => Bind(25, input));
    [Fact]
    public void VersionTargetActionAbsenceAndSecretInputsCannotBeSilentlyChanged()
    {
        var first = Bind(9); var body = JsonNode.Parse(CanonicalFixtures.For(9))!; body["expectedVersions"]!["account"] = "AQECAwQFBgc="; Assert.NotEqual(first.CopyDigest(), Bind(9, body.ToJsonString()).CopyDigest());
        Assert.NotEqual(first.CopyDigest(), CanonicalIntent.Bind(9, CanonicalFixtures.For(9), new Dictionary<string, int> { { "accountId", 2 } }, 65536).CopyDigest()); Assert.NotEqual(first.CopyDigest(), Bind(10).CopyDigest());
        body["password"] = "secret must be rejected"; Assert.Throws<ApiErrorException>(() => Bind(9, body.ToJsonString())); Assert.Throws<ApiErrorException>(() => CanonicalIntent.Bind(4, "{}", new Dictionary<string, int>(), 100));
        Assert.Throws<ApiErrorException>(() => CanonicalIntent.Bind(25, CanonicalFixtures.For(25), new Dictionary<string, int> { { "accountId", 1 } }, 100));
    }
    [Fact]
    public void BadUnicodeUnknownNestedFieldsDuplicateBindingsAndOverprecisionFailClosed()
    {
        var staff = CanonicalFixtures.For(8).Replace("Nguyễn An", "\\ud800"); Assert.Throws<ApiErrorException>(() => Bind(8, staff));
        var grade = JsonNode.Parse(CanonicalFixtures.For(139))!; grade["scores"]![0]!["score"] = "80.500"; Assert.Throws<ApiErrorException>(() => Bind(139, grade.ToJsonString()));
        grade = JsonNode.Parse(CanonicalFixtures.For(139))!; grade["expectedVersions"]!["components"]!.AsArray().Add(grade["expectedVersions"]!["components"]![0]!.DeepClone()); Assert.Throws<ApiErrorException>(() => Bind(139, grade.ToJsonString()));
        var delta = JsonNode.Parse(CanonicalFixtures.For(133))!; delta["expectedSet"]!["unrecordedAssignmentIds"]!.AsArray().Clear(); Assert.Throws<ApiErrorException>(() => Bind(133, delta.ToJsonString())); delta = JsonNode.Parse(CanonicalFixtures.For(133))!; delta["expectedVersions"]!["actor"] = 1; Assert.Throws<ApiErrorException>(() => Bind(133, delta.ToJsonString()));
    }
    [Fact]
    public void WeeklyOrderedPlanMustMatchEveryRuleDateAndCannotBeSortedToRepairIntent()
    {
        var body = JsonNode.Parse(CanonicalFixtures.For(120))!; body["rule"]!["toDate"] = "2026-10-13"; var next = body["candidates"]![0]!.DeepClone(); next["date"] = "2026-10-13"; body["candidates"]!.AsArray().Add(next); Assert.NotNull(Bind(120, body.ToJsonString()));
        var a = body["candidates"]!.AsArray(); var items = a.Select(n => n!.DeepClone()).Reverse().ToArray(); a.Clear(); foreach (var n in items) a.Add(n); Assert.Throws<ApiErrorException>(() => Bind(120, body.ToJsonString()));
        body = JsonNode.Parse(CanonicalFixtures.For(120))!; body["candidates"]!.AsArray().Clear(); Assert.Throws<ApiErrorException>(() => Bind(120, body.ToJsonString()));
    }
    [Fact]
    public void StringBytesAreStrictLiteralUtf8WithEveryControlEscape()
    {
        var input = "\"\\/Nguyễn\u2028\u2029😀"; var expected = "\"\\\"\\\\/Nguyễn\u2028\u2029😀\""; Assert.Equal(Encoding.UTF8.GetBytes(expected), C1StringEncoder.Encode(input));
        for (var c = 0; c < 32; c++) Assert.Equal(Encoding.ASCII.GetBytes("\"\\u00" + c.ToString("x2") + "\""), C1StringEncoder.Encode(((char)c).ToString()));
        Assert.Throws<ArgumentException>(() => C1StringEncoder.Encode(((char)0xd800).ToString())); Assert.Throws<ArgumentException>(() => C1StringEncoder.Encode(((char)0xdc00).ToString()));
    }
    [Fact]
    public void RegistrySeparatesActionsRolesAndLookupGroupsExactly()
    {
        Assert.Equal(55, CanonicalActionRegistry.All.Count); Assert.Equal(55, CanonicalActionRegistry.All.Select(a => a.Code).Distinct().Count()); Assert.Equal(6, CanonicalActionRegistry.All.Select(a => a.LookupApiId).Distinct().Count()); Assert.Equal(2, CanonicalActionRegistry.All.Count(a => a.ResultMode == "SECRET_RESULT_UNAVAILABLE"));
        Assert.Throws<ArgumentException>(() => CanonicalActionRegistry.ForCode("admin.account.lock")); Assert.Throws<ArgumentException>(() => CanonicalActionRegistry.ForCode("API-010"));
        Assert.True(CanonicalActionRegistry.AllowsLookup(14, "Admin", "ADMIN.ACCOUNT.LOCK")); Assert.False(CanonicalActionRegistry.AllowsLookup(14, "Teacher", "ADMIN.ACCOUNT.LOCK")); Assert.False(CanonicalActionRegistry.AllowsLookup(90, "AcademicAffairs", "TEACHER.GRADE_DRAFT.SAVE_DELTA"));
    }
    [Fact]
    public void InternalExpectedAbsenceAndBeforeSaveBindingsAreNotDropped()
    {
        var body = JsonNode.Parse(CanonicalFixtures.For(28))!; body["expectedVersions"]!["invoice"] = new JsonObject { ["id"] = 9, ["version"] = "AAECAwQFBgc=" }; body["expectedSet"]!["invoiceId"] = 9;
        Assert.NotEqual(Bind(28).CopyDigest(), Bind(28, body.ToJsonString()).CopyDigest()); body["expectedSet"]!["invoiceId"] = null; Assert.Throws<ApiErrorException>(() => Bind(28, body.ToJsonString()));
        var program = JsonNode.Parse(CanonicalFixtures.For(84))!; program["expectedVersions"]!["existingComponents"]!.AsArray().Add(new JsonObject { ["id"] = 1, ["version"] = "AAECAwQFBgc=" }); program["expectedSet"]!["existingComponentIds"]!.AsArray().Add(1);
        // Removed desired component remains in the full source guard and changes the digest.
        Assert.NotEqual(Bind(84).CopyDigest(), Bind(84, program.ToJsonString()).CopyDigest()); program["expectedSet"]!["existingComponentIds"]!.AsArray().Clear(); Assert.Throws<ApiErrorException>(() => Bind(84, program.ToJsonString()));
    }
    [Fact]
    public void ComponentsUseExistingIdsThenOrdinalNormalizedNewNamesAndAccentSensitiveNames()
    {
        var body = JsonNode.Parse(CanonicalFixtures.For(84))!;
        body["expectedVersions"]!["existingComponents"]!.AsArray().Add(new JsonObject { ["id"] = 4, ["version"] = "AAECAwQFBgc=" }); body["expectedSet"]!["existingComponentIds"]!.AsArray().Add(4);
        body["components"] = new JsonArray(new JsonObject { ["id"] = null, ["name"] = "z", ["kind"] = "GENERAL", ["weight"] = "40.5" }, new JsonObject { ["id"] = 4, ["name"] = "Existing", ["kind"] = "LESSON", ["weight"] = "10" }, new JsonObject { ["id"] = null, ["name"] = "a", ["kind"] = "GENERAL", ["weight"] = "20.00" });
        var intent = Bind(84, body.ToJsonString()); var normalized = JsonNode.Parse(intent.CopyBytes())!["body"]!["components"]!.AsArray(); Assert.Equal(4, normalized[0]!["id"]!.GetValue<int>()); Assert.Equal("a", normalized[1]!["name"]!.GetValue<string>()); Assert.Equal("z", normalized[2]!["name"]!.GetValue<string>());
        body["components"]![2]!["name"] = "Z"; Assert.Throws<ApiErrorException>(() => Bind(84, body.ToJsonString())); body["components"]![2]!["name"] = "ž"; Assert.NotNull(Bind(84, body.ToJsonString()));
    }
    [Fact]
    public void DuplicateNestedTimetableIdentityAndFalseFlagsCannotBeSmuggled()
    {
        var body = JsonNode.Parse(CanonicalFixtures.For(108))!; var timetable = body["expectedSet"]!["studentTimetable"]!;
        timetable["bookings"] = new JsonArray(new JsonObject { ["bookingId"] = 1, ["attemptId"] = 1, ["sessionId"] = 1, ["bookingVersion"] = "AAECAwQFBgc=", ["sessionVersion"] = "AAECAwQFBgc=" }, new JsonObject { ["bookingId"] = 1, ["attemptId"] = 2, ["sessionId"] = 2, ["bookingVersion"] = "AAECAwQFBgc=", ["sessionVersion"] = "AAECAwQFBgc=" }); Assert.Throws<ApiErrorException>(() => Bind(108, body.ToJsonString()));
        body = JsonNode.Parse(CanonicalFixtures.For(102))!; body["expectedSet"]!["hasScores"] = true; Assert.Throws<ApiErrorException>(() => Bind(102, body.ToJsonString()));
        var attendance = CanonicalFixtures.For(133).Replace("\"present\":0", "\"present\":-0"); Assert.Throws<ApiErrorException>(() => Bind(133, attendance));
    }

    [Fact]
    public void CompositeBindingsSortPrimaryAssignmentThenRemainingIdNamesInAsciiOrder()
    {
        var body=JsonNode.Parse(CanonicalFixtures.For(101))!;
        body["expectedSet"]!["assignmentIds"]=new JsonArray(1);
        body["expectedVersions"]!["attendances"]=new JsonArray(new JsonObject { ["id"]=20,["version"]="AAECAwQFBgc=" },new JsonObject { ["id"]=10,["version"]="AAECAwQFBgc=" });
        body["expectedVersions"]!["schedules"]=new JsonArray(new JsonObject { ["id"]=2,["version"]="AAECAwQFBgc=" },new JsonObject { ["id"]=1,["version"]="AAECAwQFBgc=" });
        body["expectedSet"]!["scheduleBindings"]=new JsonArray(new JsonObject { ["id"]=2,["classId"]=1,["obligationKey"]="11111111-1111-1111-1111-111111111112",["sessionType"]="LESSON",["isCurrent"]=true,["replacesScheduleId"]=null },new JsonObject { ["id"]=1,["classId"]=1,["obligationKey"]="11111111-1111-1111-1111-111111111111",["sessionType"]="LESSON",["isCurrent"]=true,["replacesScheduleId"]=null });
        body["expectedSet"]!["attendanceBindings"]=new JsonArray(new JsonObject { ["classAssignmentId"]=1,["attendanceId"]=20,["scheduleId"]=1 },new JsonObject { ["classAssignmentId"]=1,["attendanceId"]=10,["scheduleId"]=2 });
        var result=JsonNode.Parse(Bind(101,body.ToJsonString()).CopyBytes())!;
        Assert.Equal(10,result["body"]!["expectedSet"]!["attendanceBindings"]![0]!["attendanceId"]!.GetValue<int>());
    }

}

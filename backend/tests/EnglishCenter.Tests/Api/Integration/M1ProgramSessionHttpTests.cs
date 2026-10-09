namespace EnglishCenter.Tests.Api.Integration;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Xunit;
[Collection("V01 HTTP SQL")]
public sealed class M1ProgramSessionHttpTests
{
    private const string OptIn="M1 HTTP owned scratch SQL, verified JWT and framework CSRF.";
    private static CancellationToken Ct=>OwnedSqlFixture.Ct;
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task SessionHttp_ExactWireBodyCsrfStatusReplayLookupRosterRoleAndRestricted()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();await using var host=await M1HttpSqlTests.Host.Create(f);await host.Login("AcademicAffairs");var body=new{date=f.Clock.GetUtcNow().AddHours(7).AddDays(1).ToString("yyyy-MM-dd"),startTime="09:00",endTime="10:00",room="HTTP",graderId=f.Accounts["Teacher"],capacity=20};var key=Guid.NewGuid();const string root="/api/v1/admissions/entry-test-sessions";
        using var denied=await host.Send(HttpMethod.Post,root,body,key,csrf:false);Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);
        using var created=await host.Send(HttpMethod.Post,root,body,key);Assert.Equal(HttpStatusCode.Created,created.StatusCode);var first=await created.Content.ReadFromJsonAsync<JsonElement>(Ct);Assert.Equal(2,first.EnumerateObject().Count());var session=first.GetProperty("session");Assert.Equal(21,session.EnumerateObject().Count());var id=session.GetProperty("id").GetInt32();
        using var replay=await host.Send(HttpMethod.Post,root,body,key);Assert.Equal(HttpStatusCode.OK,replay.StatusCode);Assert.Equal(first.GetProperty("operationId").GetString(),(await replay.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("operationId").GetString());
        using var lookup=await host.Send(HttpMethod.Get,$"/api/v1/admissions/command-results/{key:D}?action=ACADEMIC.ENTRY_SESSION.CREATE");Assert.Equal(HttpStatusCode.OK,lookup.StatusCode);
        using var preview=await host.Send(HttpMethod.Post,root+$"/{id}/cancellation-preview");Assert.Equal(HttpStatusCode.OK,preview.StatusCode);var p=await preview.Content.ReadFromJsonAsync<JsonElement>(Ct);Assert.Empty(p.GetProperty("effects").EnumerateArray());
        using var roster=await host.Send(HttpMethod.Post,root+$"/{id}/roster/search",new{name="%"});Assert.Equal(HttpStatusCode.OK,roster.StatusCode);Assert.DoesNotContain("%",root+$"/{id}/roster/search");
        using var list=await host.Send(HttpMethod.Get,root);Assert.Equal(HttpStatusCode.OK,list.StatusCode);var page=await list.Content.ReadFromJsonAsync<JsonElement>(Ct);Assert.Equal(17,page.GetProperty("items")[0].EnumerateObject().Count());
        await host.Login("Admissions");using var role=await host.Send(HttpMethod.Get,root);Assert.Equal(HttpStatusCode.Forbidden,role.StatusCode);await f.Restricted("AcademicAffairs");await host.Login("AcademicAffairs");using var restricted=await host.Send(HttpMethod.Get,root);Assert.Equal(HttpStatusCode.Forbidden,restricted.StatusCode);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ProgramHttp_DraftExactProjectionConfigurationAndActivationPreview()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();await using var seed=f.Db();await EnglishCenter.Api.Infrastructure.Operator.LocalBandReferenceSeed.SeedAsync(seed,Ct);var course=new EnglishCenter.Api.Domain.Academics.Course{CourseCode="HTTP",CourseName="Synthetic",BasePrice=1000,Status="ACTIVE"};seed.Courses.Add(course);await seed.SaveChangesAsync(Ct);var band=await seed.Bands.OrderBy(x=>x.MinScore).FirstAsync(Ct);await using var host=await M1HttpSqlTests.Host.Create(f);await host.Login("AcademicAffairs");const string root="/api/v1/academics/training-programs";
        var body=new{courseId=course.CourseId,bandId=band.BandId,name="HTTP Draft",expectedLessonCount=1,components=Array.Empty<object>(),expectedVersions=new{course=EnglishCenter.Api.Common.Serialization.RowVersionCodec.Encode(course.RowVersion),band=EnglishCenter.Api.Common.Serialization.RowVersionCodec.Encode(band.RowVersion)}};
        var key=Guid.NewGuid();using var created=await host.Send(HttpMethod.Post,root,body,key);Assert.Equal(HttpStatusCode.Created,created.StatusCode);var r=await created.Content.ReadFromJsonAsync<JsonElement>(Ct);var p=r.GetProperty("program");Assert.Equal(15,p.EnumerateObject().Count());Assert.Empty(p.GetProperty("components").EnumerateArray());var id=p.GetProperty("id").GetInt32();using var preview=await host.Send(HttpMethod.Post,root+$"/{id}/activation-preview");Assert.Equal(HttpStatusCode.OK,preview.StatusCode);Assert.False((await preview.Content.ReadFromJsonAsync<JsonElement>(Ct)).GetProperty("eligible").GetBoolean());
        using var bad=await host.Send(HttpMethod.Get,root+"?status=LOCKED");Assert.Equal(HttpStatusCode.BadRequest,bad.StatusCode);using var lookup=await host.Send(HttpMethod.Get,$"/api/v1/academics/command-results/{key:D}?action=ACADEMIC.PROGRAM.CREATE");Assert.Equal(HttpStatusCode.OK,lookup.StatusCode);
        await host.Login("Teacher");using var wrong=await host.Send(HttpMethod.Get,root);Assert.Equal(HttpStatusCode.Forbidden,wrong.StatusCode);
    }
}

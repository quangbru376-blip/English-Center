namespace EnglishCenter.Tests.Api.Integration;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Infrastructure.Administration;
using EnglishCenter.Api.Infrastructure.Catalogue;
using EnglishCenter.Api.Services.Administration;
using EnglishCenter.Api.Services.Catalogue;
using Microsoft.EntityFrameworkCore;
using Xunit;

[Collection("V01 HTTP SQL")]
public sealed class M1StaffCourseSqlTests
{
    private const string OptIn="M1 isolated owned-COMMIT SQL uses existing identity-cleaned scratch fixture.";
    private static CancellationToken Ct=>OwnedSqlFixture.Ct;
    private static SqlStaffAccountExecutor Staff(OwnedSqlFixture f)=>new(()=>f.Db(),f.Clock,f.Hasher,
        new TemporaryPasswordGenerator(new PasswordPolicy(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory,"Resources","PasswordBlocklist")))),10,1000);
    private static SqlCourseExecutor Courses(OwnedSqlFixture f)=>new(()=>f.Db(),f.Clock,10,1000);
    private static StaffAccountInput StaffInput(int api,int id,string version,Guid? key=null)=>StaffAccountInput.Parse(api,id,key??Guid.NewGuid(),JsonSerializer.Serialize(new{expectedVersions=new{account=version}}),4096);
    private static CourseInput CourseInput(int api,int id,object body,Guid? key=null)=>ServicesInput(api,id,JsonSerializer.Serialize(body),key);
    private static CourseInput ServicesInput(int api,int id,string body,Guid? key=null)=>EnglishCenter.Api.Services.Catalogue.CourseInput.Parse(api,id,key??Guid.NewGuid(),body,4096);
    private static async Task<CourseResult> Create(OwnedSqlFixture f,EnglishCenter.Api.Common.Security.VerifiedSessionToken token,Guid? key=null)=>
        await Courses(f).CommandAsync(token,CourseInput(75,0,new{code="IELTS",name="IELTS Foundation",basePrice="999999999999999999",status="ACTIVE"},key),Ct);

    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task StaffCreate_SecretFirstOnly_DuplicateContactsAndSafeReplay()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("Admin"));var executor=Staff(f);var key=Guid.NewGuid();
        var input=StaffAccountInput.Parse(8,0,key,"{\"fullName\":\" New Teacher \",\"email\":\"NEW@example.invalid\",\"role\":\"Teacher\",\"phone\":\"+84912345678\"}",4096);
        var first=Assert.IsType<CredentialIssueResult>(await executor.ExecuteAsync(token,input,Ct));Assert.Equal(32,first.TemporaryPassword.Length);
        Assert.DoesNotContain(first.TemporaryPassword,first.ToString());
        Assert.IsType<EnglishCenter.Api.Common.Replay.SafeCommandCompletion>(await executor.ExecuteAsync(token,input,Ct));
        await using var db=f.Db();var row=await db.Accounts.SingleAsync(a=>a.AccountId==first.AccountId,Ct);
        Assert.Equal("new@example.invalid",row.Email);Assert.Equal("0912345678",row.Phone);Assert.True(row.MustChangePassword);Assert.Equal(row.TempIssuedAt!.Value.AddHours(24),row.TempExpiresAt);
        Assert.NotEqual(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed,f.Hasher.Verify(row.PasswordHash,first.TemporaryPassword));
        Assert.False(await db.StudentProfiles.AnyAsync(p=>p.AccountId==row.AccountId,Ct));Assert.False(await db.AuthSessions.AnyAsync(p=>p.AccountId==row.AccountId,Ct));
        Assert.Equal(1,await db.AuditLogs.CountAsync(a=>a.Action=="ADMIN.STAFF.CREATE",Ct));Assert.Equal(1,await db.ReplayOperations.CountAsync(Ct));
        var restricted=await f.Login.ExecuteAsync(EnglishCenter.Api.Services.Auth.LoginInput.Parse(JsonSerializer.Serialize(new{email=row.Email,password=first.TemporaryPassword})),Ct);
        Assert.True(restricted.Accepted);Assert.Equal("PASSWORD_CHANGE",restricted.Grant!.Context.Scope);
        var duplicate=StaffAccountInput.Parse(8,0,Guid.NewGuid(),"{\"fullName\":\"Other\",\"email\":\"new@EXAMPLE.invalid\",\"role\":\"Teacher\"}",4096);
        Assert.Equal(ApiErrorCode.CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.ExecuteAsync(token,duplicate,Ct))).Code);
        Assert.DoesNotContain(first.TemporaryPassword,string.Join("",await db.AuditLogs.Select(a=>a.Details).ToListAsync(Ct)));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData(9,"ACTIVE","ACTIVE")][InlineData(9,"LOCKED","LOCKED")][InlineData(9,"DISABLED","DISABLED")]
    [InlineData(11,"LOCKED","ACTIVE")][InlineData(12,"ACTIVE","DISABLED")][InlineData(12,"LOCKED","DISABLED")][InlineData(13,"DISABLED","ACTIVE")]
    public async Task AccountCommands_VersionsRevocationStatusAuditReplay(int api,string source,string destination)
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("Admin"));await f.Grant("Teacher");var id=f.Accounts["Teacher"];
        string version;await using(var db=f.Db()){var row=await db.Accounts.SingleAsync(a=>a.AccountId==id,Ct);row.Status=source;await db.SaveChangesAsync(Ct);version=RowVersionCodec.Encode(row.RowVersion);}
        var input=StaffInput(api,id,version);var executor=Staff(f);var result=await executor.ExecuteAsync(token,input,Ct);
        if(api==9)Assert.IsType<CredentialIssueResult>(result);else Assert.IsType<AdminAccountCommandResult>(result);
        await executor.ExecuteAsync(token,input,Ct);
        await using var check=f.Db();var account=await check.Accounts.SingleAsync(a=>a.AccountId==id,Ct);Assert.Equal(destination,account.Status);
        Assert.Equal(api is 9 or 12?2:1,account.SecurityVersion);Assert.Equal(api is 9 or 12?0:1,await check.AuthSessions.CountAsync(s=>s.AccountId==id&&s.RevokedAt==null,Ct));
        Assert.Equal(1,await check.ReplayOperations.CountAsync(Ct));Assert.Equal(1,await check.AuditLogs.CountAsync(a=>a.AuditCategory=="MANDATORY",Ct));
        if(api==9)Assert.True(account.MustChangePassword);
        var stale=StaffInput(api,id,version);Assert.Equal(ApiErrorCode.CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.ExecuteAsync(token,stale,Ct))).Code);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task SelfReset_CommitsAndRevokesCallerWithoutRestoringSessions()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var grant=await f.Grant("Admin");var token=f.Token(grant);
        var result=Assert.IsType<CredentialIssueResult>(await Staff(f).ExecuteAsync(token,StaffInput(9,token.AccountId,grant.Context.AccountVersion),Ct));
        Assert.Equal(token.AccountId,result.AccountId);
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,(await Assert.ThrowsAsync<ApiErrorException>(()=>f.Current.ReadAsync(token,true,Ct))).Code);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task Disable_SelfAndLockedCallerRefused_NoEffects()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var grant=await f.Grant("Admin");var token=f.Token(grant);
        var executor=Staff(f);Assert.Equal(ApiErrorCode.STATE_CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.ExecuteAsync(token,StaffInput(12,token.AccountId,grant.Context.AccountVersion),Ct))).Code);
        string version;var target=f.Accounts["Admin2"];
        await using(var db=f.Db()){var actor=await db.Accounts.SingleAsync(a=>a.AccountId==token.AccountId,Ct);actor.Status="LOCKED";await db.SaveChangesAsync(Ct);var a=await db.Accounts.SingleAsync(a=>a.AccountId==target,Ct);version=RowVersionCodec.Encode(a.RowVersion);}
        // A locked caller cannot exercise even otherwise valid target authority.
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.ExecuteAsync(token,StaffInput(12,target,version),Ct))).Code);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task CourseLifecycle_ExactMoneyNoopReplayAndReadBandIntegrity()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));var executor=Courses(f);var key=Guid.NewGuid();var first=await Create(f,token,key);
        Assert.Equal("999999999999999999",first.Course.BasePrice);Assert.False(first.Replayed);var repeated=await Create(f,token,key);Assert.True(repeated.Replayed);Assert.Equal(first.OperationId,repeated.OperationId);
        var same=CourseInput(77,first.Course.Id,new{name=first.Course.Name,basePrice=first.Course.BasePrice,expectedVersions=new{course=first.Course.Version}});
        Assert.Equal(ApiErrorCode.VALIDATION_ERROR,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,same,Ct))).Code);
        var edited=await executor.CommandAsync(token,CourseInput(77,first.Course.Id,new{name="Updated",basePrice="123456789012345678",expectedVersions=new{course=first.Course.Version}}),Ct);
        Assert.Equal("IELTS",edited.Course.Code);var inactive=await executor.CommandAsync(token,CourseInput(78,first.Course.Id,new{expectedVersions=new{course=edited.Course.Version}}),Ct);Assert.Equal("INACTIVE",inactive.Course.Status);
        var active=await executor.CommandAsync(token,CourseInput(79,first.Course.Id,new{expectedVersions=new{course=inactive.Course.Version}}),Ct);Assert.Equal("ACTIVE",active.Course.Status);
        Assert.Equal(ApiErrorCode.CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.CommandAsync(token,CourseInput(78,first.Course.Id,new{expectedVersions=new{course=first.Course.Version}}),Ct))).Code);
        Assert.Equal(ApiErrorCode.SERVICE_UNAVAILABLE,(await Assert.ThrowsAsync<ApiErrorException>(()=>executor.BandsAsync(token,Ct))).Code);
        await using var db=f.Db();await using var tx=await db.Database.BeginTransactionAsync(Ct);
        await db.Database.ExecuteSqlRawAsync("EXEC sys.sp_getapplock @Resource=N'EC:BANDSET',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@DbPrincipal=N'public';",Ct);
        db.Bands.AddRange(new[]{0m,25m,45m,65m,85m}.Select((score,i)=>new Band{BandCode="Band"+(i+1),MinScore=score}));await db.SaveChangesAsync(Ct);await tx.CommitAsync(Ct);
        var bands=await executor.BandsAsync(token,Ct);Assert.Equal(5,bands.Items.Count);Assert.False(bands.IsThresholdSetFrozen);
        var page=await executor.SearchAsync(token,new(null,"%",null,"CODE_ASC",1,20),Ct);Assert.Empty(page.Items);
        Assert.Equal(4,await db.ReplayOperations.CountAsync(Ct));Assert.Equal(4,await db.AuditLogs.CountAsync(a=>a.AuditCategory=="MANDATORY",Ct));
    }
    [Theory(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    [InlineData("Student")][InlineData("Teacher")][InlineData("Admissions")][InlineData("Accountant")][InlineData("Director")]
    public async Task WrongRolesCannotUseEitherModule(string role)
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant(role));
        Assert.Equal(ApiErrorCode.FORBIDDEN,(await Assert.ThrowsAsync<ApiErrorException>(()=>Staff(f).SearchAsync(token,AccountSearch.Parse("{}"),Ct))).Code);
        Assert.Equal(ApiErrorCode.FORBIDDEN,(await Assert.ThrowsAsync<ApiErrorException>(()=>Courses(f).SearchAsync(token,new(null,null,null,"CODE_ASC",1,20),Ct))).Code);
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task MandatoryAuditFailure_RollsBackCourseAndStaffAndMarkers()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var admin=f.Token(await f.Grant("Admin"));var academic=f.Token(await f.Grant("AcademicAffairs"));
        await using(var gate=await f.HoldAudit())
        {
            await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>Create(f,academic));
            var input=StaffAccountInput.Parse(8,0,Guid.NewGuid(),"{\"fullName\":\"Fail\",\"email\":\"fail@example.invalid\",\"role\":\"Teacher\"}",4096);
            await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>Staff(f).ExecuteAsync(admin,input,Ct));
            await using var reference=f.Db();
            await Assert.ThrowsAsync<EnglishCenter.Api.Infrastructure.Auditing.MandatoryAuditException>(()=>EnglishCenter.Api.Infrastructure.Operator.LocalBandReferenceSeed.SeedAsync(reference,Ct));
        }
        await using var db=f.Db();Assert.Empty(await db.Courses.ToListAsync(Ct));Assert.Empty(await db.Bands.ToListAsync(Ct));Assert.False(await db.Accounts.AnyAsync(a=>a.Email=="fail@example.invalid",Ct));Assert.Empty(await db.ReplayOperations.ToListAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ConcurrentAdmins_DisablingEachOtherKeepOneActive()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var first=await f.Grant("Admin");var second=await f.Grant("Admin2");
        async Task<bool> Attempt(EnglishCenter.Api.Services.Auth.LoginGrant actor,int target,string version)
        {try{await Staff(f).ExecuteAsync(f.Token(actor),StaffInput(12,target,version),Ct);return true;}catch(ApiErrorException e){Assert.Contains(e.Code,new[]{ApiErrorCode.AUTHENTICATION_REQUIRED,ApiErrorCode.STATE_CONFLICT,ApiErrorCode.SERVICE_UNAVAILABLE});return false;}}
        var results=await Task.WhenAll(Attempt(first,f.Accounts["Admin2"],second.Context.AccountVersion),Attempt(second,f.Accounts["Admin"],first.Context.AccountVersion));
        Assert.Single(results,x=>x);await using var db=f.Db();Assert.Equal(1,await db.Accounts.CountAsync(a=>a.Status=="ACTIVE"&&db.Roles.Any(r=>r.RoleId==a.RoleId&&r.RoleCode=="Admin"),Ct));Assert.Equal(1,await db.ReplayOperations.CountAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ConcurrentCourseCodeAndReplayDoNotDuplicateRows()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();var token=f.Token(await f.Grant("AcademicAffairs"));
        async Task<bool> Attempt(Guid key){try{await Create(f,token,key);return true;}catch(ApiErrorException e){Assert.Contains(e.Code,new[]{ApiErrorCode.CONFLICT,ApiErrorCode.SERVICE_UNAVAILABLE});return false;}}
        var results=await Task.WhenAll(Attempt(Guid.NewGuid()),Attempt(Guid.NewGuid()));Assert.Single(results,x=>x);
        await using var db=f.Db();Assert.Equal(1,await db.Courses.CountAsync(Ct));Assert.Equal(1,await db.ReplayOperations.CountAsync(Ct));
    }
    [Fact(Skip=OptIn,SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
    public async Task ExplicitReferenceSeed_IsAtomicIdempotentAndNeverRepairsPartialSet()
    {
        await using var f=await OwnedSqlFixture.CreateAsync();
        await using(var db=f.Db()){Assert.True(await EnglishCenter.Api.Infrastructure.Operator.LocalBandReferenceSeed.SeedAsync(db,Ct));Assert.True(await EnglishCenter.Api.Infrastructure.Operator.LocalBandReferenceSeed.SeedAsync(db,Ct));Assert.Equal(5,await db.Bands.CountAsync(Ct));Assert.Equal(5,await db.AuditLogs.CountAsync(a=>a.Action=="SYSTEM.BAND.SEED",Ct));}
        await using var partial=f.Db();await using(var tx=await partial.Database.BeginTransactionAsync(Ct))
        {await partial.Database.ExecuteSqlRawAsync("EXEC sys.sp_getapplock @Resource=N'EC:BANDSET',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@DbPrincipal=N'public';",Ct);var last=await partial.Bands.OrderByDescending(b=>b.MinScore).FirstAsync(Ct);partial.Remove(last);await partial.SaveChangesAsync(Ct);await tx.CommitAsync(Ct);}
        Assert.False(await EnglishCenter.Api.Infrastructure.Operator.LocalBandReferenceSeed.SeedAsync(partial,Ct));Assert.Equal(4,await partial.Bands.CountAsync(Ct));
    }
}

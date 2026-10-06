namespace EnglishCenter.SqlTests;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class StudentSignupTests
{
    [Fact]
    public async Task CreatesOnlyStudentAccountProfileMandatoryAuditWithRawCredentialAndNoSessionReplay()
    {
        await using var f=await SqlFixture.CreateAsync();await AuthTestSupport.StudentRole(f);
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        var raw="  "+AuthTestSupport.Password()+"  ";var input=AuthTestSupport.Signup(raw," SIGNUP+tag@EXAMPLE.INVALID ");
        var result=await new StudentSignupService(new SqlStudentSignupRepository(scope),AuthTestSupport.Hasher(),new BusinessClock(new AuthTestSupport.Clock(new(SqlFixture.Instant)))).RegisterAsync(input,TestContext.Current.CancellationToken);
        Assert.Equal("CREATED",result.Outcome);var account=await f.Context.Accounts.AsNoTracking().SingleAsync(a=>a.Email==input.Email,TestContext.Current.CancellationToken);
        Assert.Equal("0912345678",account.Phone);Assert.Equal("ACTIVE",account.Status);Assert.False(account.MustChangePassword);Assert.Equal(1,account.SecurityVersion);
        Assert.True(AuthTestSupport.Hasher().Verify(account.PasswordHash,raw)==PasswordVerificationResult.Success);Assert.True(AuthTestSupport.Hasher().Verify(account.PasswordHash,raw.Trim())==PasswordVerificationResult.Failed);
        var profile=await f.Context.StudentProfiles.AsNoTracking().SingleAsync(p=>p.AccountId==account.AccountId,TestContext.Current.CancellationToken);Assert.Equal(new DateOnly(2000,1,1),profile.DateOfBirth);Assert.Null(profile.Address);
        var audit=await f.Context.AuditLogs.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);Assert.Equal("STUDENT.ACCOUNT.CREATE",audit.Action);Assert.Null(audit.ActorAccountId);Assert.Null(audit.SecurityExpiresAt);Assert.DoesNotContain("email",audit.Details);Assert.DoesNotContain("password",audit.Details);
        Assert.Equal(0,await f.Context.AuthSessions.CountAsync(TestContext.Current.CancellationToken));Assert.Equal(0,await f.Context.ReplayOperations.CountAsync(TestContext.Current.CancellationToken));await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Theory]
    [InlineData("email")] [InlineData("phone")]
    public async Task NormalizedUniquenessAppliesAcrossRolesWithoutOwnerDisclosure(string field)
    {
        await using var f=await SqlFixture.CreateAsync();await AuthTestSupport.StudentRole(f);f.Teacher.Phone="0912345678";await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        var input=AuthTestSupport.Signup(AuthTestSupport.Password(),field=="email"?" FIXTURE@EXAMPLE.INVALID ":"signup@example.invalid",field=="phone"?"+84 912 345 678":"0987654321");
        var error=await Assert.ThrowsAsync<ApiErrorException>(()=>new StudentSignupService(new SqlStudentSignupRepository(scope),AuthTestSupport.Hasher(),new BusinessClock(new AuthTestSupport.Clock(new(SqlFixture.Instant)))).RegisterAsync(input,TestContext.Current.CancellationToken));
        Assert.Equal(ApiErrorCode.CONFLICT,error.Code);Assert.Equal(field,error.FieldPath);Assert.Equal(1,await f.Context.Accounts.CountAsync(TestContext.Current.CancellationToken));Assert.Equal(0,await f.Context.AuditLogs.CountAsync(TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task MissingRoleFutureBirthAndBadPasswordNeverAutoSeedOrWrite()
    {
        await using var f=await SqlFixture.CreateAsync();await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        var service=new StudentSignupService(new SqlStudentSignupRepository(scope),AuthTestSupport.Hasher(),new BusinessClock(new AuthTestSupport.Clock(new(SqlFixture.Instant))));
        var missing=await Assert.ThrowsAsync<ApiErrorException>(()=>service.RegisterAsync(AuthTestSupport.Signup(AuthTestSupport.Password()),TestContext.Current.CancellationToken));Assert.Equal(ApiErrorCode.SERVICE_UNAVAILABLE,missing.Code);
        var future=await Assert.ThrowsAsync<ApiErrorException>(()=>service.RegisterAsync(AuthTestSupport.Signup(AuthTestSupport.Password(),birth:"2099-01-01"),TestContext.Current.CancellationToken));Assert.Equal("dateOfBirth",future.FieldPath);
        var bad=await Assert.ThrowsAsync<ApiErrorException>(()=>service.RegisterAsync(AuthTestSupport.Signup(AuthTestSupport.Password()[..5]),TestContext.Current.CancellationToken));Assert.Equal("password",bad.FieldPath);
        Assert.Equal(1,await f.Context.Accounts.CountAsync(TestContext.Current.CancellationToken));Assert.Equal(1,await f.Context.Roles.CountAsync(TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task ActualDeniedMandatoryAuditRollsBackAccountAndProfileTogether()
    {
        await using var f=await SqlFixture.CreateAsync();await AuthTestSupport.StudentRole(f);await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        await SqlRows.ReadAsync(f,"IF DATABASE_PRINCIPAL_ID('D12_fixture_user_0') IS NOT NULL THROW 51912,'Fixture exists',1; CREATE USER D12_fixture_user_0 WITHOUT LOGIN; ALTER ROLE ec_app_runtime ADD MEMBER D12_fixture_user_0; DENY INSERT ON dbo.AuditLog TO ec_app_runtime; EXECUTE AS USER='D12_fixture_user_0';");
        try { await Assert.ThrowsAsync<MandatoryAuditException>(()=>new StudentSignupService(new SqlStudentSignupRepository(scope),AuthTestSupport.Hasher(),new BusinessClock(new AuthTestSupport.Clock(new(SqlFixture.Instant)))).RegisterAsync(AuthTestSupport.Signup(AuthTestSupport.Password()),TestContext.Current.CancellationToken)); }
        finally { await SqlRows.ReadAsync(f,"REVERT;"); }
        Assert.Equal(2,await f.Context.Accounts.CountAsync(TestContext.Current.CancellationToken));Assert.Equal(2,await f.Context.StudentProfiles.CountAsync(TestContext.Current.CancellationToken));await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task FailureOrCancellationAfterAllThreeWritesStillRequiresCompleteRollback(bool cancel)
    {
        await using var f=await SqlFixture.CreateAsync();await AuthTestSupport.StudentRole(f);await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        var service=new StudentSignupService(new FailAfterWrite(new SqlStudentSignupRepository(scope),cancel),AuthTestSupport.Hasher(),new BusinessClock(new AuthTestSupport.Clock(new(SqlFixture.Instant))));
        if(cancel)await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>service.RegisterAsync(AuthTestSupport.Signup(AuthTestSupport.Password()),TestContext.Current.CancellationToken));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>service.RegisterAsync(AuthTestSupport.Signup(AuthTestSupport.Password()),TestContext.Current.CancellationToken));
        Assert.Equal(1,await f.Context.AuditLogs.CountAsync(TestContext.Current.CancellationToken));await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Fact]
    public async Task CompetingConnectionCannotReadOrReserveHeldNormalizedEmailRange()
    {
        await using var f=await SqlFixture.CreateAsync();await AuthTestSupport.StudentRole(f);await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        await new StudentSignupService(new SqlStudentSignupRepository(scope),AuthTestSupport.Hasher(),new BusinessClock(new AuthTestSupport.Clock(new(SqlFixture.Instant)))).RegisterAsync(AuthTestSupport.Signup(AuthTestSupport.Password()),TestContext.Current.CancellationToken);
        await using var other=new EnglishCenterDbContext(new DbContextOptionsBuilder<EnglishCenterDbContext>().UseSqlServer(f.Context.Database.GetConnectionString(),sql=>sql.UseCompatibilityLevel(160)).Options);
        await other.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);await using var tx=await other.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);await using var otherScope=await SqlAuthDeadlineScope.EnterAsync(other,10,100,TestContext.Current.CancellationToken);
        var error=await Assert.ThrowsAsync<SqlException>(()=>new SqlStudentSignupRepository(otherScope).EmailExistsAsync("SIGNUP+TAG@EXAMPLE.INVALID",TestContext.Current.CancellationToken));Assert.Equal(1222,error.Number);
    }

    [Theory]
    [InlineData("email")] [InlineData("phone")]
    public async Task ActualUniqueIndexBackstopMapsOnlyKnownContactCollision(string field)
    {
        await using var f=await SqlFixture.CreateAsync();await AuthTestSupport.StudentRole(f);f.Teacher.Phone="0912345678";await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        var roleId=await new SqlStudentSignupRepository(scope).StudentRoleAsync(TestContext.Current.CancellationToken);
        var account=new EnglishCenter.Api.Domain.Identity.Account { RoleId=roleId,Email=field=="email"?"FIXTURE@EXAMPLE.INVALID":"signup@example.invalid",Phone=field=="phone"?"0912345678":"0987654321",FullName="Synthetic",PasswordHash=AuthTestSupport.Hasher().Hash(AuthTestSupport.Password()),Status="ACTIVE",SecurityVersion=1,CreatedAt=SqlFixture.Instant };
        var profile=new EnglishCenter.Api.Domain.Identity.StudentProfile { Account=account,DateOfBirth=new DateOnly(2000,1,1) };
        var error=await Assert.ThrowsAsync<EnglishCenter.Api.Common.Errors.ApiErrorException>(()=>new SqlStudentSignupRepository(scope).InsertWithAuditAsync(account,profile,new(SqlFixture.Instant),TestContext.Current.CancellationToken));Assert.Equal(ApiErrorCode.CONFLICT,error.Code);Assert.Equal(field,error.FieldPath);
        Assert.Equal(0,await f.Context.AuditLogs.CountAsync(TestContext.Current.CancellationToken));
    }
    private sealed class FailAfterWrite(IStudentSignupRepository inner,bool cancel) : IStudentSignupRepository
    {
        public Task<int> StudentRoleAsync(CancellationToken ct)=>inner.StudentRoleAsync(ct);
        public Task<bool> EmailExistsAsync(string email,CancellationToken ct)=>inner.EmailExistsAsync(email,ct);
        public Task<bool> PhoneExistsAsync(string phone,CancellationToken ct)=>inner.PhoneExistsAsync(phone,ct);
        public async Task InsertWithAuditAsync(EnglishCenter.Api.Domain.Identity.Account a,EnglishCenter.Api.Domain.Identity.StudentProfile p,DateTimeOffset at,CancellationToken ct)
        { await inner.InsertWithAuditAsync(a,p,at,ct);if(cancel)throw new OperationCanceledException();throw new InvalidOperationException("Synthetic post-write failure."); }
    }
}

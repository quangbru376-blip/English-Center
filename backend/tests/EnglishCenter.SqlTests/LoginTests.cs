namespace EnglishCenter.SqlTests;

using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Infrastructure;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Infrastructure.Auth;
using EnglishCenter.Api.Services.Auth;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class LoginTests
{
    private static LoginService Service(SqlAuthDeadlineScope scope,AuthTestSupport.Clock clock,IJwtSessionIssuer? signer=null)=>new(new SqlLoginRepository(scope),AuthTestSupport.Hasher(),new BusinessClock(clock),signer??AuthTestSupport.Signer(),AuthTestSupport.Hasher().Hash(AuthTestSupport.Password()));
    [Theory]
    [InlineData("Admin")] [InlineData("Admissions")] [InlineData("AcademicAffairs")]
    [InlineData("Accountant")] [InlineData("Teacher")] [InlineData("Director")] [InlineData("Student")]
    public async Task AllSevenFixedRolesUseCurrentRoleContextAndSameRejectedDecision(string role)
    {
        await using var f=await SqlFixture.CreateAsync();var password=await AuthTestSupport.Credential(f);
        await f.ExecuteAsync($"UPDATE dbo.Role SET role_code={role} WHERE role_id={f.Teacher.RoleId};");
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        var service=Service(scope,new(new(SqlFixture.Instant)));
        var success=await service.LoginAsync(AuthTestSupport.Login(password),TestContext.Current.CancellationToken);
        Assert.True(success.Accepted);Assert.Equal(role,success.Grant!.Context.Role);
        var rejected=await service.LoginAsync(AuthTestSupport.Login(AuthTestSupport.Password()),TestContext.Current.CancellationToken);
        Assert.False(rejected.Accepted);Assert.Null(rejected.Grant);
        Assert.Equal(1,await f.Context.AuthSessions.CountAsync(TestContext.Current.CancellationToken));
        await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Fact]
    public async Task RealCountersBlockFiveFailuresForFullFifteenMinutesAndExpiryDoesNotChangeStatus()
    {
        await using var f=await SqlFixture.CreateAsync();var password=await AuthTestSupport.Credential(f);await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        var clock=new AuthTestSupport.Clock(new(SqlFixture.Instant));var service=Service(scope,clock);
        for(var i=0;i<4;i++)Assert.False((await service.LoginAsync(AuthTestSupport.Login(AuthTestSupport.Password()),TestContext.Current.CancellationToken)).Accepted);
        clock.Now=clock.Now.AddMinutes(14);Assert.False((await service.LoginAsync(AuthTestSupport.Login(AuthTestSupport.Password()),TestContext.Current.CancellationToken)).Accepted);
        var blockedUntil=f.Teacher.LoginThrottledUntil!.Value;Assert.Equal(5,f.Teacher.FailedLoginCount);
        clock.Now=new DateTimeOffset(SqlFixture.Instant).AddMinutes(16);Assert.False((await service.LoginAsync(AuthTestSupport.Login(password),TestContext.Current.CancellationToken)).Accepted);Assert.Equal(blockedUntil,f.Teacher.LoginThrottledUntil);
        clock.Now=new DateTimeOffset(DateTime.SpecifyKind(blockedUntil,DateTimeKind.Utc));var accepted=await service.LoginAsync(AuthTestSupport.Login(password),TestContext.Current.CancellationToken);Assert.True(accepted.Accepted);
        Assert.Equal("ACTIVE",f.Teacher.Status);Assert.Equal(0,f.Teacher.FailedLoginCount);Assert.Null(f.Teacher.LoginThrottledUntil);Assert.Null(f.Teacher.FailureWindowStartedAt);
        Assert.Equal(1,await f.Context.AuthSessions.CountAsync(TestContext.Current.CancellationToken));await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Theory]
    [InlineData("unknown")] [InlineData("wrong")] [InlineData("LOCKED")] [InlineData("DISABLED")] [InlineData("expired-temp")]
    public async Task AllCredentialAndStatusFailuresReturnSameRejectedDecisionWithoutSession(string mode)
    {
        await using var f=await SqlFixture.CreateAsync();var password=await AuthTestSupport.Credential(f);await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        if(mode is "LOCKED" or "DISABLED")await f.ExecuteAsync($"UPDATE dbo.Account SET status={mode},security_version=9 WHERE account_id={f.Teacher.AccountId};");
        if(mode=="expired-temp")await f.ExecuteAsync($"UPDATE dbo.Account SET must_change_password=1,temp_issued_at={SqlFixture.Instant.AddDays(-1)},temp_expires_at={SqlFixture.Instant} WHERE account_id={f.Teacher.AccountId};");
        var decision=await Service(scope,new(new(SqlFixture.Instant))).LoginAsync(AuthTestSupport.Login(mode=="wrong"?AuthTestSupport.Password():password,mode=="unknown"?"missing@example.invalid":f.Teacher.Email),TestContext.Current.CancellationToken);
        Assert.False(decision.Accepted);Assert.Null(decision.Grant);Assert.Equal(0,await f.Context.AuthSessions.CountAsync(TestContext.Current.CancellationToken));
        if(mode is "LOCKED" or "DISABLED"){Assert.Equal(mode,f.Teacher.Status);Assert.Equal(9,f.Teacher.SecurityVersion);}
    }
    [Fact]
    public async Task ConcurrentDeviceSessionsHaveFreshSidVersionRowVersionAndNoRevocationOrSecretBody()
    {
        await using var f=await SqlFixture.CreateAsync();var password=await AuthTestSupport.Credential(f);await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        f.Teacher.FailedLoginCount=3;f.Teacher.FailureWindowStartedAt=SqlFixture.Instant;await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);var old=f.Teacher.RowVersion.ToArray();
        var clock=new AuthTestSupport.Clock(new(SqlFixture.Instant));var service=Service(scope,clock);var first=await service.LoginAsync(AuthTestSupport.Login(password),TestContext.Current.CancellationToken);var second=await service.LoginAsync(AuthTestSupport.Login(password),TestContext.Current.CancellationToken);
        Assert.True(first.Accepted&&second.Accepted);var sessions=await f.Context.AuthSessions.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);Assert.Equal(2,sessions.Count);Assert.False(sessions[0].Sid.SequenceEqual(sessions[1].Sid));Assert.All(sessions,s=>{Assert.Equal(32,s.Sid.Length);Assert.Null(s.RevokedAt);Assert.Equal(1,s.SecurityVersion);Assert.Equal(SqlFixture.Instant.AddHours(2),s.ExpiresAt);Assert.Equal(SqlFixture.Instant,s.LastActivityAt);});
        Assert.Equal("NORMAL",first.Grant!.Context.Scope);Assert.Equal("Teacher",first.Grant.Context.Role);Assert.False(old.SequenceEqual(f.Teacher.RowVersion));Assert.Equal(Convert.ToBase64String(f.Teacher.RowVersion),second.Grant!.Context.AccountVersion);
        Assert.Equal(nameof(LoginGrant),first.Grant.ToString());await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Theory]
    [InlineData(60)] [InlineData(600)] [InlineData(1200)]
    public async Task TemporaryCredentialIssuesOnlyRestrictedWindowCappedByExpiry(int remainingSeconds)
    {
        await using var f=await SqlFixture.CreateAsync();var password=await AuthTestSupport.Credential(f);await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        var tempExpiry=SqlFixture.Instant.AddSeconds(remainingSeconds);await f.ExecuteAsync($"UPDATE dbo.Account SET must_change_password=1,temp_issued_at={tempExpiry.AddDays(-1)},temp_expires_at={tempExpiry} WHERE account_id={f.Teacher.AccountId};");
        var result=await Service(scope,new(new(SqlFixture.Instant))).LoginAsync(AuthTestSupport.Login(password),TestContext.Current.CancellationToken);Assert.True(result.Accepted);Assert.Equal("PASSWORD_CHANGE",result.Grant!.Context.Scope);
        var session=await f.Context.AuthSessions.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);Assert.Equal(remainingSeconds>600?SqlFixture.Instant.AddMinutes(10):tempExpiry,session.ExpiresAt);Assert.True(f.Teacher.MustChangePassword);Assert.Equal(tempExpiry,f.Teacher.TempExpiresAt);
    }
    [Fact]
    public async Task ExpiredFailureWindowResetsBeforeCountingNextBadPassword()
    {
        await using var f=await SqlFixture.CreateAsync();await AuthTestSupport.Credential(f);await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        f.Teacher.FailedLoginCount=4;f.Teacher.FailureWindowStartedAt=SqlFixture.Instant.AddMinutes(-15);await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        Assert.False((await Service(scope,new(new(SqlFixture.Instant))).LoginAsync(AuthTestSupport.Login(AuthTestSupport.Password()),TestContext.Current.CancellationToken)).Accepted);
        Assert.Equal(1,f.Teacher.FailedLoginCount);Assert.Equal(SqlFixture.Instant,f.Teacher.FailureWindowStartedAt);Assert.Null(f.Teacher.LoginThrottledUntil);
    }
    [Fact]
    public async Task CompetingAccountLockTimesOutBeforeItCanLoseCounterUpdatesOrIssueSession()
    {
        await using var f=await SqlFixture.CreateAsync();await AuthTestSupport.Credential(f);await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        await Service(scope,new(new(SqlFixture.Instant))).LoginAsync(AuthTestSupport.Login(AuthTestSupport.Password()),TestContext.Current.CancellationToken);
        await using var other=new EnglishCenterDbContext(new DbContextOptionsBuilder<EnglishCenterDbContext>().UseSqlServer(f.Context.Database.GetConnectionString(),sql=>sql.UseCompatibilityLevel(160)).Options);
        await other.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);await using var tx=await other.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);await using var otherScope=await SqlAuthDeadlineScope.EnterAsync(other,10,100,TestContext.Current.CancellationToken);
        var error=await Assert.ThrowsAsync<SqlException>(()=>new SqlLoginRepository(otherScope).LockAccountAsync(f.Teacher.Email,TestContext.Current.CancellationToken));Assert.Equal(1222,error.Number);
        Assert.Equal(1,f.Teacher.FailedLoginCount);Assert.Equal(0,await f.Context.AuthSessions.CountAsync(TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task SigningFailureLeavesNoSessionAndCallerRollsBackTheWholeUnit()
    {
        await using var f=await SqlFixture.CreateAsync();var password=await AuthTestSupport.Credential(f);await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Service(scope,new(new(SqlFixture.Instant)),new BrokenSigner()).LoginAsync(AuthTestSupport.Login(password),TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.AuthSessions.CountAsync(TestContext.Current.CancellationToken));await AuthTestSupport.EmptyAfterRollback(f);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task SessionInsertFailureOrCancellationRollsBackPersistedCounterReset(bool cancel)
    {
        await using var f=await SqlFixture.CreateAsync();var password=await AuthTestSupport.Credential(f);
        f.Teacher.FailedLoginCount=3;f.Teacher.FailureWindowStartedAt=SqlFixture.Instant;await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        await using var context=new EnglishCenterDbContext(new DbContextOptionsBuilder<EnglishCenterDbContext>().UseSqlServer(f.Context.Database.GetDbConnection(),sql=>sql.UseCompatibilityLevel(160)).AddInterceptors(new FailSessionInsert(cancel)).Options);
        await context.Database.UseTransactionAsync(Microsoft.EntityFrameworkCore.Storage.DbContextTransactionExtensions.GetDbTransaction(f.Context.Database.CurrentTransaction!),TestContext.Current.CancellationToken);
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(context,10,200,TestContext.Current.CancellationToken);
        if(cancel)await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>Service(scope,new(new(SqlFixture.Instant))).LoginAsync(AuthTestSupport.Login(password),TestContext.Current.CancellationToken));
        else
        {
            var error=await Assert.ThrowsAsync<DbUpdateException>(()=>Service(scope,new(new(SqlFixture.Instant))).LoginAsync(AuthTestSupport.Login(password),TestContext.Current.CancellationToken));Assert.IsType<InvalidOperationException>(error.InnerException);
        }
        var account=await f.Context.Accounts.AsNoTracking().SingleAsync(a=>a.AccountId==f.Teacher.AccountId,TestContext.Current.CancellationToken);Assert.Equal(0,account.FailedLoginCount);
        Assert.Equal(0,await f.Context.AuthSessions.CountAsync(TestContext.Current.CancellationToken));await AuthTestSupport.EmptyAfterRollback(f);
    }
    private sealed class FailSessionInsert(bool cancel) : Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        public override ValueTask<Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader>> ReaderExecutingAsync(System.Data.Common.DbCommand command,Microsoft.EntityFrameworkCore.Diagnostics.CommandEventData eventData,Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader> result,CancellationToken cancellationToken=default)
        {
            if(command.CommandText.Contains("INSERT INTO [dbo].[AuthSession]",StringComparison.Ordinal))
            { if(cancel)throw new OperationCanceledException();throw new InvalidOperationException("Synthetic session insert failure."); }
            return ValueTask.FromResult(result);
        }
    }
    [Fact]
    public async Task SubsecondRemainingTemporaryDeadlineRejectsWithoutIssuingSession()
    {
        await using var f=await SqlFixture.CreateAsync();var password=await AuthTestSupport.Credential(f);
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        var expiry=SqlFixture.Instant.AddMilliseconds(1);
        await f.ExecuteAsync($"UPDATE dbo.Account SET must_change_password=1,temp_issued_at={expiry.AddDays(-1)},temp_expires_at={expiry} WHERE account_id={f.Teacher.AccountId};");
        var clock=new AuthTestSupport.Clock(new DateTimeOffset(SqlFixture.Instant).AddTicks(5000));
        var result=await Service(scope,clock).LoginAsync(AuthTestSupport.Login(password),TestContext.Current.CancellationToken);
        Assert.False(result.Accepted);Assert.Equal(0,await f.Context.AuthSessions.CountAsync(TestContext.Current.CancellationToken));
        await AuthTestSupport.EmptyAfterRollback(f);
    }
    [Fact]
    public async Task ClockCrossingIssuedDeadlineDuringSigningPreventsSessionDml()
    {
        await using var f=await SqlFixture.CreateAsync();var password=await AuthTestSupport.Credential(f);
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,TestContext.Current.CancellationToken);
        var clock=new AuthTestSupport.Clock(new(SqlFixture.Instant));
        var error=await Assert.ThrowsAsync<EnglishCenter.Api.Common.Errors.ApiErrorException>(()=>Service(scope,clock,new AdvancingSigner(clock)).LoginAsync(AuthTestSupport.Login(password),TestContext.Current.CancellationToken));
        Assert.Equal(EnglishCenter.Api.Common.Errors.ApiErrorCode.AUTHENTICATION_FAILED,error.Code);
        Assert.Equal(0,await f.Context.AuthSessions.CountAsync(TestContext.Current.CancellationToken));
        await AuthTestSupport.EmptyAfterRollback(f);
    }
    private sealed class AdvancingSigner(AuthTestSupport.Clock clock) : IJwtSessionIssuer
    {
        public string Issue(int accountId,long securityVersion,byte[] sid,SessionIssuanceWindow window)
        {
            clock.Now=window.AbsoluteExpiresAt;return "fixture-only";
        }
    }
    private sealed class BrokenSigner : IJwtSessionIssuer
    {
        public string Issue(int id,long version,byte[] sid,SessionIssuanceWindow window)=>throw new InvalidOperationException("Synthetic signer failure.");
    }
}

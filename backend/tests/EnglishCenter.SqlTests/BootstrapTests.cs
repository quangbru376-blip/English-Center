namespace EnglishCenter.SqlTests;

using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure;
using EnglishCenter.Api.Infrastructure.Bootstrap;
using EnglishCenter.Api.Services.Bootstrap;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class BootstrapTests
{
    private static CredentialHasher Hasher()=>new(new PasswordPolicy(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory,"Resources","PasswordBlocklist"))));
    private static string Password()=>new TemporaryPasswordGenerator(new PasswordPolicy(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory,"Resources","PasswordBlocklist")))).Generate();
    private static BootstrapService Service(SqlFixture f)=>new(new SqlBootstrapRepository(f.Context,10,200),Hasher());
    private static Task<BootstrapOutcome> Run(BootstrapService service,string password,CancellationToken? cancellationToken=null)=>service.EnsureAdminAsync("bootstrap@example.invalid","SQL operator fixture",password,new DateTimeOffset(SqlFixture.Instant),cancellationToken ?? TestContext.Current.CancellationToken);

    [Theory]
    [InlineData("ACTIVE")] [InlineData("LOCKED")] [InlineData("DISABLED")]
    public async Task CreatesExactRolesAndAuditedAdminThenRepeatsNeverOverwrite(string status)
    {
        await using var f=await SqlFixture.CreateAsync(isolation:IsolationLevel.Serializable);var service=Service(f);var password=Password();
        Assert.Equal(BootstrapOutcome.Created,await Run(service,password));
        Assert.True(FixedRoleCodes.MatchesExact(await f.Context.Roles.Select(r=>r.RoleCode).ToListAsync(TestContext.Current.CancellationToken)));
        var admin=await f.Context.Accounts.AsNoTracking().SingleAsync(a=>a.Email=="bootstrap@example.invalid",TestContext.Current.CancellationToken);
        Assert.True(Hasher().Verify(admin.PasswordHash,password)==PasswordVerificationResult.Success);Assert.False(admin.MustChangePassword);
        var audit=await f.Context.AuditLogs.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);Assert.Equal("SYSTEM.ADMIN.BOOTSTRAP",audit.Action);Assert.Null(audit.ActorAccountId);Assert.Null(audit.SecurityExpiresAt);Assert.DoesNotContain("email",audit.Details);
        await f.ExecuteAsync($"UPDATE dbo.Account SET status={status},security_version=5 WHERE account_id={admin.AccountId};");
        Assert.Equal(BootstrapOutcome.AlreadyConfigured,await Run(service,Password()));
        var unchanged=await f.Context.Accounts.AsNoTracking().SingleAsync(a=>a.AccountId==admin.AccountId,TestContext.Current.CancellationToken);
        Assert.Equal(status,unchanged.Status);Assert.Equal(5,unchanged.SecurityVersion);Assert.True(admin.PasswordHash==unchanged.PasswordHash);
        Assert.Equal(7,await f.Context.Roles.CountAsync(TestContext.Current.CancellationToken));Assert.Equal(1,await f.Context.AuditLogs.CountAsync(TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task SevenSyntheticRoleIdentitiesRemainRollbackOnlyAndSecretsNeverLeaveMemory()
    {
        await using var f=await SqlFixture.CreateAsync(isolation:IsolationLevel.Serializable);await Run(Service(f),Password());
        var roles=await f.Context.Roles.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);
        foreach(var role in roles.Where(r=>r.RoleCode!="Admin"))
        {
            var account=new Account { RoleId=role.RoleId,Email="test-"+role.RoleCode.ToLowerInvariant()+"@example.invalid",FullName="Synthetic fixture",PasswordHash=Hasher().Hash(Password()),Status="ACTIVE",SecurityVersion=1,CreatedAt=SqlFixture.Instant };
            f.Context.Add(account);await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
            if(role.RoleCode=="Student") { f.Context.Add(new StudentProfile { AccountId=account.AccountId,DateOfBirth=new DateOnly(2000,1,1) });await f.Context.SaveChangesAsync(TestContext.Current.CancellationToken); }
        }
        Assert.Equal(7,await f.Context.Accounts.Where(a=>a.Email.StartsWith("test-")||a.Email=="bootstrap@example.invalid").CountAsync(TestContext.Current.CancellationToken));
        await f.Context.Database.CurrentTransaction!.RollbackAsync(TestContext.Current.CancellationToken);
        Assert.Equal(0,await f.Context.Accounts.CountAsync(TestContext.Current.CancellationToken));Assert.Equal(0,await f.Context.Roles.CountAsync(TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task CollisionAndCancellationNeverCreateInitialAdmin()
    {
        await using var f=await SqlFixture.CreateAsync(isolation:IsolationLevel.Serializable);var service=Service(f);
        var error=await Assert.ThrowsAsync<ApiErrorException>(()=>service.EnsureAdminAsync(f.Teacher.Email,"Fixture",Password(),new DateTimeOffset(SqlFixture.Instant),TestContext.Current.CancellationToken));Assert.Equal(ApiErrorCode.CONFLICT,error.Code);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>Run(service,Password(),new CancellationToken(true)));
        Assert.Equal(1,await f.Context.Roles.CountAsync(TestContext.Current.CancellationToken));Assert.Equal(0,await f.Context.AuditLogs.CountAsync(TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task RuntimeRoleCannotInvokeOperatorBootstrap()
    {
        await using var f=await SqlFixture.CreateAsync(isolation:IsolationLevel.Serializable);
        await SqlRows.ReadAsync(f,"IF DATABASE_PRINCIPAL_ID('D12_fixture_user_0') IS NOT NULL THROW 51912,'Fixture exists',1; CREATE USER D12_fixture_user_0 WITHOUT LOGIN; ALTER ROLE ec_app_runtime ADD MEMBER D12_fixture_user_0; EXECUTE AS USER='D12_fixture_user_0';");
        try { var error=await Assert.ThrowsAsync<ApiErrorException>(()=>Run(Service(f),Password()));Assert.Equal(ApiErrorCode.FORBIDDEN,error.Code); }
        finally { await SqlRows.ReadAsync(f,"REVERT;"); }
        Assert.Equal(1,await f.Context.Roles.CountAsync(TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task ConcurrentCallerCannotPassAdminGateWhileFirstBootstrapIsUncommitted()
    {
        await using var f=await SqlFixture.CreateAsync(isolation:IsolationLevel.Serializable);await Run(Service(f),Password());
        await using var other=new EnglishCenterDbContext(new DbContextOptionsBuilder<EnglishCenterDbContext>().UseSqlServer(f.Context.Database.GetConnectionString(),sql=>sql.UseCompatibilityLevel(160)).Options);
        await other.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);await using var tx=await other.Database.BeginTransactionAsync(IsolationLevel.Serializable,TestContext.Current.CancellationToken);
        var service=new BootstrapService(new SqlBootstrapRepository(other,10,100),Hasher());
        var error=await Assert.ThrowsAsync<ApiErrorException>(()=>Run(service,Password()));Assert.Equal(ApiErrorCode.CONFLICT,error.Code);
        Assert.Equal(1,await f.Context.AuditLogs.CountAsync(TestContext.Current.CancellationToken));
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task FailureOrCancellationAfterRoleWritesRollsBackCompleteBootstrap(bool cancel)
    {
        await using var f=await SqlFixture.CreateAsync(isolation:IsolationLevel.Serializable);
        await using var context=new EnglishCenterDbContext(new DbContextOptionsBuilder<EnglishCenterDbContext>()
            .UseSqlServer(f.Context.Database.GetDbConnection(),sql=>sql.UseCompatibilityLevel(160)).AddInterceptors(new FailAdminInsert(cancel)).Options);
        await context.Database.UseTransactionAsync(Microsoft.EntityFrameworkCore.Storage.DbContextTransactionExtensions.GetDbTransaction(f.Context.Database.CurrentTransaction!),TestContext.Current.CancellationToken);
        var service=new BootstrapService(new SqlBootstrapRepository(context,10,200),Hasher());
        if(cancel) await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>Run(service,Password()));
        else
        {
            var error=await Assert.ThrowsAsync<DbUpdateException>(()=>Run(service,Password()));
            Assert.IsType<InvalidOperationException>(error.InnerException);
        }
        // Role inserts happened, but the caller has not committed and must discard the unit.
        Assert.Equal(7,await f.Context.Roles.CountAsync(TestContext.Current.CancellationToken));
        await f.Context.Database.CurrentTransaction!.RollbackAsync(TestContext.Current.CancellationToken);context.ChangeTracker.Clear();f.Context.ChangeTracker.Clear();
        Assert.Equal(0,await f.Context.Roles.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.Accounts.CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0,await f.Context.AuditLogs.CountAsync(TestContext.Current.CancellationToken));
    }

    private sealed class FailAdminInsert(bool cancel) : Microsoft.EntityFrameworkCore.Diagnostics.DbCommandInterceptor
    {
        public override ValueTask<Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader>> ReaderExecutingAsync(
            System.Data.Common.DbCommand command,Microsoft.EntityFrameworkCore.Diagnostics.CommandEventData eventData,
            Microsoft.EntityFrameworkCore.Diagnostics.InterceptionResult<System.Data.Common.DbDataReader> result,CancellationToken cancellationToken=default)
        {
            if(command.CommandText.Contains("INSERT INTO [dbo].[Account]",StringComparison.Ordinal))
            {
                if(cancel) throw new OperationCanceledException();
                throw new InvalidOperationException("Synthetic persistence failure.");
            }
            return ValueTask.FromResult(result);
        }
    }

}

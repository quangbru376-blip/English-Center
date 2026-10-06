namespace EnglishCenter.SqlTests;

using System.Security.Cryptography;
using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Domain.Administration;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure;
using EnglishCenter.Api.Infrastructure.Administration;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Services.Administration;
using EnglishCenter.Api.Services.Replay;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Xunit;

[CollectionDefinition("R02 R03 SQL",DisableParallelization=true)]
public sealed class ReplayAdminSqlCollection { }
[Collection("R02 R03 SQL")]
public class ReplayAdminSqlTests
{
    private static CancellationToken Ct=>TestContext.Current.CancellationToken;
    private sealed class Fixture:IAsyncDisposable
    {
        internal readonly EnglishCenterDbContext Context=new(new DbContextOptionsBuilder<EnglishCenterDbContext>()
            .UseSqlServer("Server=lpc:.\\SQLEXPRESS;Database=EnglishCenter_D07_20261005;Integrated Security=True;Encrypt=True;TrustServerCertificate=True",s=>s.UseCompatibilityLevel(160)).Options);
        internal readonly AuthTestSupport.Clock Time=new(new(SqlFixture.Instant));
        internal BusinessClock Clock=>new(Time);
        internal IDbContextTransaction Tx=null!;internal Account Actor=null!,Target=null!;
        internal VerifiedSessionToken Proof=null!;internal SqlAuthDeadlineScope Scope=null!;
        internal static async Task<Fixture> Create(string targetRole="Teacher",string actorRole="Admin",bool restricted=false,bool snapshot=false)
        {
            var f=new Fixture();
            try
            {
                await f.Context.Database.OpenConnectionAsync(Ct);
                Assert.Equal("EnglishCenter_D07_20261005",f.Context.Database.GetDbConnection().Database);
                f.Tx=await f.Context.Database.BeginTransactionAsync(snapshot?System.Data.IsolationLevel.Snapshot:System.Data.IsolationLevel.ReadCommitted,Ct);
                var roles=FixedRoleCodes.All.Select(r=>new Role{RoleCode=r,RoleName=r}).ToArray();
                f.Context.Roles.AddRange(roles);await f.Context.SaveChangesAsync(Ct);
                var hash=AuthTestSupport.Hasher().Hash(AuthTestSupport.Password());
                f.Target=new(){RoleId=roles.Single(r=>r.RoleCode==targetRole).RoleId,FullName="Synthetic target",Email="target-fixture@example.invalid",
                    Phone=targetRole=="Student"?"0912345678":null,Status="ACTIVE",PasswordHash=hash,SecurityVersion=1,CreatedAt=SqlFixture.Instant};
                f.Actor=new(){RoleId=roles.Single(r=>r.RoleCode==actorRole).RoleId,FullName="Synthetic actor",Email="actor-fixture@example.invalid",
                    Status="ACTIVE",PasswordHash=hash,SecurityVersion=1,CreatedAt=SqlFixture.Instant,MustChangePassword=restricted,
                    TempIssuedAt=restricted?SqlFixture.Instant:null,TempExpiresAt=restricted?SqlFixture.Instant.AddHours(24):null};
                // Target ID deliberately precedes caller to exercise ascending cross-auth order.
                f.Context.Accounts.Add(f.Target);await f.Context.SaveChangesAsync(Ct);
                f.Context.Accounts.Add(f.Actor);await f.Context.SaveChangesAsync(Ct);
                var sid=RandomNumberGenerator.GetBytes(32);var window=restricted?SessionIssuanceWindow.Restricted(f.Time.Now,f.Time.Now.AddHours(24)):SessionIssuanceWindow.Normal(f.Time.Now);
                f.Context.AuthSessions.Add(new(){Sid=sid,AccountId=f.Actor.AccountId,SecurityVersion=1,SessionScope=window.Scope,
                    IssuedAt=SqlFixture.Instant,ExpiresAt=window.AbsoluteExpiresAt.UtcDateTime,LastActivityAt=SqlFixture.Instant});
                for(var i=0;i<3;i++)f.Context.AuthSessions.Add(new(){Sid=RandomNumberGenerator.GetBytes(32),AccountId=f.Target.AccountId,
                    SecurityVersion=1,SessionScope="NORMAL",IssuedAt=SqlFixture.Instant,ExpiresAt=SqlFixture.Instant.AddHours(2),
                    LastActivityAt=SqlFixture.Instant,RevokedAt=i==2?SqlFixture.Instant:null});
                await f.Context.SaveChangesAsync(Ct);
                var key=new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
                var jwt=new JwtSessionIssuer(new(key,"HS256"),"fixture","fixture").Issue(f.Actor.AccountId,1,sid,window);
                f.Proof=new JwtSessionValidator([key],["HS256"],"fixture","fixture",f.Clock,4096).Validate(jwt)!;
                if(!snapshot)f.Scope=await SqlAuthDeadlineScope.EnterAsync(f.Context,10,200,Ct);
                return f;
            }
            catch{await f.DisposeAsync();throw;}
        }
        internal SqlReplayRepository Markers=>new(Scope,200);
        internal AdminAccountService Service=>new(new SqlAdminAccountRepository(Scope),Markers,Clock);
        internal AdminLockInput Input(Guid? key=null,string? rv=null,int? target=null)=>AdminLockInput.Parse(target??Target.AccountId,key??Guid.NewGuid(),
            JsonSerializer.Serialize(new{expectedVersions=new{account=rv??RowVersionCodec.Encode(Target.RowVersion)}}),4096);
        internal Task<int> Sql(FormattableString sql)=>Context.Database.ExecuteSqlInterpolatedAsync(sql,Ct);
        internal async Task RollbackEmpty()
        {
            await Tx.RollbackAsync(Ct);Context.ChangeTracker.Clear();
            Assert.Equal(0,await Context.Accounts.CountAsync(Ct));Assert.Equal(0,await Context.AuthSessions.CountAsync(Ct));
            Assert.Equal(0,await Context.ReplayOperations.CountAsync(Ct));Assert.Equal(0,await Context.AuditLogs.CountAsync(Ct));
        }
        public async ValueTask DisposeAsync()
        {
            if(Scope is not null)await Scope.DisposeAsync();
            if(Tx is not null){await Tx.DisposeAsync();}await Context.DisposeAsync();
        }
    }
    [Theory][InlineData("Student")][InlineData("Teacher")][InlineData("Admissions")][InlineData("AcademicAffairs")]
    [InlineData("Accountant")][InlineData("Director")][InlineData("Admin")]
    public async Task LockCommitsOneLogicalUnitInCallerTxAndReplayDoesNotRepeat(string role)
    {
        await using var f=await Fixture.Create(role);var before=f.Target.RowVersion.ToArray();var hash=f.Target.PasswordHash;var key=Guid.NewGuid();var input=f.Input(key);
        f.Time.Now=f.Time.Now.AddMinutes(1);
        var result=await f.Service.LockAsync(f.Proof,input,Ct);
        Assert.Equal("LOCKED",result.Account.Status);Assert.NotEqual(RowVersionCodec.Encode(before),result.Account.Version);
        Assert.Equal(2,f.Target.SecurityVersion);Assert.Equal(hash,f.Target.PasswordHash);
        Assert.Equal(1,await f.Context.AuditLogs.CountAsync(Ct));Assert.Equal(1,await f.Context.ReplayOperations.CountAsync(Ct));
        var rows=await f.Context.AuthSessions.AsNoTracking().ToListAsync(Ct);
        Assert.All(rows.Where(s=>s.AccountId==f.Target.AccountId),s=>Assert.NotNull(s.RevokedAt));
        var actor=Assert.Single(rows,s=>s.AccountId==f.Actor.AccountId);Assert.Null(actor.RevokedAt);Assert.Equal(f.Time.Now.UtcDateTime,actor.LastActivityAt);
        var audit=await f.Context.AuditLogs.SingleAsync(Ct);Assert.Equal(result.OperationId,audit.OperationId);
        Assert.Equal("ADMIN.ACCOUNT.LOCK",audit.Action);Assert.DoesNotContain(hash,audit.Details!);
        var marker=await f.Context.ReplayOperations.SingleAsync(Ct);Assert.Equal(marker.CompletedAt.AddHours(24),marker.ExpiresAt);
        Assert.Equal(input.Intent.CopyDigest(),marker.InputDigest);Assert.Equal(result.OperationId,marker.OperationId);
        var serialized=JsonSerializer.Serialize(result.Account,new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase});
        using var view=JsonDocument.Parse(serialized);Assert.Equal(12,view.RootElement.EnumerateObject().Count());
        Assert.DoesNotContain("passwordHash",serialized);Assert.DoesNotContain("securityVersion",serialized);
        var version=f.Target.RowVersion.ToArray();f.Time.Now=f.Time.Now.AddMinutes(1);
        var replay=await f.Service.LockAsync(f.Proof,input,Ct);Assert.Equal(result.OperationId,replay.OperationId);
        Assert.Equal(version,f.Target.RowVersion);Assert.Equal(actor.LastActivityAt,(await f.Context.AuthSessions.AsNoTracking().SingleAsync(s=>s.AccountId==f.Actor.AccountId,Ct)).LastActivityAt);
        Assert.Equal(1,await f.Context.AuditLogs.CountAsync(Ct));Assert.Equal(1,await f.Context.ReplayOperations.CountAsync(Ct));
        var lookup=await f.Service.LookupAsync(f.Proof,"ADMIN.ACCOUNT.LOCK",key,Ct);Assert.Equal(result.OperationId,lookup.OperationId);
        Assert.Equal(f.Target.AccountId,lookup.Target["accountId"]);Assert.Equal("SAFE_REFERENCE",lookup.ResultMode);
        Assert.Equal(ApiErrorCode.STATE_CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>f.Service.LockAsync(f.Proof,f.Input(),Ct))).Code);
        Assert.Equal(ApiErrorCode.IDEMPOTENCY_CONTENT_MISMATCH,(await Assert.ThrowsAsync<ApiErrorException>(()=>f.Service.LockAsync(f.Proof,f.Input(key),Ct))).Code);
        Assert.Equal(ApiErrorCode.IDEMPOTENCY_CONTENT_MISMATCH,(await Assert.ThrowsAsync<ApiErrorException>(()=>f.Service.LockAsync(f.Proof,
            f.Input(key,RowVersionCodec.Encode(f.Actor.RowVersion),f.Actor.AccountId),Ct))).Code);
        await f.RollbackEmpty();
    }
    [Theory][InlineData("self")][InlineData("stale")][InlineData("state")][InlineData("overflow")]
    [InlineData("revoked")][InlineData("security")][InlineData("expiry")][InlineData("role")][InlineData("restricted")][InlineData("missing")]
    public async Task GuardFailureLeavesNoAuditOrCompletion(string fault)
    {
        await using var f=await Fixture.Create(actorRole:fault=="role"?"Teacher":"Admin",restricted:fault=="restricted");
        if(fault=="state")f.Target.Status="LOCKED";if(fault=="overflow")f.Target.SecurityVersion=long.MaxValue;
        if(fault=="security")f.Actor.SecurityVersion++;
        if(fault=="revoked"){var row=await f.Context.AuthSessions.SingleAsync(s=>s.AccountId==f.Actor.AccountId,Ct);row.RevokedAt=SqlFixture.Instant;}
        await f.Context.SaveChangesAsync(Ct);
        if(fault=="expiry")f.Time.Now=f.Time.Now.AddMinutes(30);
        var input=f.Input(rv:fault=="stale"?"AAAAAAAAAAE=":fault=="self"?RowVersionCodec.Encode(f.Actor.RowVersion):null,target:fault=="self"?f.Actor.AccountId:fault=="missing"?int.MaxValue:null);
        var e=await Assert.ThrowsAsync<ApiErrorException>(()=>f.Service.LockAsync(f.Proof,input,Ct));
        Assert.Equal(fault switch{"role" or "restricted"=>ApiErrorCode.FORBIDDEN,"security" or "revoked" or "expiry"=>ApiErrorCode.AUTHENTICATION_REQUIRED,
            "stale"=>ApiErrorCode.CONFLICT,"missing"=>ApiErrorCode.NOT_FOUND,_=>ApiErrorCode.STATE_CONFLICT},e.Code);
        Assert.Equal(0,await f.Context.AuditLogs.CountAsync(Ct));Assert.Equal(0,await f.Context.ReplayOperations.CountAsync(Ct));await f.RollbackEmpty();
    }
    [Theory][InlineData("Account")][InlineData("AuthSession")][InlineData("AuditLog")][InlineData("ReplayOperation")]
    public async Task DeniedDmlAuditOrCompletionRollsBackWholeUnit(string table)
    {
        await using var f=await Fixture.Create();await f.Sql($"CREATE USER D12_fixture_user_0 WITHOUT LOGIN; ALTER ROLE ec_app_runtime ADD MEMBER D12_fixture_user_0;");
        if(table=="Account")await f.Sql($"DENY UPDATE ON dbo.Account TO D12_fixture_user_0;");
        else if(table=="AuthSession")await f.Sql($"DENY UPDATE ON dbo.AuthSession TO D12_fixture_user_0;");
        else if(table=="AuditLog")await f.Sql($"DENY INSERT ON dbo.AuditLog TO D12_fixture_user_0;");
        else await f.Sql($"DENY INSERT ON dbo.ReplayOperation TO D12_fixture_user_0;");
        await f.Sql($"EXECUTE AS USER='D12_fixture_user_0';");
        try{await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.LockAsync(f.Proof,f.Input(),Ct));}
        finally{await f.Sql($"REVERT;");}
        await f.RollbackEmpty();
    }
    [Theory][InlineData("ADMIN.STAFF.CREATE")][InlineData("ADMIN.ACCOUNT.RESET_PASSWORD")][InlineData("ADMIN.ACCOUNT.LOCK")]
    [InlineData("ADMIN.ACCOUNT.UNLOCK")][InlineData("ADMIN.ACCOUNT.DISABLE")][InlineData("ADMIN.ACCOUNT.RESTORE")]
    public async Task LookupOwnSafeMarkerDoesNotExposeAccountOrSecret(string code)
    {
        await using var f=await Fixture.Create();var action=CanonicalActionRegistry.ForCode(code);var key=Guid.NewGuid();var op=Guid.NewGuid();
        var safe=SafeCommandCompletion.Create(action,op,new Dictionary<string,int>{{"accountId",f.Target.AccountId}});
        f.Context.ReplayOperations.Add(new(){ActorAccountId=f.Actor.AccountId,ActionCode=code,RequestId=key,OperationId=op,InputDigest=new byte[32],
            ResultMode=action.ResultMode,ResultReference=safe.EncodeReference(),CompletedAt=SqlFixture.Instant,ExpiresAt=SqlFixture.Instant.AddHours(24)});
        await f.Context.SaveChangesAsync(Ct);
        var result=await f.Service.LookupAsync(f.Proof,code,key,Ct);Assert.Equal(op,result.OperationId);Assert.Equal(action.ResultMode,result.ResultMode);
        var json=JsonSerializer.Serialize(result);Assert.DoesNotContain(f.Target.Email,json);Assert.DoesNotContain(f.Target.PasswordHash,json);
        Assert.Equal(0,await f.Context.AuditLogs.CountAsync(Ct));await f.RollbackEmpty();
    }
    [Fact] public async Task CurrentAuthorityCutsOffExpiredRetainedMarkerAndReadDoesNotRenew()
    {
        await using var f=await Fixture.Create();var input=f.Input();var result=await f.Service.LockAsync(f.Proof,input,Ct);
        var marker=await f.Context.ReplayOperations.SingleAsync(Ct);marker.CompletedAt=SqlFixture.Instant.AddHours(-24);marker.ExpiresAt=SqlFixture.Instant;await f.Context.SaveChangesAsync(Ct);
        Assert.Equal(ApiErrorCode.RESULT_NOT_CONFIRMED,(await Assert.ThrowsAsync<ApiErrorException>(()=>f.Service.LookupAsync(f.Proof,"ADMIN.ACCOUNT.LOCK",input.RequestId,Ct))).Code);
        Assert.Equal(ApiErrorCode.CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>f.Service.LockAsync(f.Proof,input,Ct))).Code);
        var detail=await f.Service.DetailAsync(f.Proof,f.Target.AccountId,Ct);Assert.Equal("LOCKED",detail.Status);
        f.Actor.Status="LOCKED";await f.Context.SaveChangesAsync(Ct);
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,(await Assert.ThrowsAsync<ApiErrorException>(()=>f.Service.LookupAsync(f.Proof,"ADMIN.ACCOUNT.LOCK",input.RequestId,Ct))).Code);
        Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,(await Assert.ThrowsAsync<ApiErrorException>(()=>f.Service.DetailAsync(f.Proof,f.Target.AccountId,Ct))).Code);
        await f.RollbackEmpty();
    }
    [Fact] public async Task RequestAndAdminSetGatesAndAccountRowsBlockCompetingConnection()
    {
        await using var f=await Fixture.Create();var input=f.Input();await f.Service.LockAsync(f.Proof,input,Ct);
        await using var other=new EnglishCenterDbContext(new DbContextOptionsBuilder<EnglishCenterDbContext>().UseSqlServer(f.Context.Database.GetConnectionString(),s=>s.UseCompatibilityLevel(160)).Options);
        await other.Database.OpenConnectionAsync(Ct);await using var tx=await other.Database.BeginTransactionAsync(Ct);
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(other,10,100,Ct);var markers=new SqlReplayRepository(scope,100);
        foreach(var resource in new[]{ReplayService.RequestResource(f.Actor.AccountId,input.Intent.Action,input.RequestId),"EC:ADMINSET"})
            Assert.Equal(ApiErrorCode.SERVICE_UNAVAILABLE,(await Assert.ThrowsAsync<ApiErrorException>(()=>markers.AcquireAsync(resource,Ct))).Code);
        var repo=new SqlAdminAccountRepository(scope);
        Assert.Equal(1222,(await Assert.ThrowsAsync<SqlException>(()=>repo.LockAccountAsync(f.Target.AccountId,Ct))).Number);
        var sid=await f.Context.AuthSessions.Where(s=>s.AccountId==f.Target.AccountId).Select(s=>s.Sid).FirstAsync(Ct);
        Assert.Equal(1222,(await Assert.ThrowsAsync<SqlException>(()=>repo.LockSessionAsync(sid,Ct))).Number);
        await tx.RollbackAsync(Ct);await f.RollbackEmpty();
    }

    private sealed class FaultAfterCompletion(IReplayRepository inner,bool cancel):IReplayRepository
    {
        public Task AcquireAsync(string resource,CancellationToken ct)=>inner.AcquireAsync(resource,ct);
        public Task<ReplayOperation?> FindAsync(int actor,string action,Guid request,CancellationToken ct)=>inner.FindAsync(actor,action,request,ct);
        public async Task AppendAsync(ReplayOperation row,CancellationToken ct)
        { await inner.AppendAsync(row,ct);if(cancel)throw new OperationCanceledException(ct);throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR); }
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task FailureAfterActualCompletionAuditAndDmlRollsBackAll(bool cancel)
    {
        await using var f=await Fixture.Create();var service=new AdminAccountService(new SqlAdminAccountRepository(f.Scope),new FaultAfterCompletion(f.Markers,cancel),f.Clock);
        if(cancel)await Assert.ThrowsAsync<OperationCanceledException>(()=>service.LockAsync(f.Proof,f.Input(),Ct));
        else await Assert.ThrowsAsync<ApiErrorException>(()=>service.LockAsync(f.Proof,f.Input(),Ct));
        Assert.Equal(1,await f.Context.AuditLogs.CountAsync(Ct));Assert.Equal(1,await f.Context.ReplayOperations.CountAsync(Ct));
        Assert.Equal("LOCKED",(await f.Context.Accounts.AsNoTracking().SingleAsync(a=>a.AccountId==f.Target.AccountId,Ct)).Status);
        await f.RollbackEmpty();
    }
    [Fact] public async Task SqlUniquePartitionBackstopIsReal()
    {
        await using var f=await Fixture.Create();await f.Service.LockAsync(f.Proof,f.Input(),Ct);
        var row=await f.Context.ReplayOperations.AsNoTracking().SingleAsync(Ct);
        f.Context.ReplayOperations.Add(new(){ActorAccountId=row.ActorAccountId,ActionCode=row.ActionCode,RequestId=row.RequestId,InputDigest=row.InputDigest,
            OperationId=Guid.NewGuid(),ResultMode=row.ResultMode,ResultReference=row.ResultReference,CompletedAt=row.CompletedAt,ExpiresAt=row.ExpiresAt});
        var e=await Assert.ThrowsAsync<DbUpdateException>(()=>f.Context.SaveChangesAsync(Ct));
        Assert.Contains(((SqlException)e.InnerException!).Number,new[]{2601,2627});await f.RollbackEmpty();
    }

    [Fact] public async Task SnapshotDetailBuffersExactDataWithoutAuthOrMutationAndRequiresSnapshot()
    {
        await using(var f=await Fixture.Create(snapshot:true))
        {
            var reader=new SqlAdminAccountSnapshotReader(f.Context,10);
            var view=await reader.ReadAsync(f.Actor.AccountId,f.Target.AccountId,Ct);
            Assert.Equal(f.Target.Email,view!.Email);Assert.Equal("Teacher",view.Role);
            Assert.Null(await reader.ReadAsync(f.Actor.AccountId,int.MaxValue,Ct));
            Assert.Equal(0,await f.Context.AuditLogs.CountAsync(Ct));Assert.Equal(0,await f.Context.ReplayOperations.CountAsync(Ct));await f.RollbackEmpty();
        }
        await using(var f=await Fixture.Create())
        {
            Assert.Equal(ApiErrorCode.STATE_CONFLICT,(await Assert.ThrowsAsync<ApiErrorException>(()=>new SqlAdminAccountSnapshotReader(f.Context,10).ReadAsync(f.Actor.AccountId,f.Target.AccountId,Ct))).Code);
            await f.RollbackEmpty();
        }
    }
}

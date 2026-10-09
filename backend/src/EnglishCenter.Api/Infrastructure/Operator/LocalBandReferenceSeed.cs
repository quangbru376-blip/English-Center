using System.Data;
using System.Security.Principal;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Infrastructure.Identity;
using EnglishCenter.Api.Infrastructure.Replay;
using EnglishCenter.Api.Infrastructure.Auditing;
using EnglishCenter.Api.Common.Auditing;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenter.Api.Infrastructure.Operator;

// Explicit operator provisioning of accepted reference data; never a GET repair.
public static class LocalBandReferenceSeed
{
    public static async Task<bool> RunAsync(int expectedId,Guid expectedGuid,CancellationToken ct)
    {
        if(!OperatingSystem.IsWindows()||expectedId<=4||expectedGuid==Guid.Empty)return false;
        using var identity=WindowsIdentity.GetCurrent();
        if(identity.Name!=Environment.MachineName+@"\EcM0Schema"||new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator))return false;
        using var deadline=CancellationTokenSource.CreateLinkedTokenSource(ct);deadline.CancelAfter(30000);
        try
        {
            await using var db=new EnglishCenterDbContext(new DbContextOptionsBuilder<EnglishCenterDbContext>()
                .UseSqlServer(LocalRuntimeReferences.SqlConnection,s=>s.UseCompatibilityLevel(160).CommandTimeout(10)).Options);
            await db.Database.OpenConnectionAsync(deadline.Token);
            await using(var command=db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandTimeout=10;
                command.CommandText="SELECT CONVERT(int,DB_ID()),r.database_guid,CASE WHEN DB_NAME()=N'EnglishCenter_M0_LOCALDEV' AND USER_NAME()=N'M0SchemaUser' AND ORIGINAL_LOGIN()=@login AND IS_ROLEMEMBER('db_owner')=1 AND IS_SRVROLEMEMBER('sysadmin')=0 AND HAS_PERMS_BY_NAME(NULL,NULL,'CONTROL SERVER')=0 THEN 1 ELSE 0 END FROM sys.database_recovery_status r WHERE r.database_id=DB_ID()";
                command.Parameters.Add(new SqlParameter("@login",SqlDbType.NVarChar,256){Value=identity.Name});
                await using var reader=await command.ExecuteReaderAsync(deadline.Token);
                if(!await reader.ReadAsync(deadline.Token)||reader.GetInt32(0)!=expectedId||reader.GetGuid(1)!=expectedGuid||reader.GetInt32(2)!=1||await reader.ReadAsync(deadline.Token))return false;
            }
            if(!(await db.Database.GetAppliedMigrationsAsync(deadline.Token)).SequenceEqual(db.Database.GetMigrations(),StringComparer.Ordinal))return false;
            return await SeedAsync(db,deadline.Token);
        }
        catch(OperationCanceledException)when(ct.IsCancellationRequested){throw;}
        catch(Exception){return false;}
    }
    public static async Task<bool> SeedAsync(EnglishCenterDbContext db,CancellationToken ct)
    {
        if(db.Database.CurrentTransaction is not null)throw new InvalidOperationException("Owned transaction required.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        await using var scope=await SqlAuthDeadlineScope.EnterAsync(db,10,3000,ct);
        await new SqlReplayRepository(scope,3000).AcquireAsync("EC:BANDSET",ct);
        var expected=new decimal[]{0,25,45,65,85};var existing=await db.Bands.OrderBy(b=>b.MinScore).ThenBy(b=>b.BandId).ToListAsync(ct);
        if(existing.Count>0)return existing.Count==5&&!existing.Where((b,i)=>b.BandCode!="Band"+(i+1)||b.MinScore!=expected[i]).Any();
        if(await db.EntryTestAttempts.AnyAsync(a=>a.Score!=null&&a.CompletedBookingId!=null,ct))return false;
        var seeded=expected.Select((score,i)=>new Band{BandCode="Band"+(i+1),MinScore=score}).ToArray();
        db.Bands.AddRange(seeded);await db.SaveChangesAsync(ct);
        var operation=Guid.NewGuid();var now=DateTimeOffset.UtcNow;
        foreach(var band in seeded)await new SqlAuditAppender().AppendMandatoryAsync((SqlConnection)db.Database.GetDbConnection(),
            (SqlTransaction)tx.GetDbTransaction(),AuditEntry.InitialBand(band.BandId,band.BandCode,band.MinScore,operation,now),10,ct);
        await tx.CommitAsync(ct);return true;
    }
}

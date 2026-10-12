namespace EnglishCenter.Tests.Api.Integration;
using System.Data;
using System.Data.Common;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
[Collection("V01 HTTP SQL")]
public sealed class M4ReportsManagerSqlTests
{
 private sealed class Probe(OwnedSqlFixture f,M1AdmissionsSqlTests.Clock time,bool extend):DbTransactionInterceptor
 {
  public bool Fired;
  public override async ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction tx,TransactionEventData data,InterceptionResult result,CancellationToken ct=default)
  {
   if(!Fired&&tx.IsolationLevel==IsolationLevel.Snapshot){Fired=true;await using var db=f.Db();if(extend){foreach(var sid in await db.AuthSessions.Where(s=>s.AccountId==f.Accounts["Accountant"]).ToArrayAsync(ct))sid.LastActivityAt=time.Now.AddMinutes(20).UtcDateTime;}else{(await db.Accounts.SingleAsync(a=>a.AccountId==f.Accounts["Accountant"],ct)).Status="LOCKED";}await db.SaveChangesAsync(ct);if(extend)time.Now=time.Now.AddMinutes(31);}return result;
  }
 }
 [Theory(Skip="Assigned M4 owned SQL report final authority/cap.",SkipUnless=nameof(OwnedSqlGate.Enabled),SkipType=typeof(OwnedSqlGate))]
 [InlineData(false)][InlineData(true)]
 public async Task ExportRejectsFinalLockAndOriginalCapEvenWhenLiveActivityExtended(bool extend)
 {
  var time=new M1AdmissionsSqlTests.Clock();await using var f=await OwnedSqlFixture.CreateAsync(time);var token=f.Token(await f.Grant("Accountant"));var probe=new Probe(f,time,extend);var executor=new SqlReportsExecutor(()=>f.Db(probe),f.Clock,10,1000);var error=await Assert.ThrowsAsync<ApiErrorException>(()=>executor.Csv(token,new QueryCollection(),OwnedSqlFixture.Ct));Assert.Equal(ApiErrorCode.AUTHENTICATION_REQUIRED,error.Code);Assert.True(probe.Fired);
 }
}

using System.Text.Json;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Infrastructure;
using EnglishCenter.Api.Infrastructure.Operator;
using Microsoft.EntityFrameworkCore;

internal enum DemoFixtureStep { Scenarios, Chronology, Contacts, DraftReports }
internal static class DemoScenarios
{
    const string Root=@"C:\ProgramData\EnglishCenter\DemoOperator";
    public static async Task<int> Run(DemoFixtureStep step=DemoFixtureStep.Scenarios)
    {
        var marker=Path.Combine(Root,step.ToString().ToLowerInvariant()+".json");
        if(File.Exists(marker))throw new InvalidOperationException("Scenario fixture already applied; refusing repeat");
        using var receipt=JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(Root,"catalog.json")));
        await using var db=new EnglishCenterDbContext(new DbContextOptionsBuilder<EnglishCenterDbContext>().UseSqlServer(LocalRuntimeReferences.SqlConnection,o=>o.UseCompatibilityLevel(160)).Options);
        await db.Database.OpenConnectionAsync();
        await using(var command=db.Database.GetDbConnection().CreateCommand()){
            command.CommandText="SELECT CAST(DB_ID() AS int),CONVERT(varchar(36),database_guid) FROM sys.database_recovery_status WHERE database_id=DB_ID()";
            await using var reader=await command.ExecuteReaderAsync();
            if(!await reader.ReadAsync() || reader.GetInt32(0)!=receipt.RootElement.GetProperty("databaseId").GetInt32() || reader.GetString(1)!=receipt.RootElement.GetProperty("databaseGuid").GetString())throw new InvalidOperationException("Demo catalog identity mismatch");
        }
        if(await db.Accounts.CountAsync()!=67 || await db.Registrations.CountAsync()!=70 || await db.Classes.CountAsync()!=12)throw new InvalidOperationException("Demo fixture changed; refusing enrichment");
        await using var tx=await db.Database.BeginTransactionAsync();
        if(step==DemoFixtureStep.Contacts){
            var profiles=await db.StudentProfiles.OrderBy(x=>x.StudentId).ToArrayAsync();
            for(var i=0;i<profiles.Length;i++){var account=await db.Accounts.SingleAsync(x=>x.AccountId==profiles[i].AccountId);if(account.Phone is null)account.Phone=$"090000{i+1:0000}";}
            await db.SaveChangesAsync();
        }else if(step==DemoFixtureStep.DraftReports){
            var missing=await db.StudentClassAssignments.Where(a=>!db.GradeReports.Any(r=>r.ClassAssignmentId==a.ClassAssignmentId)).Select(a=>a.ClassAssignmentId).ToArrayAsync();
            db.GradeReports.AddRange(missing.Select(id=>new GradeReport{ClassAssignmentId=id,Status="DRAFT"}));await db.SaveChangesAsync();
        }else if(step==DemoFixtureStep.Chronology)await NormalizeHistory(db);else await Populate(db,DateTime.UtcNow);await tx.CommitAsync();
        await File.WriteAllTextAsync(marker,JsonSerializer.Serialize(new {success=true,synthetic=true,appliedAt=DateTime.UtcNow}));
        Console.WriteLine("Demo scenario fixture applied and database identity verified.");return 0;
    }
    public static async Task Populate(EnglishCenterDbContext db,DateTime now)
    {
        async Task Gate(string key)=>await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @r int;EXEC @r=sys.sp_getapplock @Resource={key},@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public',@LockTimeout=3000;IF @r<0 THROW 51902,'Demo gate refused',1;");
        var classes=await db.Classes.OrderBy(x=>x.ClassCode).ToArrayAsync();
        foreach(var c in classes){await Gate($"EC:PROGRAM:{c.ProgramId}");await Gate($"EC:CLASS:{c.ClassId}");}
        var courses=await db.Courses.OrderBy(x=>x.CourseCode).ToArrayAsync();var sessions=await db.EntryTestSessions.OrderBy(x=>x.SessionId).ToArrayAsync();
        var students=await db.StudentProfiles.OrderBy(x=>x.StudentId).ToArrayAsync();
        var admissions=await db.Accounts.Where(x=>x.RoleId==db.Roles.Single(r=>r.RoleCode=="Admissions").RoleId).OrderBy(x=>x.AccountId).FirstAsync();
        var academic=await db.Accounts.Where(x=>x.RoleId==db.Roles.Single(r=>r.RoleCode=="AcademicAffairs").RoleId).OrderBy(x=>x.AccountId).FirstAsync();
        var registrations=await db.Registrations.OrderBy(x=>x.RegistrationId).ToArrayAsync();
        // Leave Student01 genuinely new, while preserving 70 registrations and uniqueness.
        foreach(var r in registrations.Where(x=>x.StudentId==students[0].StudentId))r.StudentId=students[47].StudentId;
        var second=registrations[1];second.Status="BAND_ASSIGNED";
        var first=new EntryTestAttempt {RegistrationId=second.RegistrationId,AttemptNumber=1};db.EntryTestAttempts.Add(first);await db.SaveChangesAsync();
        var firstBooking=new EntryTestBooking {AttemptId=first.AttemptId,SessionId=sessions[0].SessionId,Status="COMPLETED",BookedAt=now.AddDays(-76)};db.EntryTestBookings.Add(firstBooking);await db.SaveChangesAsync();
        first.Score=0;first.GradedBy=sessions[0].GraderId;first.GradedAt=sessions[0].EndAt.AddMinutes(5);first.CompletedBookingId=firstBooking.BookingId;
        var retake=new EntryTestAttempt {RegistrationId=second.RegistrationId,AttemptNumber=2};db.EntryTestAttempts.Add(retake);await db.SaveChangesAsync();
        db.EntryTestBookings.Add(new EntryTestBooking {AttemptId=retake.AttemptId,SessionId=sessions[2].SessionId,Status="ABSENT",BookedAt=now.AddDays(-74),AbsentBy=sessions[2].GraderId,AbsentAt=sessions[2].EndAt.AddMinutes(5),AbsentOperationId=Guid.NewGuid()});
        var rebook=new EntryTestBooking {AttemptId=retake.AttemptId,SessionId=sessions[3].SessionId,Status="COMPLETED",BookedAt=sessions[2].EndAt.AddHours(1)};db.EntryTestBookings.Add(rebook);await db.SaveChangesAsync();
        retake.Score=35;retake.GradedBy=sessions[3].GraderId;retake.GradedAt=sessions[3].EndAt.AddMinutes(5);retake.CompletedBookingId=rebook.BookingId;
        var b2=await db.Bands.SingleAsync(x=>x.BandCode=="B2");db.BandAssignments.Add(new BandAssignment {RegistrationId=second.RegistrationId,BestAttemptId=retake.AttemptId,BandId=b2.BandId,AssignedAt=retake.GradedAt.Value});
        // A past, still-open session gives Teacher02 an immediately actionable ungraded roster.
        sessions[1].Status="OPEN";var waiting=registrations[2];waiting.Status="TESTING";
        var ungraded=new EntryTestAttempt {RegistrationId=waiting.RegistrationId,AttemptNumber=1};db.EntryTestAttempts.Add(ungraded);await db.SaveChangesAsync();
        db.EntryTestBookings.Add(new EntryTestBooking {AttemptId=ungraded.AttemptId,SessionId=sessions[1].SessionId,Status="SCHEDULED",BookedAt=now.AddDays(-75)});
        var cancelled=registrations[3];Cancel(cancelled,admissions.AccountId,now,"PENDING");
        var rejected=registrations[15];rejected.Status="REJECTED";rejected.IsUnfinished=false;rejected.RejectedBy=admissions.AccountId;rejected.RejectedAt=now.AddDays(-60);rejected.RejectedFromStatus="CONFIRMED";rejected.RejectionReason="DEMO: học viên đổi kế hoạch học";rejected.RejectionOperationId=Guid.NewGuid();
        var cancelledInvoice=await db.Invoices.SingleAsync(x=>x.RegistrationId==registrations[23].RegistrationId);
        cancelledInvoice.Status="CANCELLED";cancelledInvoice.CancelledBy=admissions.AccountId;cancelledInvoice.CancelledAt=now.AddDays(-1);cancelledInvoice.CancelledFromStatus="UNPAID";cancelledInvoice.CancellationReason="DEMO: hồ sơ hủy trước thanh toán";cancelledInvoice.CancellationOperationId=Guid.NewGuid();Cancel(registrations[23],admissions.AccountId,now,"ADMITTED");
        courses[5].Status="INACTIVE";var inactive=await db.TrainingPrograms.SingleAsync(x=>x.ProgramId==classes[5].ProgramId);inactive.Status="INACTIVE";
        var cancelledClass=classes[5];cancelledClass.Status="CANCELLED";cancelledClass.CancelledBy=academic.AccountId;cancelledClass.CancelledAt=now.AddDays(-1);cancelledClass.CancellationReason="DEMO: lớp chưa có học viên";cancelledClass.CancellationOperationId=Guid.NewGuid();
        classes[6].Status="OPEN";registrations[56].CourseId=courses[0].CourseId; // B2 paid waiting -> OPEN DEMO-07.
        sessions[6].Capacity=await db.EntryTestBookings.CountAsync(x=>x.SessionId==sessions[6].SessionId&&x.Status=="SCHEDULED");
        sessions[11].Status="CANCELLED";sessions[11].CancelledBy=academic.AccountId;sessions[11].CancelledAt=now;sessions[11].CancellationReason="DEMO: đổi lịch tổ chức";sessions[11].CancellationOperationId=Guid.NewGuid();
        var live=classes[7];var lessons=await db.Schedules.Where(x=>x.ClassId==live.ClassId).OrderBy(x=>x.SessionDate).ToArrayAsync();
        var today=DateOnly.FromDateTime(now.AddHours(7));for(var j=0;j<lessons.Length;j++)lessons[j].SessionDate=today.AddDays(j-1);
        lessons[0].OccurrenceState="HELD";lessons[0].HeldBy=live.TeacherId;lessons[0].HeldAt=today.AddDays(-1).ToDateTime(new TimeOnly(13,0),DateTimeKind.Utc);
        var assignments=await db.StudentClassAssignments.Where(x=>x.ClassId==live.ClassId).OrderBy(x=>x.ClassAssignmentId).ToArrayAsync();
        var general=await db.ScoreComponentConfigs.SingleAsync(x=>x.ProgramId==live.ProgramId&&x.Kind=="GENERAL");
        for(var j=0;j<assignments.Length;j++){
            db.Attendances.Add(new Attendance {ClassAssignmentId=assignments[j].ClassAssignmentId,ScheduleId=lessons[0].ScheduleId,Present=j!=3,EnteredBy=live.TeacherId,EnteredAt=lessons[0].HeldAt ?? throw new InvalidOperationException("Held timestamp missing")});
            db.Scores.Add(new Score {ClassAssignmentId=assignments[j].ClassAssignmentId,ComponentId=general.ComponentId,ScoreValue=65+j*5,EnteredBy=live.TeacherId,EnteredAt=now.AddMinutes(-10)});
        }
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync("UPDATE r SET is_unfinished=f.isUnfinished FROM dbo.Registration r CROSS APPLY dbo.ufn_RegistrationUnfinished(r.registration_id) f;");
        await NormalizeHistory(db);
    }
    static async Task NormalizeHistory(EnglishCenterDbContext db)
    {
        var sessions=await db.EntryTestSessions.ToDictionaryAsync(x=>x.SessionId);
        var bookings=await db.EntryTestBookings.ToDictionaryAsync(x=>x.BookingId);
        var attempts=await db.EntryTestAttempts.Where(x=>x.CompletedBookingId!=null).ToDictionaryAsync(x=>x.AttemptId);
        foreach(var a in attempts.Values)a.GradedAt=sessions[bookings[a.CompletedBookingId!.Value].SessionId].EndAt.AddMinutes(5);
        var bands=await db.BandAssignments.ToArrayAsync();
        var registrations=await db.Registrations.ToDictionaryAsync(x=>x.RegistrationId);
        foreach(var b in bands){b.AssignedAt=attempts[b.BestAttemptId].GradedAt!.Value;var r=registrations[b.RegistrationId];if(r.AdmissionConfirmedAt is {} confirmed && confirmed<b.AssignedAt)r.AdmissionConfirmedAt=b.AssignedAt.AddHours(1);}
        await db.SaveChangesAsync();
    }
    static void Cancel(Registration r,int actor,DateTime now,string from){r.Status="CANCELLED";r.IsUnfinished=false;r.CancelledBy=actor;r.CancelledAt=now.AddDays(-1);r.CancelledFromStatus=from;r.CancellationReason="DEMO: lịch sử hủy giả lập";r.CancellationOperationId=Guid.NewGuid();}
}

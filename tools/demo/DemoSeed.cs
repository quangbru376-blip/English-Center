using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Domain.Academics;
using EnglishCenter.Api.Domain.Admissions;
using EnglishCenter.Api.Domain.Training;
using EnglishCenter.Api.Domain.Finance;
using EnglishCenter.Api.Infrastructure;
using EnglishCenter.Api.Infrastructure.Operator;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

// Operator fixture only. Never registered in the application or exposed over HTTP.
internal static class DemoSeed
{
    const string Root = @"C:\ProgramData\EnglishCenter\DemoOperator";
    const string Catalog = "EnglishCenter_DEMO_LOCAL";
    record Credential(string Email, string Password, string Role, string Scenario, bool Temporary);
    public static async Task<int> Run()
    {
        try
        {
            if (!OperatingSystem.IsWindows() || !Directory.Exists(Root)) throw new InvalidOperationException("Prepare demo operator custody first");
            var receipt = Path.Combine(Root, "catalog.json");
            var resume = File.Exists(receipt) && !File.Exists(Path.Combine(Root,"seed.json"));
            if (File.Exists(Path.Combine(Root,"seed.json"))) throw new InvalidOperationException("Demo already seeded; refusing reset");
            await using var master = new SqlConnection(new SqlConnectionStringBuilder(LocalRuntimeReferences.SqlConnection) { InitialCatalog = "master" }.ConnectionString);
            await master.OpenAsync();
            await using (var create = master.CreateCommand())
            {
                if(!resume) {
                    create.CommandText = "IF DB_ID(N'EnglishCenter_DEMO_LOCAL') IS NOT NULL THROW 51901,'Demo catalog already exists',1; CREATE DATABASE [EnglishCenter_DEMO_LOCAL] COLLATE Latin1_General_100_CI_AS_SC;";
                    await create.ExecuteNonQueryAsync();
                } else {
                    using var recorded=JsonDocument.Parse(await File.ReadAllTextAsync(receipt));
                    create.CommandText="SELECT d.database_id,CONVERT(varchar(36),r.database_guid) FROM sys.databases d JOIN sys.database_recovery_status r ON d.database_id=r.database_id WHERE d.name=N'EnglishCenter_DEMO_LOCAL';";
                    await using var check=await create.ExecuteReaderAsync();
                    if(!await check.ReadAsync() || check.GetInt32(0)!=recorded.RootElement.GetProperty("databaseId").GetInt32() || check.GetString(1)!=recorded.RootElement.GetProperty("databaseGuid").GetString()) throw new InvalidOperationException("Owned catalog identity mismatch");
                }
                create.CommandText = "ALTER DATABASE [EnglishCenter_DEMO_LOCAL] SET COMPATIBILITY_LEVEL=160; ALTER DATABASE [EnglishCenter_DEMO_LOCAL] SET ALLOW_SNAPSHOT_ISOLATION ON; ALTER DATABASE [EnglishCenter_DEMO_LOCAL] SET AUTO_CLOSE OFF; SELECT d.database_id,CONVERT(varchar(36),r.database_guid) FROM sys.databases d JOIN sys.database_recovery_status r ON d.database_id=r.database_id WHERE d.name=N'EnglishCenter_DEMO_LOCAL';";
                await using var reader = await create.ExecuteReaderAsync();
                await reader.ReadAsync();
                await File.WriteAllTextAsync(receipt, JsonSerializer.Serialize(new { catalog = Catalog, databaseId = reader.GetInt32(0), databaseGuid = reader.GetString(1), phase = "created", generatedAt = DateTime.UtcNow }));
            }
            var options = new DbContextOptionsBuilder<EnglishCenterDbContext>().UseSqlServer(LocalRuntimeReferences.SqlConnection, o => o.UseCompatibilityLevel(160)).Options;
            await using var db = new EnglishCenterDbContext(options);
            await db.Database.MigrateAsync();
            if(await db.Accounts.AnyAsync() || await db.Courses.AnyAsync() || await db.Registrations.AnyAsync()) throw new InvalidOperationException("Catalog is not an empty owned fixture; refusing resume");
            await using var tx = await db.Database.BeginTransactionAsync();
            async Task Gate(string key) => await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource={key},@LockMode='Exclusive',@LockOwner='Transaction',@DbPrincipal='public',@LockTimeout=3000; IF @r<0 THROW 51902,'Demo gate refused',1;");
            await Gate("EC:BANDSET");
            var now = DateTime.UtcNow; var today = DateOnly.FromDateTime(now.AddHours(7));
            var hasher = new CredentialHasher(new PasswordPolicy(OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist"))));
            var roles = new[] { "Student", "Admissions", "AcademicAffairs", "Teacher", "Accountant", "Director", "Admin" };
            var ledger = new List<Credential>(); var people = new Dictionary<string, List<Account>>();
            foreach (var code in roles)
            {
                var role = new Role { RoleCode = code, RoleName = code }; db.Roles.Add(role); await db.SaveChangesAsync();
                var count = code == "Student" ? 50 : code == "Teacher" ? 8 : code == "Director" ? 1 : 2;
                people[code] = [];
                for (var i = 1; i <= count; i++)
                {
                    var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
                    var temporary = code != "Student";
                    var email = $"{code.ToLowerInvariant()}{i:00}@demo.example";
                    var a = new Account { RoleId = role.RoleId, Email = email, FullName = $"DEMO {code} {i:00}", PasswordHash = hasher.Hash(password), Status = code == "Student" && i == 49 ? "LOCKED" : code == "Student" && i == 50 ? "DISABLED" : "ACTIVE", CreatedAt = now.AddDays(-90), SecurityVersion = 1, MustChangePassword = temporary, TempIssuedAt = temporary ? now : null, TempExpiresAt = temporary ? now.AddHours(24) : null };
                    if(code=="Student")a.Phone=$"090000{i:0000}";
                    db.Accounts.Add(a); people[code].Add(a);
                    ledger.Add(new(email, password, code, i >= 49 ? a.Status : "Dữ liệu mẫu; xem hướng dẫn kịch bản", temporary));
                }
                await db.SaveChangesAsync();
            }
            var profiles = people["Student"].Select((a,i) => new StudentProfile { AccountId = a.AccountId, DateOfBirth = new DateOnly(2000 + i % 8, 1 + i % 12, 10), Address = $"Địa chỉ giả lập số {i+1}, TP. Hồ Chí Minh" }).ToArray();
            db.StudentProfiles.AddRange(profiles);
            var courses = Enumerable.Range(1, 6).Select(i => new Course { CourseCode = $"DEMO{i:00}", CourseName = new[] { "Tiếng Anh giao tiếp", "IELTS nền tảng", "IELTS chuyên sâu", "TOEIC công sở", "Tiếng Anh học thuật", "Tiếng Anh thiếu niên" }[i-1], BasePrice = 2500000m + i*500000m, Status = "ACTIVE" }).ToArray();
            db.Courses.AddRange(courses);
            var bands = new[] { 0m,25m,45m,65m,85m }.Select((x,i) => new Band { BandCode = $"B{i+1}", MinScore = x }).ToArray(); db.Bands.AddRange(bands); await db.SaveChangesAsync();
            var programs = Enumerable.Range(0,12).Select(i => new TrainingProgram { CourseId = courses[i%6].CourseId, BandId = bands[i/6].BandId, ProgramName = $"DEMO {courses[i%6].CourseName} - {bands[i/6].BandCode}", Status = "DRAFT", ExpectedLessonCount = 3 }).ToArray();
            db.TrainingPrograms.AddRange(programs); await db.SaveChangesAsync();
            foreach (var p in programs) await Gate($"EC:PROGRAM:{p.ProgramId}");
            var components = new Dictionary<int, ScoreComponentConfig[]>();
            foreach (var p in programs)
            {
                var cc = new[] { new ScoreComponentConfig { ProgramId=p.ProgramId, ComponentName="Tham gia học tập", Kind="GENERAL", Weight=20 }, new ScoreComponentConfig { ProgramId=p.ProgramId, ComponentName="Bài kiểm tra trên lớp", Kind="LESSON", Weight=30 }, new ScoreComponentConfig { ProgramId=p.ProgramId, ComponentName="Thi cuối khóa", Kind="FINAL_EXAM", Weight=50 } };
                components[p.ProgramId]=cc; db.ScoreComponentConfigs.AddRange(cc);
            }
            await db.SaveChangesAsync(); foreach(var p in programs) p.Status="ACTIVE"; await db.SaveChangesAsync();
            var classes = programs.Select((p,i) => new Class { ProgramId=p.ProgramId, TeacherId=people["Teacher"][i%8].AccountId, ClassCode=$"DEMO-{i+1:00}", Status=i<2?"PLANNED":i<4?"OPEN":"ONGOING", MaxSize=15, CreatedAt=now.AddDays(-60) }).ToArray();
            db.Classes.AddRange(classes); await db.SaveChangesAsync();
            var schedules = new Dictionary<int,Schedule[]>();
            for(var i=0;i<12;i++)
            {
                await Gate($"EC:CLASS:{classes[i].ClassId}");
                // Different dates per class avoid teacher/room/student timetable collisions.
                var start = i<4 ? today.AddDays(14+i*5) : i<8 ? today.AddDays(-2+(i-4)*5) : today.AddDays(-50+(i-8)*5);
                var ss=Enumerable.Range(0,4).Select(j=>new Schedule {ClassId=classes[i].ClassId, SessionType=j==3?"FINAL_EXAM":"LESSON", ObligationKey=Guid.NewGuid(), IsCurrent=true, SessionDate=start.AddDays(j), StartTime=new TimeOnly(18,0),EndTime=new TimeOnly(20,0), Room=$"D{i+1:00}",OccurrenceState=i>=8 || (i==4&&j==0)?"HELD":"PLANNED",HeldBy=i>=8 || (i==4&&j==0)?classes[i].TeacherId:null,HeldAt=i>=8?now.AddDays(-20):i==4&&j==0?now.AddDays(-1):null }).ToArray();
                schedules[classes[i].ClassId]=ss; db.Schedules.AddRange(ss);
            }
            var sessions=Enumerable.Range(0,12).Select(i=>new EntryTestSession {StartAt=DateTime.SpecifyKind(today.ToDateTime(new TimeOnly(3,0)).AddDays(i<6?-70+i:2+i),DateTimeKind.Utc),EndAt=DateTime.SpecifyKind(today.ToDateTime(new TimeOnly(4,0)).AddDays(i<6?-70+i:2+i),DateTimeKind.Utc),Room=$"TEST{i+1:00}",Capacity=20,GraderId=people["Teacher"][i%8].AccountId,Status=i<6?"CLOSED":"OPEN"}).ToArray();
            db.EntryTestSessions.AddRange(sessions); await db.SaveChangesAsync();
            var invoiceCount=0;
            for(var i=0;i<70;i++)
            {
                var scenario=i<48?i/4: i<50?0: (i-50)%12;
                var ci=scenario; var courseIndex=i<50?ci%6:(((i%50)/4)%6+1)%6;
                // First 48 students: four per class/scenario; extra registrations use another course.
                var student=profiles[i%50];
                var state=scenario==0?"PENDING":scenario==1?"TESTING":scenario==2?"BAND_ASSIGNED":scenario==3?"CONFIRMED":"ADMITTED";
                var r=new Registration {StudentId=student.StudentId,CourseId=courses[courseIndex].CourseId,Status=state,CreatedAt=now.AddDays(-80),AdmissionConfirmedAt=scenario>=3?now.AddDays(-65):null,AdmissionApprovedBy=scenario>=4?people["Admissions"][0].AccountId:null,AdmissionApprovedAt=scenario>=4?now.AddDays(-64):null};
                db.Registrations.Add(r); await db.SaveChangesAsync();
                if(scenario>=1)
                {
                    var attempt=new EntryTestAttempt {RegistrationId=r.RegistrationId,AttemptNumber=1}; db.EntryTestAttempts.Add(attempt); await db.SaveChangesAsync();
                    var booking=new EntryTestBooking {AttemptId=attempt.AttemptId,SessionId=sessions[scenario==1?6:ci%6].SessionId,Status=scenario==1?"SCHEDULED":"COMPLETED",BookedAt=now.AddDays(-75)}; db.EntryTestBookings.Add(booking); await db.SaveChangesAsync();
                    if(scenario>=2)
                    {
                        attempt.Score=ci<6?20:35;attempt.GradedBy=sessions[ci%6].GraderId;attempt.GradedAt=now.AddDays(-68);attempt.CompletedBookingId=booking.BookingId;
                        db.BandAssignments.Add(new BandAssignment {RegistrationId=r.RegistrationId,BestAttemptId=attempt.AttemptId,BandId=bands[ci/6].BandId,AssignedAt=now.AddDays(-68)}); await db.SaveChangesAsync();
                    }
                }
                if(scenario>=4)
                {
                    var paid=scenario>=6; var inv=new Invoice {RegistrationId=r.RegistrationId,InvoiceNumber=$"HD-{now.Year}-{++invoiceCount:00000000}",Amount=courses[courseIndex].BasePrice,IssuedAt=now.AddDays(-64),DueDate=scenario==4?today.AddDays(-5):scenario==5?today.AddDays(5):today.AddDays(-57),Status=paid?"PAID":"UNPAID"}; db.Invoices.Add(inv); await db.SaveChangesAsync();
                    if(paid) {db.Payments.Add(new Payment {InvoiceId=inv.InvoiceId,AmountPaid=inv.Amount,Method=i%2==0?"CASH":"TRANSFER",TransactionReference=i%2==0?null:$"DEMO-{i:000}",ConfirmedBy=people["Accountant"][0].AccountId,PaidAt=now.AddDays(-60)});await db.SaveChangesAsync();}
                    // Extra registrations remain waiting placement; their course differs deliberately.
                    if(paid && i<48)
                    {
                        var c=classes[ci]; var assignment=new StudentClassAssignment {ClassId=c.ClassId,RegistrationId=r.RegistrationId,AssignedBy=people["AcademicAffairs"][0].AccountId,AssignedAt=now.AddDays(-55)};db.StudentClassAssignments.Add(assignment);await db.SaveChangesAsync();
                        if(ci<8)db.GradeReports.Add(new GradeReport{ClassAssignmentId=assignment.ClassAssignmentId,Status="DRAFT"});
                        var ss=schedules[c.ClassId];
                        foreach(var s in ss.Where(x=>x.SessionType=="LESSON"&&x.OccurrenceState=="HELD")) db.Attendances.Add(new Attendance {ClassAssignmentId=assignment.ClassAssignmentId,ScheduleId=s.ScheduleId,Present=i%4!=3,EnteredBy=c.TeacherId,EnteredAt=now.AddDays(-19)});
                        if(ci>=8)
                        {
                            var value=i%4==3?35m:75m+i%4*5;
                            foreach(var comp in components[c.ProgramId])db.Scores.Add(new Score {ClassAssignmentId=assignment.ClassAssignmentId,ComponentId=comp.ComponentId,ScheduleId=comp.Kind=="GENERAL"?null:comp.Kind=="LESSON"?ss[1].ScheduleId:ss[3].ScheduleId,ScoreValue=value,EnteredBy=c.TeacherId,EnteredAt=now.AddDays(-19)});
                            await db.SaveChangesAsync();
                            var status=ci==8?"DRAFT":ci==9?"RETURNED":ci==10?"SUBMITTED":"FINALIZED";
                            var reviewed=status is "RETURNED" or "FINALIZED";
                            db.GradeReports.Add(new GradeReport {ClassAssignmentId=assignment.ClassAssignmentId,Status=status,TeacherComment="Dữ liệu lịch sử giả lập phục vụ trải nghiệm demo",SubmittedBy=status!="DRAFT"?c.TeacherId:null,SubmittedAt=status!="DRAFT"?now.AddDays(-18):null,SubmittedOperationId=status!="DRAFT"?Guid.NewGuid():null,ReviewedBy=reviewed?people["AcademicAffairs"][0].AccountId:null,ReviewedAt=reviewed?now.AddDays(-17):null,ReviewOperationId=reviewed?Guid.NewGuid():null,ReturnedReason=status=="RETURNED"?"DEMO: bổ sung nhận xét học viên":null,FinalScore=status=="FINALIZED"?value:null,FinalizedAt=status=="FINALIZED"?now.AddDays(-17):null});
                        }
                        await db.SaveChangesAsync();
                    }
                }
            }
            // Attendance is frozen after completion, so complete only after inserting historical marks.
            foreach(var c in classes.Skip(8))c.Status="COMPLETED";
            db.InvoiceNumberCounters.Add(new InvoiceNumberCounter {IssueYear=(short)now.Year,LastValue=invoiceCount}); await db.SaveChangesAsync();
            var machine=Environment.MachineName.Replace("]", "]]");
            await using var grant = db.Database.GetDbConnection().CreateCommand(); grant.Transaction = tx.GetDbTransaction(); grant.CommandText = $"CREATE USER M0RuntimeUser FOR LOGIN [{machine}\\EcM0Runtime]; CREATE USER M0MaintUser FOR LOGIN [{machine}\\EcM0Maint]; CREATE USER M0SchemaUser FOR LOGIN [{machine}\\EcM0Schema]; ALTER ROLE ec_app_runtime ADD MEMBER M0RuntimeUser; GRANT CONNECT TO M0RuntimeUser; GRANT VIEW DEFINITION TO M0RuntimeUser; GRANT SELECT ON dbo.__EFMigrationsHistory(MigrationId,ProductVersion) TO M0RuntimeUser; GRANT CONNECT TO M0MaintUser; GRANT DELETE ON dbo.AuditLog TO M0MaintUser; GRANT DELETE ON dbo.ReplayOperation TO M0MaintUser; GRANT SELECT ON dbo.AuditLog(audit_category,security_expires_at,ip_expires_at) TO M0MaintUser; GRANT SELECT ON dbo.ReplayOperation(expires_at) TO M0MaintUser; GRANT UPDATE ON dbo.AuditLog(ip_address,ip_expires_at) TO M0MaintUser; ALTER ROLE db_owner ADD MEMBER M0SchemaUser;"; await grant.ExecuteNonQueryAsync();
            await DemoScenarios.Populate(db,now);
            // Persist encrypted credentials before COMMIT; failure never commits unreachable accounts.
            var clear=JsonSerializer.SerializeToUtf8Bytes(ledger);
            try {await File.WriteAllBytesAsync(Path.Combine(Root,"accounts.dpapi"),ProtectedData.Protect(clear,null,DataProtectionScope.CurrentUser));}finally{CryptographicOperations.ZeroMemory(clear);}
            await tx.CommitAsync();
            await File.WriteAllTextAsync(Path.Combine(Root,"scenarios.json"),JsonSerializer.Serialize(new {success=true,synthetic=true,appliedAt=now}));
            await File.WriteAllTextAsync(Path.Combine(Root,"seed.json"),JsonSerializer.Serialize(new {generatedAt=now,accounts=67,students=50,teachers=8,courses=6,programs=12,classes=12,entrySessions=12,registrations=70,invoiceCount,history="Synthetic operator fixture; not historical audit events",phase="committed"}));
            Console.WriteLine("Demo seed committed: 67 accounts, 6 courses, 12 programs/classes/test sessions, 70 registrations."); return 0;
        }
        catch(Exception e) {Console.Error.WriteLine($"Demo seed refused: {e.GetType().Name}: {e.Message}; {e.InnerException?.Message}");return 1;}
    }
}

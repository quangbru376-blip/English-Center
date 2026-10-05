namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Domain.Admissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class AdmissionsMappingTests
{
    // Expected signatures transcribed independently from accepted T09–T13.
    private static readonly Dictionary<string, string> Signatures = new()
    {
        ["Registration"] = "registration_id:int student_id:int course_id:int status:varchar(16) is_unfinished:bit created_at:datetime2(3) admission_confirmed_at:datetime2(3)? admission_approved_by:int? admission_approved_at:datetime2(3)? rejected_by:int? rejected_at:datetime2(3)? rejection_reason:nvarchar(2000)? rejected_from_status:varchar(16)? rejection_operation_id:uniqueidentifier? cancelled_by:int? cancelled_at:datetime2(3)? cancellation_reason:nvarchar(2000)? cancelled_from_status:varchar(16)? cancellation_operation_id:uniqueidentifier? row_version:rowversion",
        ["EntryTestSession"] = "session_id:int start_at:datetime2(3) end_at:datetime2(3) room:varchar(30) capacity:int grader_id:int status:varchar(16) cancelled_by:int? cancelled_at:datetime2(3)? cancellation_reason:nvarchar(2000)? cancellation_operation_id:uniqueidentifier? row_version:rowversion",
        ["EntryTestAttempt"] = "attempt_id:int registration_id:int attempt_number:int score:decimal(5,2)? graded_by:int? graded_at:datetime2(3)? completed_booking_id:int? row_version:rowversion",
        ["EntryTestBooking"] = "booking_id:int attempt_id:int session_id:int status:varchar(16) booked_at:datetime2(3) absent_by:int? absent_at:datetime2(3)? absent_operation_id:uniqueidentifier? cancelled_by:int? cancelled_at:datetime2(3)? cancellation_reason:nvarchar(2000)? cancelled_from_status:varchar(16)? cancellation_operation_id:uniqueidentifier? row_version:rowversion",
        ["BandAssignment"] = "band_assignment_id:int registration_id:int best_attempt_id:int band_id:int assigned_at:datetime2(3) row_version:rowversion"
    };

    [Theory]
    [InlineData("Registration","registration_id")]
    [InlineData("EntryTestSession","session_id")]
    [InlineData("EntryTestAttempt","attempt_id")]
    [InlineData("EntryTestBooking","booking_id")]
    [InlineData("BandAssignment","band_assignment_id")]
    public void ColumnsIdentityRowversionAndNulls_MatchExactDictionary(string table,string id)
    {
        using var context=RoleMappingTests.Create();
        var entity=Entity(context.GetService<IDesignTimeModel>().Model,table);
        var actual=entity.GetProperties().ToDictionary(p=>p.GetColumnName());
        var expected=Signatures[table].Split(' ').Select(x=>x.Split(':')).ToDictionary(x=>x[0],x=>x[1]);
        Assert.Equal(expected.Keys.Order(),actual.Keys.Order());
        Assert.Equal("dbo",entity.GetSchema());
        foreach(var (column,type) in expected)
        {
            var property=actual[column];
            Assert.False(property.IsShadowProperty());
            Assert.Equal(type.EndsWith('?'),property.IsNullable);
            var sql=type.TrimEnd('?');
            Assert.Equal(sql,property.GetColumnType());
            if(sql.StartsWith("varchar",StringComparison.Ordinal)) Assert.False(property.IsUnicode());
            if(sql.StartsWith("nvarchar",StringComparison.Ordinal)) Assert.True(property.IsUnicode());
            if(sql.StartsWith("datetime2",StringComparison.Ordinal))
            {Assert.Equal(3,property.GetPrecision());Assert.Equal(type.EndsWith('?')?typeof(DateTime?):typeof(DateTime),property.ClrType);}
            if(sql=="uniqueidentifier") Assert.Equal(typeof(Guid?),property.ClrType);
            if(sql.Contains("char",StringComparison.Ordinal)) Assert.Equal(int.Parse(sql.Split('(')[1].TrimEnd(')')),property.GetMaxLength());
        }
        var key=Assert.Single(entity.FindPrimaryKey()!.Properties);
        Assert.Equal(id,key.GetColumnName());
        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn,key.GetValueGenerationStrategy());
        Assert.Equal(1L,key.GetIdentitySeed());Assert.Equal(1,key.GetIdentityIncrement());
        Assert.Equal(ValueGenerated.OnAdd,key.ValueGenerated);
        var rv=actual["row_version"];
        Assert.Equal(typeof(byte[]),rv.ClrType);Assert.True(rv.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate,rv.ValueGenerated);
        Assert.Equal(PropertySaveBehavior.Ignore,rv.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Ignore,rv.GetAfterSaveBehavior());
        Assert.Empty(entity.GetSeedData());
    }

    [Fact]
    public void Defaults_AreOnlyInitialStatesUnfinishedAndServerEventTimes()
    {
        using var context=RoleMappingTests.Create();
        var model=context.GetService<IDesignTimeModel>().Model;
        var allowed=new Dictionary<string,string[]>
        {
            ["Registration"]=["status","is_unfinished","created_at"],
            ["EntryTestSession"]=["status"], ["EntryTestAttempt"]=[],
            ["EntryTestBooking"]=["status","booked_at"], ["BandAssignment"]=["assigned_at"]
        };
        foreach(var table in Signatures.Keys)
        {
            var entity=Entity(model,table);
            var generatedDefaults=entity.GetProperties().Where(p=>p.FindAnnotation(RelationalAnnotationNames.DefaultValue)!=null || p.GetDefaultValueSql()!=null).Select(p=>p.GetColumnName());
            Assert.Equal(allowed[table].Order(),generatedDefaults.Order());
        }
        foreach(var (table,status) in new[]{("Registration","PENDING"),("EntryTestSession","CLOSED"),("EntryTestBooking","SCHEDULED")})
        {
            var property=Column(Entity(model,table),"status");
            Assert.Equal(status,property.GetDefaultValue());Assert.Equal(string.Empty,property.Sentinel);
        }
        var unfinished=Column(Entity(model,"Registration"),"is_unfinished");
        Assert.Equal(true,unfinished.GetDefaultValue());Assert.Equal(true,unfinished.Sentinel);
        Assert.True(new Registration().IsUnfinished);
        foreach(var (table,column) in new[]{("Registration","created_at"),("EntryTestBooking","booked_at"),("BandAssignment","assigned_at")})
            Assert.Equal("SYSUTCDATETIME()",Column(Entity(model,table),column).GetDefaultValueSql());
        var score=Column(Entity(model,"EntryTestAttempt"),"score");
        Assert.Equal(typeof(decimal?),score.ClrType);Assert.Equal(5,score.GetPrecision());Assert.Equal(2,score.GetScale());
        Assert.Null(new EntryTestAttempt().Score);Assert.Null(new EntryTestAttempt().CompletedBookingId);
    }

    [Fact]
    public void Indexes_KeepConditionalHistoryUniquenessAndGuardQueryCoverage()
    {
        using var context=RoleMappingTests.Create();
        var model=context.GetService<IDesignTimeModel>().Model;
        var registration=Entity(model,"Registration");
        Index(registration,"UQ_Registration_Unfinished",true,"[is_unfinished] = 1","student_id","course_id");
        Index(registration,"IX_Registration_StudentCreated",false,null,"student_id","created_at","registration_id");
        Index(registration,"IX_Registration_CourseStateCreated",false,null,"course_id","status","created_at","registration_id");
        Index(registration,"IX_Registration_ApprovedBy",false,null,"admission_approved_by");
        Index(registration,"IX_Registration_RejectedBy",false,null,"rejected_by");
        Index(registration,"IX_Registration_CancelledBy",false,null,"cancelled_by");
        Assert.Equal(6,registration.GetIndexes().Count());
        var session=Entity(model,"EntryTestSession");
        Index(session,"IX_Session_GraderStart",false,null,"grader_id","start_at","end_at","session_id");
        Index(session,"IX_Session_RoomStart",false,null,"room","start_at","end_at","session_id");
        Index(session,"IX_Session_CancelledBy",false,null,"cancelled_by");Assert.Equal(3,session.GetIndexes().Count());
        var attempt=Entity(model,"EntryTestAttempt");
        Index(attempt,"UQ_Attempt_RegistrationNumber",true,null,"registration_id","attempt_number");
        Index(attempt,"UQ_Attempt_CompletedBooking",true,"[completed_booking_id] IS NOT NULL","completed_booking_id");
        Index(attempt,"IX_Attempt_GradedBy",false,null,"graded_by");Assert.Equal(3,attempt.GetIndexes().Count());
        var booking=Entity(model,"EntryTestBooking");
        Index(booking,"UQ_Booking_Current",true,"[status] = 'SCHEDULED'","attempt_id");
        Index(booking,"UQ_Booking_Completed",true,"[status] = 'COMPLETED'","attempt_id");
        var history=Index(booking,"IX_Booking_AttemptHistory",false,null,"attempt_id","booked_at","booking_id");
        Assert.Equal(new[]{false,true,true},history.IsDescending);
        Index(booking,"IX_Booking_SessionState",false,null,"session_id","status","booking_id");
        Index(booking,"IX_Booking_AbsentBy",false,null,"absent_by");Index(booking,"IX_Booking_CancelledBy",false,null,"cancelled_by");
        Assert.Equal(6,booking.GetIndexes().Count());
        var band=Entity(model,"BandAssignment");
        Index(band,"UQ_BandAssignment_Registration",true,null,"registration_id");
        Index(band,"IX_BandAssignment_Band",false,null,"band_id");
        Index(band,"IX_BandAssignment_BestAttempt",false,null,"best_attempt_id","registration_id");Assert.Equal(3,band.GetIndexes().Count());
    }

    [Fact]
    public void ForeignKeysAndAlternateKeys_KeepCompositeOwnershipAndOptionalCycle()
    {
        using var context=RoleMappingTests.Create();
        var model=context.GetService<IDesignTimeModel>().Model;
        var r=Entity(model,"Registration");
        Foreign(r,"FK_Registration_StudentProfile_student_id","StudentProfile",true,["student_id"],["student_id"]);
        Foreign(r,"FK_Registration_Course_course_id","Course",true,["course_id"],["course_id"]);
        foreach(var actor in new[]{"admission_approved_by","rejected_by","cancelled_by"}) Foreign(r,"FK_Registration_Account_"+actor,"Account",false,[actor],["account_id"]);
        var s=Entity(model,"EntryTestSession");
        Foreign(s,"FK_EntryTestSession_Account_grader_id","Account",true,["grader_id"],["account_id"]);
        Foreign(s,"FK_EntryTestSession_Account_cancelled_by","Account",false,["cancelled_by"],["account_id"]);
        var a=Entity(model,"EntryTestAttempt");
        Foreign(a,"FK_Attempt_Registration_registration_id","Registration",true,["registration_id"],["registration_id"]);
        Foreign(a,"FK_Attempt_Account_graded_by","Account",false,["graded_by"],["account_id"]);
        var result=Foreign(a,"FK_Attempt_Booking_CompletedBookingAttempt","EntryTestBooking",false,["completed_booking_id","attempt_id"],["booking_id","attempt_id"]);
        Assert.False(result.IsUnique);
        var b=Entity(model,"EntryTestBooking");
        Foreign(b,"FK_Booking_Attempt_attempt_id","EntryTestAttempt",true,["attempt_id"],["attempt_id"]);
        Foreign(b,"FK_Booking_Session_session_id","EntryTestSession",true,["session_id"],["session_id"]);
        Foreign(b,"FK_Booking_Account_absent_by","Account",false,["absent_by"],["account_id"]);
        Foreign(b,"FK_Booking_Account_cancelled_by","Account",false,["cancelled_by"],["account_id"]);
        var band=Entity(model,"BandAssignment");
        Assert.True(Foreign(band,"FK_BandAssignment_Registration_registration_id","Registration",true,["registration_id"],["registration_id"]).IsUnique);
        Foreign(band,"FK_BandAssignment_Attempt_BestAttemptRegistration","EntryTestAttempt",true,["best_attempt_id","registration_id"],["attempt_id","registration_id"]);
        Foreign(band,"FK_BandAssignment_Band_band_id","Band",true,["band_id"],["band_id"]);
        Alternate(a,"AK_Attempt_IdRegistration","attempt_id","registration_id");
        Alternate(b,"AK_Booking_IdAttempt","booking_id","attempt_id");
        Assert.Equal(17,model.GetEntityTypes().Where(e=>Signatures.ContainsKey(e.GetTableName()!)).Sum(e=>e.GetForeignKeys().Count()));
        Assert.Equal(2,model.GetEntityTypes().Sum(e=>e.GetKeys().Count(k=>!k.IsPrimaryKey())));
    }

    [Theory]
    [InlineData("Registration","CK_Registration_RejectedBundle","REJECTED","rejected_by,rejected_at,rejection_reason,rejected_from_status,rejection_operation_id")]
    [InlineData("Registration","CK_Registration_CancelledBundle","CANCELLED","cancelled_by,cancelled_at,cancelled_from_status,cancellation_operation_id")]
    [InlineData("EntryTestSession","CK_EntryTestSession_CancelledBundle","CANCELLED","cancelled_by,cancelled_at,cancellation_reason,cancellation_operation_id")]
    [InlineData("EntryTestBooking","CK_EntryTestBooking_AbsenceBundle","ABSENT","absent_by,absent_at,absent_operation_id")]
    [InlineData("EntryTestBooking","CK_EntryTestBooking_CancellationBundle","CANCELLED","cancelled_by,cancelled_at,cancelled_from_status,cancellation_operation_id")]
    public void EventBundles_ExplicitlyRejectPartialNullMembers(string table,string name,string state,string required)
    {
        using var context=RoleMappingTests.Create();
        var sql=Check(context.GetService<IDesignTimeModel>().Model,table,name);
        Assert.Contains("[status] = '"+state+"'",sql);
        Assert.Contains("[status] <> '"+state+"'",sql);Assert.Contains(" OR ",sql);
        foreach(var member in required.Split(',')) {Assert.Contains("["+member+"] IS NOT NULL",sql);Assert.Contains("["+member+"] IS NULL",sql);}
        if(name.Contains("Rejected",StringComparison.Ordinal))
        {
            Assert.Contains("LEN([rejection_reason]) > 0",sql);Assert.Contains("'CONFIRMED'",sql);
            Assert.Contains("DATALENGTH([rejected_from_status]) = LEN([rejected_from_status])",sql);
            Assert.Contains("COLLATE Latin1_General_100_BIN2",sql);
        }
        if(name.Contains("Cancel",StringComparison.Ordinal))
        {
            Assert.Contains("[cancellation_reason] IS NULL",sql);
            if(table=="EntryTestSession") Assert.Contains("LEN([cancellation_reason]) > 0",sql);
            else Assert.Contains("DATALENGTH([cancelled_from_status]) = LEN([cancelled_from_status])",sql);
        }
    }

    [Fact]
    public void ResultAndApprovalBundles_DistinguishNoResultFromZero()
    {
        using var context=RoleMappingTests.Create();
        var model=context.GetService<IDesignTimeModel>().Model;
        var result=Check(model,"EntryTestAttempt","CK_EntryTestAttempt_ResultBundle");
        foreach(var column in new[]{"score","graded_by","graded_at","completed_booking_id"})
        {Assert.Contains("["+column+"] IS NULL",result);Assert.Contains("["+column+"] IS NOT NULL",result);}
        Assert.Contains(" OR ",result);
        var approved=Check(model,"Registration","CK_Registration_ApprovedBundle");
        foreach(var column in new[]{"admission_approved_by","admission_approved_at"})
        {Assert.Contains("["+column+"] IS NULL",approved);Assert.Contains("["+column+"] IS NOT NULL",approved);}
        var range=Check(model,"EntryTestAttempt","CK_EntryTestAttempt_Score");
        Assert.Contains("[score] IS NULL",range);Assert.Contains("[score] >= 0",range);Assert.Contains("[score] <= 100",range);
        Assert.Contains("[attempt_number] IN (1,2)",Check(model,"EntryTestAttempt","CK_EntryTestAttempt_Number").Replace("1, 2","1,2"));
    }

    [Fact]
    public void EnumsAndSessionChecks_PreserveExactLiteralsCapacityMinutesVietnamDateAndRoom()
    {
        using var context=RoleMappingTests.Create();var model=context.GetService<IDesignTimeModel>().Model;
        foreach(var (table,literals) in new[]{("Registration",new[]{"PENDING","TESTING","BAND_ASSIGNED","CONFIRMED","ADMITTED","REJECTED","CANCELLED"}),("EntryTestSession",new[]{"OPEN","CLOSED","CANCELLED"}),("EntryTestBooking",new[]{"SCHEDULED","COMPLETED","ABSENT","CANCELLED"})})
        {
            var sql=Check(model,table,"CK_"+table+"_Status");
            foreach(var literal in literals) Assert.Contains("'"+literal+"'",sql);
            Assert.Equal(literals.Length,sql.Count(c=>c=='\'')/2);
            Assert.Contains("COLLATE Latin1_General_100_BIN2",sql);Assert.Contains("DATALENGTH([status]) = LEN([status])",sql);
        }
        Assert.Contains("[capacity] BETWEEN 1 AND 20",Check(model,"EntryTestSession","CK_EntryTestSession_Capacity"));
        var time=Check(model,"EntryTestSession","CK_EntryTestSession_Time");
        Assert.Contains("[start_at] < [end_at]",time);
        foreach(var column in new[]{"start_at","end_at"})
        {
            Assert.Contains("DATEPART(second, ["+column+"]) = 0",time);
            Assert.Contains("DATEPART(millisecond, ["+column+"]) = 0",time);
            Assert.Contains("CAST(DATEADD(minute, 420, ["+column+"]) AS date)",time);
        }
        var room=Check(model,"EntryTestSession","CK_EntryTestSession_Room");
        Assert.Contains("DATALENGTH([room]) BETWEEN 1 AND 30",room);
        Assert.Contains("DATALENGTH([room]) = LEN([room])",room);
        Assert.Contains("COLLATE Latin1_General_100_BIN2",room);Assert.Contains("NOT LIKE '%[^A-Z0-9_-]%'",room);
    }

    [Fact]
    public void CompleteModelAndDdl_KeepThirteenTablesCompositeCycleAndNoDatabaseIo()
    {
        using var context=RoleMappingTests.Create();var model=context.GetService<IDesignTimeModel>().Model;
        Assert.Equal(new[]{"Account","AuthSession","Band","BandAssignment","Course","EntryTestAttempt","EntryTestBooking","EntryTestSession","Registration","Role","ScoreComponentConfig","StudentProfile","TrainingProgram"},model.GetEntityTypes().Select(e=>e.GetTableName()).Order());
        Assert.Equal(116,model.GetEntityTypes().Sum(e=>e.GetProperties().Count()));
        Assert.Equal(37,model.GetEntityTypes().Sum(e=>e.GetIndexes().Count()));
        Assert.Equal(34,model.GetEntityTypes().Sum(e=>e.GetCheckConstraints().Count()));
        Assert.Equal(23,model.GetEntityTypes().Sum(e=>e.GetForeignKeys().Count()));
        Assert.All(model.GetEntityTypes().SelectMany(e=>e.GetForeignKeys()),f=>Assert.Equal(DeleteBehavior.NoAction,f.DeleteBehavior));
        var ddl=context.Database.GenerateCreateScript();
        Assert.Equal(13,ddl.Split("CREATE TABLE",StringSplitOptions.None).Length-1);
        Assert.Equal(12,ddl.Split("rowversion NOT NULL",StringSplitOptions.None).Length-1);
        foreach(var table in Signatures.Keys)Assert.Contains("CREATE TABLE [dbo].["+table+"]",ddl);
        Assert.Contains("FOREIGN KEY ([completed_booking_id], [attempt_id]) REFERENCES [dbo].[EntryTestBooking] ([booking_id], [attempt_id])",ddl);
        Assert.Contains("FOREIGN KEY ([best_attempt_id], [registration_id]) REFERENCES [dbo].[EntryTestAttempt] ([attempt_id], [registration_id])",ddl);
        Assert.Contains("CREATE UNIQUE INDEX [UQ_Booking_Current]",ddl);Assert.Contains("WHERE [status] = 'SCHEDULED'",ddl);
        Assert.Contains("CREATE UNIQUE INDEX [UQ_Booking_Completed]",ddl);Assert.Contains("WHERE [status] = 'COMPLETED'",ddl);
        Assert.Contains("WHERE [is_unfinished] = 1",ddl);
        Assert.DoesNotContain("ON DELETE CASCADE",ddl);Assert.DoesNotContain("__EFMigrationsHistory",ddl);Assert.DoesNotContain("CREATE TRIGGER",ddl);
        Assert.Equal(System.Data.ConnectionState.Closed,context.Database.GetDbConnection().State);
        Assert.False(context.Database.CreateExecutionStrategy().RetriesOnFailure);
    }

    [Fact]
    public void EveryForeignKey_HasExplicitLookupCoverageWithoutConventionIndexes()
    {
        using var context=RoleMappingTests.Create();
        var model=context.GetService<IDesignTimeModel>().Model;
        foreach(var entity in model.GetEntityTypes())
        foreach(var fk in entity.GetForeignKeys())
        {
            var covered=entity.GetIndexes().Any(index =>
                (index.GetFilter()==null && index.Properties.Take(fk.Properties.Count).SequenceEqual(fk.Properties)) ||
                (index.IsUnique && index.Properties.Count==1 && index.Properties[0]==fk.Properties[0] &&
                    index.Properties[0].IsNullable && index.GetFilter()=="["+index.Properties[0].GetColumnName()+"] IS NOT NULL"));
            Assert.True(covered,fk.GetConstraintName());
        }
    }

    private static IEntityType Entity(IModel model,string table)=>model.GetEntityTypes().Single(e=>e.GetTableName()==table);
    private static IProperty Column(IEntityType entity,string name)=>entity.GetProperties().Single(p=>p.GetColumnName()==name);
    private static string Check(IModel model,string table,string name)=>Entity(model,table).GetCheckConstraints().Single(c=>c.Name==name).Sql;
    private static IIndex Index(IEntityType entity,string name,bool unique,string? filter,params string[] columns)
    {
        var index=entity.GetIndexes().Single(i=>i.GetDatabaseName()==name);
        Assert.Equal(unique,index.IsUnique);Assert.Equal(filter,index.GetFilter());Assert.Equal(columns,index.Properties.Select(p=>p.GetColumnName()));return index;
    }
    private static IForeignKey Foreign(IEntityType entity,string name,string target,bool required,string[] columns,string[] keys)
    {
        var fk=entity.GetForeignKeys().Single(f=>f.GetConstraintName()==name);
        Assert.Equal(target,fk.PrincipalEntityType.GetTableName());Assert.Equal(required,fk.IsRequired);
        Assert.Equal(DeleteBehavior.NoAction,fk.DeleteBehavior);
        Assert.Equal(columns,fk.Properties.Select(p=>p.GetColumnName()));Assert.Equal(keys,fk.PrincipalKey.Properties.Select(p=>p.GetColumnName()));return fk;
    }
    private static void Alternate(IEntityType entity,string name,params string[] columns)
    {
        var key=entity.GetKeys().Single(k=>!k.IsPrimaryKey());Assert.Equal(name,key.GetName());Assert.Equal(columns,key.Properties.Select(p=>p.GetColumnName()));
    }
}

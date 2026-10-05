namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Domain.Training;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class TrainingMappingTests
{
    private static readonly Dictionary<string,string> Expected = new()
    {
        ["Class"]="class_id:int program_id:int teacher_id:int class_code:varchar(30) status:varchar(16) max_size:int created_at:datetime2(3) cancelled_by:int? cancelled_at:datetime2(3)? cancellation_reason:nvarchar(2000)? cancellation_operation_id:uniqueidentifier? row_version:rowversion",
        ["StudentClassAssignment"]="class_assignment_id:int class_id:int registration_id:int assigned_by:int assigned_at:datetime2(3)",
        ["Schedule"]="schedule_id:int class_id:int session_type:varchar(16) obligation_key:uniqueidentifier replaces_schedule_id:int? is_current:bit session_date:date start_time:time(0) end_time:time(0) room:varchar(30) occurrence_state:varchar(16) held_by:int? held_at:datetime2(3)? not_held_by:int? not_held_at:datetime2(3)? not_held_reason:nvarchar(2000)? replacement_operation_id:uniqueidentifier? row_version:rowversion"
    };

    [Theory]
    [InlineData("Class","class_id")]
    [InlineData("StudentClassAssignment","class_assignment_id")]
    [InlineData("Schedule","schedule_id")]
    public void EveryColumnNullAndIdentity_MatchAcceptedDictionary(string table,string id)
    {
        using var context=RoleMappingTests.Create();var entity=Entity(context.GetService<IDesignTimeModel>().Model,table);
        var columns=entity.GetProperties().ToDictionary(p=>p.GetColumnName());
        var expected=Expected[table].Split(' ').Select(x=>x.Split(':')).ToDictionary(x=>x[0],x=>x[1]);
        Assert.Equal(expected.Keys.Order(),columns.Keys.Order());Assert.Equal("dbo",entity.GetSchema());
        foreach(var (name,type) in expected)
        {
            var column=columns[name];var sql=type.TrimEnd('?');
            Assert.Equal(sql,column.GetColumnType());Assert.Equal(type.EndsWith('?'),column.IsNullable);Assert.False(column.IsShadowProperty());
            if(sql.StartsWith("varchar",StringComparison.Ordinal))Assert.False(column.IsUnicode());
            if(sql.StartsWith("nvarchar",StringComparison.Ordinal))Assert.True(column.IsUnicode());
            if(sql.Contains("char",StringComparison.Ordinal))Assert.Equal(int.Parse(sql.Split('(')[1].TrimEnd(')')),column.GetMaxLength());
            if(sql=="datetime2(3)")Assert.Equal(3,column.GetPrecision());
            if(sql=="uniqueidentifier")Assert.Equal(type.EndsWith('?')?typeof(Guid?):typeof(Guid),column.ClrType);
            if(sql=="date")Assert.Equal(typeof(DateOnly),column.ClrType);
            if(sql=="time(0)"){Assert.Equal(typeof(TimeOnly),column.ClrType);Assert.Equal(0,column.GetPrecision());}
        }
        var key=Assert.Single(entity.FindPrimaryKey()!.Properties);
        Assert.Equal(id,key.GetColumnName());Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn,key.GetValueGenerationStrategy());
        Assert.Equal(1L,key.GetIdentitySeed());Assert.Equal(1,key.GetIdentityIncrement());Assert.Equal(ValueGenerated.OnAdd,key.ValueGenerated);
        Assert.Single(entity.GetKeys());Assert.Empty(entity.GetSeedData());
    }

    [Fact]
    public void Rowversion_ExistsOnlyForMutableClassAndSchedule()
    {
        using var context=RoleMappingTests.Create();var model=context.GetService<IDesignTimeModel>().Model;
        foreach(var table in new[]{"Class","Schedule"})
        {
            var rv=Column(Entity(model,table),"row_version");
            Assert.True(rv.IsConcurrencyToken);Assert.Equal(ValueGenerated.OnAddOrUpdate,rv.ValueGenerated);
            Assert.Equal(PropertySaveBehavior.Ignore,rv.GetBeforeSaveBehavior());Assert.Equal(PropertySaveBehavior.Ignore,rv.GetAfterSaveBehavior());
        }
        var assignment=Entity(model,"StudentClassAssignment");
        Assert.DoesNotContain(assignment.GetProperties(),p=>p.IsConcurrencyToken);
        Assert.Equal(5,assignment.GetProperties().Count());
    }

    [Fact]
    public void Defaults_DoNotInventDatesTimesRolesSessionTypeOrObligationKey()
    {
        using var context=RoleMappingTests.Create();var model=context.GetService<IDesignTimeModel>().Model;
        var allowed=new Dictionary<string,string[]>{["Class"]=["status","created_at"],["StudentClassAssignment"]=["assigned_at"],["Schedule"]=["occurrence_state","is_current"]};
        foreach(var table in Expected.Keys)
        {
            var defaults=Entity(model,table).GetProperties().Where(p=>p.FindAnnotation(RelationalAnnotationNames.DefaultValue)!=null||p.GetDefaultValueSql()!=null).Select(p=>p.GetColumnName());
            Assert.Equal(allowed[table].Order(),defaults.Order());
        }
        foreach(var (table,name) in new[]{("Class","status"),("Schedule","occurrence_state")})
        {var status=Column(Entity(model,table),name);Assert.Equal("PLANNED",status.GetDefaultValue());Assert.Equal(string.Empty,status.Sentinel);}
        foreach(var (table,name) in new[]{("Class","created_at"),("StudentClassAssignment","assigned_at")})Assert.Equal("SYSUTCDATETIME()",Column(Entity(model,table),name).GetDefaultValueSql());
        var current=Column(Entity(model,"Schedule"),"is_current");Assert.Equal(true,current.GetDefaultValue());Assert.Equal(true,current.Sentinel);
        Assert.True(new Schedule().IsCurrent);
        Assert.Equal(ValueGenerated.Never,Column(Entity(model,"Schedule"),"obligation_key").ValueGenerated);
        Assert.Null(new Schedule().ReplacesScheduleId);Assert.Null(new Schedule().HeldAt);Assert.Null(new Schedule().NotHeldAt);
    }

    [Fact]
    public void Indexes_KeepHistoricalCodesPlacementsAndConditionalScheduleUniqueness()
    {
        using var context=RoleMappingTests.Create();var model=context.GetService<IDesignTimeModel>().Model;
        var c=Entity(model,"Class");Index(c,"UQ_Class_Code",true,null,"class_code");
        Index(c,"IX_Class_ProgramState",false,null,"program_id","status","class_id");Index(c,"IX_Class_TeacherState",false,null,"teacher_id","status","class_id");
        Index(c,"IX_Class_CancelledBy",false,null,"cancelled_by");Assert.Equal(4,c.GetIndexes().Count());
        var a=Entity(model,"StudentClassAssignment");Index(a,"UQ_Assignment_Registration",true,null,"registration_id");
        Index(a,"IX_Assignment_Class",false,null,"class_id","class_assignment_id");Index(a,"IX_Assignment_AssignedBy",false,null,"assigned_by");Assert.Equal(3,a.GetIndexes().Count());
        var s=Entity(model,"Schedule");Index(s,"UQ_Schedule_CurrentObligation",true,"[is_current] = 1","class_id","obligation_key");
        Index(s,"UQ_Schedule_CurrentFinalExam",true,"[is_current] = 1 AND [session_type] = 'FINAL_EXAM'","class_id");
        Index(s,"UQ_Schedule_Predecessor",true,"[replaces_schedule_id] IS NOT NULL","replaces_schedule_id");
        Index(s,"IX_Schedule_ClassCurrentDate",false,null,"class_id","is_current","session_date","start_time","schedule_id");
        Index(s,"IX_Schedule_RoomDate",false,null,"room","session_date","start_time","end_time","schedule_id");
        Index(s,"IX_Schedule_HeldBy",false,null,"held_by");Index(s,"IX_Schedule_NotHeldBy",false,null,"not_held_by");Assert.Equal(7,s.GetIndexes().Count());
        Assert.DoesNotContain(s.GetIndexes(),i=>i.IsUnique && i.GetFilter()==null);
    }

    [Fact]
    public void ForeignKeys_KeepRequiredLinksAndOptionalUniquePredecessorWithoutCascade()
    {
        using var context=RoleMappingTests.Create();var model=context.GetService<IDesignTimeModel>().Model;
        var c=Entity(model,"Class");
        Foreign(c,"FK_Class_TrainingProgram_program_id","TrainingProgram",true,false,"program_id","program_id");
        Foreign(c,"FK_Class_Account_teacher_id","Account",true,false,"teacher_id","account_id");
        Foreign(c,"FK_Class_Account_cancelled_by","Account",false,false,"cancelled_by","account_id");Assert.Equal(3,c.GetForeignKeys().Count());
        var a=Entity(model,"StudentClassAssignment");
        Foreign(a,"FK_Assignment_Class_class_id","Class",true,false,"class_id","class_id");
        Foreign(a,"FK_Assignment_Registration_registration_id","Registration",true,true,"registration_id","registration_id");
        Foreign(a,"FK_Assignment_Account_assigned_by","Account",true,false,"assigned_by","account_id");Assert.Equal(3,a.GetForeignKeys().Count());
        var s=Entity(model,"Schedule");
        Foreign(s,"FK_Schedule_Class_class_id","Class",true,false,"class_id","class_id");
        Foreign(s,"FK_Schedule_Schedule_replaces_schedule_id","Schedule",false,true,"replaces_schedule_id","schedule_id");
        Foreign(s,"FK_Schedule_Account_held_by","Account",false,false,"held_by","account_id");
        Foreign(s,"FK_Schedule_Account_not_held_by","Account",false,false,"not_held_by","account_id");Assert.Equal(4,s.GetForeignKeys().Count());
        Assert.Equal(33,Prior(model).Sum(e=>e.GetForeignKeys().Count()));
    }

    [Fact]
    public void LiteralRangesAndAsciiGuards_KeepAcceptedStatesAndMinutePrecision()
    {
        using var context=RoleMappingTests.Create();var model=context.GetService<IDesignTimeModel>().Model;
        Literal(model,"Class","CK_Class_Status","status","PLANNED","OPEN","ONGOING","COMPLETED","CANCELLED");
        Literal(model,"Schedule","CK_Schedule_Type","session_type","LESSON","FINAL_EXAM");
        Literal(model,"Schedule","CK_Schedule_State","occurrence_state","PLANNED","HELD","NOT_HELD");
        Assert.Contains("[max_size] BETWEEN 1 AND 15",Check(model,"Class","CK_Class_MaxSize"));
        foreach(var (table,name,column) in new[]{("Class","CK_Class_Code","class_code"),("Schedule","CK_Schedule_Room","room")})
        {
            var sql=Check(model,table,name);Assert.Contains("DATALENGTH(["+column+"]) BETWEEN 1 AND 30",sql);
            Assert.Contains("DATALENGTH(["+column+"]) = LEN(["+column+"])",sql);
            Assert.Contains("COLLATE Latin1_General_100_BIN2",sql);Assert.Contains("NOT LIKE '%[^A-Z0-9_-]%'",sql);
        }
        var time=Check(model,"Schedule","CK_Schedule_Time");Assert.Contains("[start_time] < [end_time]",time);
        foreach(var column in new[]{"start_time","end_time"})Assert.Contains("DATEPART(second, ["+column+"]) = 0",time);
    }

    [Theory]
    [InlineData("Class","CK_Class_CancellationBundle","status","CANCELLED","cancelled_by,cancelled_at,cancellation_reason,cancellation_operation_id")]
    [InlineData("Schedule","CK_Schedule_HeldBundle","occurrence_state","HELD","held_by,held_at")]
    [InlineData("Schedule","CK_Schedule_NotHeldBundle","occurrence_state","NOT_HELD","not_held_by,not_held_at,not_held_reason,replacement_operation_id")]
    public void EventBundles_ExplicitlyGuardEveryMandatoryNull(string table,string check,string column,string state,string members)
    {
        using var context=RoleMappingTests.Create();var sql=Check(context.GetService<IDesignTimeModel>().Model,table,check);
        Assert.Contains("["+column+"] = '"+state+"'",sql);Assert.Contains("["+column+"] <> '"+state+"'",sql);Assert.Contains(" OR ",sql);
        foreach(var member in members.Split(',')){Assert.Contains("["+member+"] IS NOT NULL",sql);Assert.Contains("["+member+"] IS NULL",sql);}
        if(table=="Class")Assert.Contains("LEN([cancellation_reason]) > 0",sql);
        if(state=="NOT_HELD")Assert.Contains("LEN([not_held_reason]) > 0",sql);
    }

    [Fact]
    public void CurrentProjectionAndSelfPointer_KeepRootAndHistoryShapes()
    {
        using var context=RoleMappingTests.Create();var model=context.GetService<IDesignTimeModel>().Model;
        var self=Check(model,"Schedule","CK_Schedule_NoSelfReplacement");
        Assert.Contains("[replaces_schedule_id] IS NULL",self);Assert.Contains("[replaces_schedule_id] <> [schedule_id]",self);
        var projection=Check(model,"Schedule","CK_Schedule_CurrentState");
        foreach(var literal in new[]{"PLANNED","HELD","NOT_HELD"})Assert.Contains("'"+literal+"'",projection);
        Assert.Contains("[is_current] = 1",projection);Assert.Contains("[is_current] = 0",projection);Assert.Contains(" OR ",projection);
        Assert.Equal(8,Entity(model,"Schedule").GetCheckConstraints().Count());Assert.Empty(Entity(model,"StudentClassAssignment").GetCheckConstraints());
    }

    [Fact]
    public void D01D04_SubsetRetainsSixteenReviewedTables()
    {
        using var context=RoleMappingTests.Create();var model=context.GetService<IDesignTimeModel>().Model;
        Assert.Equal(new[]{"Account","AuthSession","Band","BandAssignment","Class","Course","EntryTestAttempt","EntryTestBooking","EntryTestSession","Registration","Role","Schedule","ScoreComponentConfig","StudentClassAssignment","StudentProfile","TrainingProgram"},Prior(model).Select(e=>e.GetTableName()).Order());
        Assert.Equal(151,Prior(model).Sum(e=>e.GetProperties().Count()));Assert.Equal(51,Prior(model).Sum(e=>e.GetIndexes().Count()));
        Assert.Equal(46,Prior(model).Sum(e=>e.GetCheckConstraints().Count()));Assert.Equal(33,Prior(model).Sum(e=>e.GetForeignKeys().Count()));
        Assert.Equal(16,Prior(model).Sum(e=>e.GetKeys().Count(k=>k.IsPrimaryKey())));Assert.Equal(2,Prior(model).Sum(e=>e.GetKeys().Count(k=>!k.IsPrimaryKey())));
        var ddl=context.Database.GenerateCreateScript();Assert.Equal(16,Prior(model).Count());
        Assert.Equal(14,Prior(model).SelectMany(e=>e.GetProperties()).Count(p=>p.GetColumnType()=="rowversion"));
        foreach(var table in Expected.Keys)Assert.Contains("CREATE TABLE [dbo].["+table+"]",ddl);
        Assert.Contains("[session_date] date NOT NULL",ddl);Assert.Contains("[start_time] time(0) NOT NULL",ddl);Assert.Contains("[end_time] time(0) NOT NULL",ddl);
        Assert.Contains("CREATE UNIQUE INDEX [UQ_Schedule_CurrentObligation]",ddl);Assert.Contains("WHERE [is_current] = 1",ddl);
        Assert.Contains("WHERE [is_current] = 1 AND [session_type] = 'FINAL_EXAM'",ddl);Assert.Contains("WHERE [replaces_schedule_id] IS NOT NULL",ddl);
        Assert.Contains("REFERENCES [dbo].[Schedule] ([schedule_id])",ddl);
        Assert.DoesNotContain("ON DELETE CASCADE",ddl);Assert.DoesNotContain("__EFMigrationsHistory",ddl);Assert.DoesNotContain("CREATE TRIGGER",ddl);
        Assert.Equal(System.Data.ConnectionState.Closed,context.Database.GetDbConnection().State);Assert.False(context.Database.CreateExecutionStrategy().RetriesOnFailure);
    }

    private static IEnumerable<IEntityType> Prior(IModel model) => model.GetEntityTypes().Where(e=>e.GetTableName() is not ("Attendance" or "Score" or "GradeReport"));
    private static IEntityType Entity(IModel model,string table)=>model.GetEntityTypes().Single(e=>e.GetTableName()==table);
    private static IProperty Column(IEntityType entity,string name)=>entity.GetProperties().Single(p=>p.GetColumnName()==name);
    private static string Check(IModel model,string table,string name)=>Entity(model,table).GetCheckConstraints().Single(c=>c.Name==name).Sql;
    private static void Index(IEntityType entity,string name,bool unique,string? filter,params string[] columns)
    {var index=entity.GetIndexes().Single(i=>i.GetDatabaseName()==name);Assert.Equal(unique,index.IsUnique);Assert.Equal(filter,index.GetFilter());Assert.Equal(columns,index.Properties.Select(p=>p.GetColumnName()));}
    private static void Foreign(IEntityType entity,string name,string target,bool required,bool unique,string column,string principal)
    {var fk=entity.GetForeignKeys().Single(f=>f.GetConstraintName()==name);Assert.Equal(target,fk.PrincipalEntityType.GetTableName());Assert.Equal(required,fk.IsRequired);Assert.Equal(unique,fk.IsUnique);Assert.Equal(DeleteBehavior.NoAction,fk.DeleteBehavior);Assert.Equal(column,Assert.Single(fk.Properties).GetColumnName());Assert.Equal(principal,Assert.Single(fk.PrincipalKey.Properties).GetColumnName());}
    private static void Literal(IModel model,string table,string name,string column,params string[] values)
    {var sql=Check(model,table,name);foreach(var value in values)Assert.Contains("'"+value+"'",sql);Assert.Equal(values.Length,sql.Count(c=>c=='\'')/2);Assert.Contains("COLLATE Latin1_General_100_BIN2",sql);Assert.Contains("DATALENGTH(["+column+"]) = LEN(["+column+"])",sql);}
}

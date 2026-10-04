namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class IdentityMappingTests
{
    private static readonly Dictionary<string,(string Type,bool Nullable)> AccountColumns=new()
    {
        ["account_id"]=("int",false),["role_id"]=("int",false),["email"]=("varchar(254)",false),
        ["phone"]=("varchar(10)",true),["password_hash"]=("varchar(512)",false),["full_name"]=("nvarchar(200)",false),
        ["status"]=("varchar(16)",false),["created_at"]=("datetime2(3)",false),["security_version"]=("bigint",false),
        ["failed_login_count"]=("int",false),["failure_window_started_at"]=("datetime2(3)",true),
        ["login_throttled_until"]=("datetime2(3)",true),["must_change_password"]=("bit",false),
        ["temp_issued_at"]=("datetime2(3)",true),["temp_expires_at"]=("datetime2(3)",true),["row_version"]=("rowversion",false)
    };
    private static readonly Dictionary<string,(string Type,bool Nullable)> StudentColumns=new()
    {
        ["student_id"]=("int",false),["account_id"]=("int",false),["date_of_birth"]=("date",false),
        ["address"]=("nvarchar(510)",true),["row_version"]=("rowversion",false)
    };
    private static readonly Dictionary<string,(string Type,bool Nullable)> SessionColumns=new()
    {
        ["sid"]=("binary(32)",false),["account_id"]=("int",false),["issued_at"]=("datetime2(3)",false),
        ["expires_at"]=("datetime2(3)",false),["last_activity_at"]=("datetime2(3)",false),["revoked_at"]=("datetime2(3)",true),
        ["security_version"]=("bigint",false),["session_scope"]=("varchar(24)",false),["row_version"]=("rowversion",false)
    };
    [Theory]
    [InlineData("Account"),InlineData("StudentProfile"),InlineData("AuthSession")]
    public void EveryColumn_HasExactAcceptedPhysicalContract(string table)
    {
        using var context=RoleMappingTests.Create();
        var entity=context.GetService<IDesignTimeModel>().Model.GetEntityTypes().Single(e=>e.GetTableName()==table);
        Assert.Equal("dbo",entity.GetSchema());
        var expected=table switch{"Account"=>AccountColumns,"StudentProfile"=>StudentColumns,_=>SessionColumns};
        var actual=entity.GetProperties().ToDictionary(p=>p.GetColumnName(),StringComparer.Ordinal);
        Assert.Equal(expected.Keys.Order().ToArray(),actual.Keys.Order().ToArray());
        foreach(var pair in expected)
        {
            Assert.Equal(pair.Value.Type,actual[pair.Key].GetColumnType());
            Assert.Equal(pair.Value.Nullable,actual[pair.Key].IsNullable);
            Assert.False(actual[pair.Key].IsShadowProperty());
        }
        Assert.Empty(entity.GetSeedData());
        var rv=actual["row_version"];
        Assert.True(rv.IsConcurrencyToken);Assert.Equal(ValueGenerated.OnAddOrUpdate,rv.ValueGenerated);
        Assert.Equal(PropertySaveBehavior.Ignore,rv.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Ignore,rv.GetAfterSaveBehavior());
        var key=Assert.Single(entity.FindPrimaryKey()!.Properties);
        Assert.Equal(table=="AuthSession"?"sid":table=="Account"?"account_id":"student_id",key.GetColumnName());
        Assert.Equal(table=="AuthSession"?ValueGenerated.Never:ValueGenerated.OnAdd,key.ValueGenerated);
        if(table=="AuthSession"){Assert.Equal(32,key.GetMaxLength());Assert.True(key.IsFixedLength());}
        if(table=="StudentProfile")Assert.Equal(typeof(DateOnly),actual["date_of_birth"].ClrType);
    }
    [Fact]
    public void ServerDefaults_AreExplicitAndNoSessionExpiryOrVersionIsInvented()
    {
        using var context=RoleMappingTests.Create();
        var model=context.GetService<IDesignTimeModel>().Model;
        var account=model.FindEntityType(typeof(Account))!;
        var columns=account.GetProperties().ToDictionary(p=>p.GetColumnName());
        Assert.Equal("ACTIVE",columns["status"].GetDefaultValue());
        Assert.Equal(new Account().Status,columns["status"].Sentinel);
        Assert.Equal(1L,columns["security_version"].GetDefaultValue());
        Assert.Equal(0,columns["failed_login_count"].GetDefaultValue());
        Assert.Equal("SYSUTCDATETIME()",columns["created_at"].GetDefaultValueSql());
        Assert.Null(columns["must_change_password"].FindAnnotation(RelationalAnnotationNames.DefaultValue));
        var session=model.FindEntityType(typeof(AuthSession))!;
        var s=session.GetProperties().ToDictionary(p=>p.GetColumnName());
        Assert.Equal("SYSUTCDATETIME()",s["issued_at"].GetDefaultValueSql());
        foreach(var name in new[]{"expires_at","last_activity_at","security_version","session_scope"})
        {Assert.Null(s[name].FindAnnotation(RelationalAnnotationNames.DefaultValue));Assert.Null(s[name].GetDefaultValueSql());}
    }
    [Fact]
    public void UniqueAndGuardIndexes_HaveExactColumnsFiltersAndNames()
    {
        using var context=RoleMappingTests.Create();
        var model=context.GetService<IDesignTimeModel>().Model;
        var a=model.FindEntityType(typeof(Account))!;
        Index(a,"UQ_Account_Email",true,null,"email");
        Index(a,"UQ_Account_Phone",true,"[phone] IS NOT NULL","phone");
        Index(a,"IX_Account_Role_Status_Id",false,null,"role_id","status","account_id");
        Assert.Equal("Latin1_General_100_CI_AS_SC",a.FindProperty(nameof(Account.Email))!.GetCollation());
        var p=model.FindEntityType(typeof(StudentProfile))!;
        Index(p,"UQ_StudentProfile_Account",true,null,"account_id");
        var s=model.FindEntityType(typeof(AuthSession))!;
        Index(s,"IX_AuthSession_Account_Revoked",false,null,"account_id","revoked_at");
        Index(s,"IX_AuthSession_Expires",false,null,"expires_at");
    }
    [Fact]
    public void ForeignKeys_NeverCascadeAndProfileIsOneToOne()
    {
        using var context=RoleMappingTests.Create();
        var model=context.GetService<IDesignTimeModel>().Model;
        foreach(var (type,target,column) in new[]{(typeof(Account),typeof(Role),"role_id"),(typeof(StudentProfile),typeof(Account),"account_id"),(typeof(AuthSession),typeof(Account),"account_id")})
        {
            var foreign=Assert.Single(model.FindEntityType(type)!.GetForeignKeys());
            Assert.Equal(target,foreign.PrincipalEntityType.ClrType);
            Assert.Equal(column,Assert.Single(foreign.Properties).GetColumnName());
            Assert.Equal(DeleteBehavior.NoAction,foreign.DeleteBehavior);
            Assert.True(foreign.IsRequired);
            Assert.Equal(type==typeof(StudentProfile),foreign.IsUnique);
        }
        Assert.Equal(nameof(Account.StudentProfile),model.FindEntityType(typeof(StudentProfile))!.GetForeignKeys().Single().PrincipalToDependent!.Name);
    }
    [Fact]
    public void Checks_ProtectLiteralSetsCountersBundlePhoneAndChronology()
    {
        using var context=RoleMappingTests.Create();
        var model=context.GetService<IDesignTimeModel>().Model;
        var checks=model.FindEntityType(typeof(Account))!.GetCheckConstraints().ToArray();
        Assert.Equal(5,checks.Length);
        var status=checks.Single(c=>c.Sql.Contains("[status]"));
        foreach(var literal in new[]{"ACTIVE","LOCKED","DISABLED"})Assert.Contains("'"+literal+"'",status.Sql);
        Assert.Contains("BIN2",status.Sql);
        Assert.Contains("> 0",checks.Single(c=>c.Sql.Contains("[security_version]")).Sql);
        Assert.Contains(">= 0",checks.Single(c=>c.Sql.Contains("[failed_login_count]")).Sql);
        var temp=checks.Single(c=>c.Sql.Contains("[must_change_password]")).Sql;
        foreach(var term in new[]{"[temp_issued_at] IS NOT NULL","[temp_expires_at] IS NOT NULL","[temp_issued_at] IS NULL","[temp_expires_at] IS NULL","DATEADD(hour, 24, [temp_issued_at])","[must_change_password] = 1","[must_change_password] = 0"})Assert.Contains(term,temp);
        var phone=checks.Single(c=>c.Sql.Contains("[phone]")).Sql;
        foreach(var term in new[]{"IS NULL","DATALENGTH","10","BIN2","[^0-9]","NOT LIKE"})Assert.Contains(term,phone);
        Assert.Contains("LEFT([phone], 1) = '0'",phone);
        var session=model.FindEntityType(typeof(AuthSession))!.GetCheckConstraints().ToArray();
        var scope=session.Single(c=>c.Sql.Contains("[session_scope]")).Sql;
        Assert.Contains("'NORMAL'",scope);Assert.Contains("'PASSWORD_CHANGE'",scope);Assert.Contains("BIN2",scope);
        var all=string.Join(" ",session.Select(c=>c.Sql));
        foreach(var term in new[]{"[issued_at] <= [last_activity_at]","[last_activity_at] <= [expires_at]","[expires_at] > [issued_at]","[revoked_at] IS NULL","[revoked_at] >= [issued_at]"})Assert.Contains(term,all);
    }
    [Fact]
    public void SQLServerDDL_IsGeneratedDisconnectedForExactlyFourTables()
    {
        using var context=RoleMappingTests.Create();
        var model=context.GetService<IDesignTimeModel>().Model;
        Assert.Equal(new[]{"Account","AuthSession","Role","StudentProfile"},model.GetEntityTypes().Select(e=>e.GetTableName()).Order().ToArray());
        Assert.Equal(33,model.GetEntityTypes().Sum(e=>e.GetProperties().Count()));
        Assert.Equal(7,model.GetEntityTypes().Sum(e=>e.GetIndexes().Count()));
        Assert.Equal(9,model.GetEntityTypes().Sum(e=>e.GetCheckConstraints().Count()));
        var ddl=context.Database.GenerateCreateScript();
        foreach(var table in new[]{"Role","Account","StudentProfile","AuthSession"})Assert.Contains("CREATE TABLE [dbo].["+table+"]",ddl);
        Assert.Equal(4,ddl.Split("CREATE TABLE",StringSplitOptions.None).Length-1);
        Assert.Equal(3,ddl.Split("rowversion NOT NULL",StringSplitOptions.None).Length-1);
        Assert.Contains("WHERE [phone] IS NOT NULL",ddl);
        Assert.DoesNotContain("ON DELETE CASCADE",ddl);
        Assert.DoesNotContain("JWT",ddl);Assert.DoesNotContain("__EFMigrationsHistory",ddl);
        Assert.Equal(System.Data.ConnectionState.Closed,context.Database.GetDbConnection().State);
        Assert.False(context.Database.CreateExecutionStrategy().RetriesOnFailure);
    }
    private static void Index(IEntityType entity,string name,bool unique,string? filter,params string[] columns)
    {
        var index=entity.GetIndexes().Single(i=>i.GetDatabaseName()==name);
        Assert.Equal(unique,index.IsUnique);Assert.Equal(filter,index.GetFilter());
        Assert.Equal(columns,index.Properties.Select(p=>p.GetColumnName()).ToArray());
    }
}

namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Domain.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class AcademicsMappingTests
{
    private static readonly Dictionary<string, Dictionary<string, string>> Expected = new()
    {
        ["Course"] = new() { ["course_id"]="int", ["course_code"]="varchar(30)", ["course_name"]="nvarchar(300)", ["base_price"]="decimal(18,0)", ["status"]="varchar(16)", ["row_version"]="rowversion" },
        ["Band"] = new() { ["band_id"]="int", ["band_code"]="varchar(30)", ["min_score"]="decimal(5,2)", ["row_version"]="rowversion" },
        ["TrainingProgram"] = new() { ["program_id"]="int", ["course_id"]="int", ["band_id"]="int", ["program_name"]="nvarchar(300)", ["status"]="varchar(16)", ["expected_lesson_count"]="int", ["row_version"]="rowversion" },
        ["ScoreComponentConfig"] = new() { ["component_id"]="int", ["program_id"]="int", ["component_name"]="nvarchar(200)", ["weight"]="decimal(5,2)", ["kind"]="varchar(16)", ["row_version"]="rowversion" }
    };

    [Theory]
    [InlineData("Course", "course_id")]
    [InlineData("Band", "band_id")]
    [InlineData("TrainingProgram", "program_id")]
    [InlineData("ScoreComponentConfig", "component_id")]
    public void PhysicalColumns_AreExactWithIdentityAndGeneratedRowVersion(string table, string id)
    {
        using var context = RoleMappingTests.Create();
        var entity = Entity(context.GetService<IDesignTimeModel>().Model, table);
        Assert.Equal("dbo", entity.GetSchema());
        var columns = entity.GetProperties().ToDictionary(p => p.GetColumnName());
        Assert.Equal(Expected[table].Keys.Order(), columns.Keys.Order());
        foreach (var pair in Expected[table])
        {
            var property = columns[pair.Key];
            Assert.Equal(pair.Value, property.GetColumnType());
            Assert.False(property.IsNullable);
            Assert.False(property.IsShadowProperty());
            if (pair.Value.StartsWith("varchar", StringComparison.Ordinal)) Assert.False(property.IsUnicode());
            if (pair.Value.StartsWith("nvarchar", StringComparison.Ordinal)) Assert.True(property.IsUnicode());
            if (pair.Key != "status" || table != "TrainingProgram")
            {
                Assert.Null(property.FindAnnotation(RelationalAnnotationNames.DefaultValue));
                Assert.Null(property.GetDefaultValueSql());
            }
        }
        var key = Assert.Single(entity.FindPrimaryKey()!.Properties);
        Assert.Equal(id, key.GetColumnName());
        Assert.Equal(ValueGenerated.OnAdd, key.ValueGenerated);
        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, key.GetValueGenerationStrategy());
        Assert.Equal(1L, key.GetIdentitySeed());
        Assert.Equal(1, key.GetIdentityIncrement());
        var rv = columns["row_version"];
        Assert.Equal(typeof(byte[]), rv.ClrType);
        Assert.True(rv.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, rv.ValueGenerated);
        Assert.Equal(PropertySaveBehavior.Ignore, rv.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Ignore, rv.GetAfterSaveBehavior());
        Assert.Empty(entity.GetSeedData());
    }

    [Fact]
    public void DecimalStorageAndUnicodeCapacities_PreserveAcceptedRepresentation()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        foreach (var (table, column, precision, scale) in new[] { ("Course","base_price",18,0), ("Band","min_score",5,2), ("ScoreComponentConfig","weight",5,2) })
        {
            var property = Column(Entity(model, table), column);
            Assert.Equal(typeof(decimal), property.ClrType);
            Assert.Equal(precision, property.GetPrecision());
            Assert.Equal(scale, property.GetScale());
        }
        foreach (var (table, column, capacity) in new[] { ("Course","course_name",300), ("TrainingProgram","program_name",300), ("ScoreComponentConfig","component_name",200), ("Course","course_code",30), ("Band","band_code",30), ("Course","status",16), ("TrainingProgram","status",16), ("ScoreComponentConfig","kind",16) })
            Assert.Equal(capacity, Column(Entity(model, table), column).GetMaxLength());
        Assert.Equal("Latin1_General_100_CI_AS_SC", model.GetCollation());
        Assert.Equal("Latin1_General_100_CI_AS_SC", Column(Entity(model,"ScoreComponentConfig"),"component_name").GetCollation());
    }

    [Fact]
    public void DraftIsTheOnlySqlDefault_EmptySentinelAllowsDatabaseDefault()
    {
        using var context = RoleMappingTests.Create();
        var program = Entity(context.GetService<IDesignTimeModel>().Model, "TrainingProgram");
        var status = Column(program,"status");
        Assert.Equal("DRAFT", status.GetDefaultValue());
        Assert.Equal(string.Empty, status.Sentinel);
        Assert.Equal(new TrainingProgram().Status, status.Sentinel);
        Assert.Equal(ValueGenerated.OnAdd, status.ValueGenerated);
        Assert.Equal(string.Empty, new Course().Status);
        Assert.Equal(string.Empty, new ScoreComponentConfig().Kind);
    }

    [Fact]
    public void Indexes_EnforceAcceptedUniquenessAndCoverForeignKeysWithoutDuplicates()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        var course = Entity(model,"Course");
        Index(course,"UQ_Course_Code",true,null,"course_code");
        Index(course,"IX_Course_Status_Id",false,null,"status","course_id");
        Assert.Equal(2,course.GetIndexes().Count());
        var band = Entity(model,"Band");
        Index(band,"UQ_Band_Code",true,null,"band_code");
        Index(band,"UQ_Band_MinScore",true,null,"min_score");
        Assert.Equal(2,band.GetIndexes().Count());
        var program = Entity(model,"TrainingProgram");
        Index(program,"UQ_Program_CourseBand",true,null,"course_id","band_id");
        Index(program,"IX_Program_Band",false,null,"band_id");
        Index(program,"IX_Program_Status_Id",false,null,"status","program_id");
        Assert.Equal(3,program.GetIndexes().Count());
        var component = Entity(model,"ScoreComponentConfig");
        Index(component,"UQ_Component_ProgramName",true,null,"program_id","component_name");
        Index(component,"UQ_Component_FinalExam",true,"[kind] = 'FINAL_EXAM'","program_id");
        Assert.Equal(2,component.GetIndexes().Count());
        Assert.DoesNotContain(course.GetIndexes(), i => i.Properties.Any(p => p.GetColumnName()=="course_name"));
        Assert.DoesNotContain(program.GetIndexes(), i => i.Properties.Any(p => p.GetColumnName()=="program_name"));
    }

    [Fact]
    public void RequiredForeignKeys_AreNoActionAndNeverProduceShadowColumns()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        var program = Entity(model,"TrainingProgram");
        Assert.Equal(2,program.GetForeignKeys().Count());
        ForeignKey(program,"course_id",typeof(Course),"FK_TrainingProgram_Course_course_id");
        ForeignKey(program,"band_id",typeof(Band),"FK_TrainingProgram_Band_band_id");
        var component = Entity(model,"ScoreComponentConfig");
        Assert.Single(component.GetForeignKeys());
        ForeignKey(component,"program_id",typeof(TrainingProgram),"FK_ScoreComponentConfig_TrainingProgram_program_id");
        Assert.Empty(Entity(model,"Course").GetForeignKeys());
        Assert.Empty(Entity(model,"Band").GetForeignKeys());
        Assert.Equal(typeof(Course),program.FindNavigation(nameof(TrainingProgram.Course))!.TargetEntityType.ClrType);
        Assert.Equal(typeof(Band),program.FindNavigation(nameof(TrainingProgram.Band))!.TargetEntityType.ClrType);
        Assert.Equal(typeof(TrainingProgram),component.FindNavigation(nameof(ScoreComponentConfig.Program))!.TargetEntityType.ClrType);
    }

    [Fact]
    public void Checks_KeepRangeBoundariesCodesAndCasePaddingGuards()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        Check(Entity(model,"Course"),"CK_Course_Price","[base_price] > 0");
        Check(Entity(model,"Course"),"CK_Course_Code","DATALENGTH([course_code]) BETWEEN 1 AND 30","DATALENGTH([course_code]) = LEN([course_code])","Latin1_General_100_BIN2","NOT LIKE '%[^A-Z0-9_-]%'");
        LiteralCheck(Entity(model,"Course"),"CK_Course_Status","status","ACTIVE","INACTIVE");
        Check(Entity(model,"Band"),"CK_Band_MinScore","[min_score] >= 0","[min_score] <= 100");
        Check(Entity(model,"Band"),"CK_Band_Code","DATALENGTH([band_code]) BETWEEN 1 AND 30","Latin1_General_100_BIN2","NOT LIKE '%[^ -~]%'","LEN([band_code]) > 0");
        LiteralCheck(Entity(model,"TrainingProgram"),"CK_TrainingProgram_Status","status","DRAFT","ACTIVE","INACTIVE");
        Check(Entity(model,"TrainingProgram"),"CK_TrainingProgram_ExpectedLessons","[expected_lesson_count] > 0");
        var component = Entity(model,"ScoreComponentConfig");
        LiteralCheck(component,"CK_ScoreComponentConfig_Kind","kind","GENERAL","LESSON","FINAL_EXAM");
        Check(component,"CK_ScoreComponentConfig_Weight","[weight] > 0","[weight] <= 100");
        Check(component,"CK_ScoreComponentConfig_Name","LEN([component_name]) > 0");
        Assert.Equal(10,model.GetEntityTypes().Where(e=>e.ClrType.Namespace==typeof(Course).Namespace).Sum(e=>e.GetCheckConstraints().Count()));
    }

    [Fact]
    public void D01D02_SubsetRetainsPhysicalSignatureWithConnectionClosed()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        var prior=model.GetEntityTypes().Where(e=>e.ClrType.Namespace==typeof(Course).Namespace || e.ClrType.Namespace==typeof(EnglishCenter.Api.Domain.Identity.Role).Namespace).ToArray();
        Assert.Equal(new[]{"Account","AuthSession","Band","Course","Role","ScoreComponentConfig","StudentProfile","TrainingProgram"},prior.Select(e=>e.GetTableName()).Order());
        Assert.Equal(56,prior.Sum(e=>e.GetProperties().Count()));
        Assert.Equal(16,prior.Sum(e=>e.GetIndexes().Count()));
        Assert.Equal(19,prior.Sum(e=>e.GetCheckConstraints().Count()));
        Assert.Equal(6,prior.Sum(e=>e.GetForeignKeys().Count()));
        Assert.All(model.GetEntityTypes().SelectMany(e=>e.GetForeignKeys()), f=>Assert.Equal(DeleteBehavior.NoAction,f.DeleteBehavior));
        var ddl = context.Database.GenerateCreateScript();
        Assert.Equal(8,prior.Length);
        Assert.Equal(7,prior.SelectMany(e=>e.GetProperties()).Count(p=>p.GetColumnType()=="rowversion"));
        foreach(var table in Expected.Keys) Assert.Contains("CREATE TABLE [dbo].["+table+"]",ddl);
        Assert.Contains("[base_price] decimal(18,0) NOT NULL",ddl);
        Assert.Contains("[min_score] decimal(5,2) NOT NULL",ddl);
        Assert.Contains("[weight] decimal(5,2) NOT NULL",ddl);
        Assert.Contains("DEFAULT 'DRAFT'",ddl);
        Assert.Contains("CREATE UNIQUE INDEX [UQ_Component_FinalExam] ON [dbo].[ScoreComponentConfig] ([program_id]) WHERE [kind] = 'FINAL_EXAM'",ddl);
        Assert.DoesNotContain("ON DELETE CASCADE",ddl);
        Assert.DoesNotContain("__EFMigrationsHistory",ddl);
        Assert.DoesNotContain("CREATE TRIGGER",ddl);
        Assert.Equal(System.Data.ConnectionState.Closed,context.Database.GetDbConnection().State);
        Assert.False(context.Database.CreateExecutionStrategy().RetriesOnFailure);
    }

    private static IEntityType Entity(IModel model,string table)=>model.GetEntityTypes().Single(e=>e.GetTableName()==table);
    private static IProperty Column(IEntityType entity,string column)=>entity.GetProperties().Single(p=>p.GetColumnName()==column);
    private static void Index(IEntityType entity,string name,bool unique,string? filter,params string[] columns)
    {
        var index=entity.GetIndexes().Single(i=>i.GetDatabaseName()==name);
        Assert.Equal(unique,index.IsUnique); Assert.Equal(filter,index.GetFilter());
        Assert.Equal(columns,index.Properties.Select(p=>p.GetColumnName()));
    }
    private static void ForeignKey(IEntityType entity,string column,Type target,string constraint)
    {
        var foreign=entity.GetForeignKeys().Single(f=>Assert.Single(f.Properties).GetColumnName()==column);
        Assert.Equal(target,foreign.PrincipalEntityType.ClrType); Assert.True(foreign.IsRequired);
        Assert.False(foreign.IsUnique); Assert.Equal(DeleteBehavior.NoAction,foreign.DeleteBehavior);
        Assert.Equal(constraint,foreign.GetConstraintName());
        Assert.False(Assert.Single(foreign.Properties).IsShadowProperty());
    }
    private static void Check(IEntityType entity,string name,params string[] terms)
    {
        var sql=entity.GetCheckConstraints().Single(c=>c.Name==name).Sql;
        foreach(var term in terms) Assert.Contains(term,sql);
    }
    private static void LiteralCheck(IEntityType entity,string name,string column,params string[] literals)
    {
        var sql=entity.GetCheckConstraints().Single(c=>c.Name==name).Sql;
        foreach(var literal in literals) Assert.Contains("'"+literal+"'",sql);
        Assert.Contains("COLLATE Latin1_General_100_BIN2",sql);
        Assert.Contains("DATALENGTH(["+column+"]) = LEN(["+column+"])",sql);
        Assert.Equal(literals.Length,sql.Count(c=>c=='\'')/2);
    }
}

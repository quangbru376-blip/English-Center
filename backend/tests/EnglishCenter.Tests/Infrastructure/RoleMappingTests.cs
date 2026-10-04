namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Domain.Identity;
using EnglishCenter.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class RoleMappingTests
{
    internal static EnglishCenterDbContext Create()=>new(new DbContextOptionsBuilder<EnglishCenterDbContext>()
        .UseSqlServer("Server=disconnected.invalid;Database=MetadataOnly;Integrated Security=True;Encrypt=True",sql=>sql.UseCompatibilityLevel(160)).Options);
    [Fact]
    public void Role_HasAcceptedPhysicalSignatureAndExactCodeConstraint()
    {
        using var context=Create();
        var model=context.GetService<IDesignTimeModel>().Model;
        var role=model.FindEntityType(typeof(Role))!;
        Assert.Equal("dbo",role.GetSchema());Assert.Equal("Role",role.GetTableName());
        var columns=role.GetProperties().ToDictionary(p=>p.GetColumnName(),StringComparer.Ordinal);
        Assert.Equal(new[]{"role_code","role_id","role_name"},columns.Keys.Order().ToArray());
        Assert.Equal("int",columns["role_id"].GetColumnType());
        Assert.Equal(ValueGenerated.OnAdd,columns["role_id"].ValueGenerated);
        Assert.Equal(new[]{columns["role_id"]},role.FindPrimaryKey()!.Properties);
        Assert.Equal("varchar(32)",columns["role_code"].GetColumnType());
        Assert.Equal("nvarchar(200)",columns["role_name"].GetColumnType());
        Assert.All(columns.Values,p=>Assert.False(p.IsNullable));
        var unique=Assert.Single(role.GetIndexes());
        Assert.True(unique.IsUnique);Assert.Equal("UQ_Role_Code",unique.GetDatabaseName());
        Assert.Equal(new[]{columns["role_code"]},unique.Properties);
        var check=Assert.Single(role.GetCheckConstraints()).Sql;
        foreach(var code in new[]{"Student","Admissions","AcademicAffairs","Teacher","Accountant","Director","Admin"})
            Assert.Contains("'"+code+"'",check);
        Assert.Contains("BIN2",check);
        Assert.Equal("Latin1_General_100_CI_AS_SC",model.GetCollation());
        Assert.Empty(role.GetSeedData());
        var ddl=context.Database.GenerateCreateScript();
        Assert.Contains("CREATE TABLE [dbo].[Role]",ddl);
        Assert.Contains("IDENTITY",ddl);
        Assert.Contains("CREATE UNIQUE INDEX [UQ_Role_Code]",ddl);
        Assert.False(context.Database.GetDbConnection().State==System.Data.ConnectionState.Open);
    }
    [Fact]
    public void CompletedD01_ModelContainsExactlyFourReviewedTables()
    {
        using var context=Create();
        Assert.Equal(new[]{"Account","AuthSession","Role","StudentProfile"},context.Model.GetEntityTypes().Select(e=>e.GetTableName()).Order().ToArray());
    }
}

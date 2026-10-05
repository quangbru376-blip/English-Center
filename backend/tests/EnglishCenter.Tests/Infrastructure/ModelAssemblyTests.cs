namespace EnglishCenter.Tests.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class ModelAssemblyTests
{
    private static readonly string[] TriggerAffectedTables =
        ["Attendance", "Band", "Score", "ScoreComponentConfig", "TrainingProgram"];

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OutputClause_IsDisabledOnlyOnFiveTriggerAffectedTables(bool designTime)
    {
        using var context = RoleMappingTests.Create();
        var model = designTime ? context.GetService<IDesignTimeModel>().Model : context.Model;
        var disabled = model.GetEntityTypes().Where(e => !e.IsSqlOutputClauseUsed()).Select(e => e.GetTableName()).Order();
        Assert.Equal(TriggerAffectedTables, disabled);
        Assert.Equal(19, model.GetEntityTypes().Count(e => e.IsSqlOutputClauseUsed()));
        foreach (var entity in model.GetEntityTypes())
        {
            var table = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            Assert.Equal(!TriggerAffectedTables.Contains(table.Name), entity.IsSqlOutputClauseUsed(table));
        }
        Assert.Equal(System.Data.ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [Fact]
    public void RelationalTables_KeepOutputScopeAndPhysicalSignature()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        var tables = model.GetRelationalModel().Tables.ToArray();
        Assert.Equal(24, tables.Length);
        Assert.Equal(230, tables.Sum(t => t.Columns.Count()));
        Assert.Equal(TriggerAffectedTables, tables.Where(t => !t.IsSqlOutputClauseUsed()).Select(t => t.Name).Order());
        Assert.Equal(19, tables.Count(t => t.IsSqlOutputClauseUsed()));
        Assert.All(tables, t => Assert.Equal("dbo", t.Schema));
        var ddl = context.Database.GenerateCreateScript();
        Assert.Equal(24, ddl.Split("CREATE TABLE", StringSplitOptions.None).Length - 1);
        Assert.Equal(20, ddl.Split("rowversion NOT NULL", StringSplitOptions.None).Length - 1);
        Assert.DoesNotContain("CREATE TRIGGER", ddl);
        Assert.DoesNotContain("__EFMigrationsHistory", ddl);
    }

    [Fact]
    public void TriggerCompatibilityMetadata_DoesNotPretendTriggersAreImplemented()
    {
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        Assert.All(model.GetEntityTypes(), entity => Assert.Empty(entity.GetDeclaredTriggers()));
        Assert.Equal(50, model.GetEntityTypes().Sum(e => e.GetForeignKeys().Count()));
        Assert.All(model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()), fk => Assert.Equal(DeleteBehavior.NoAction, fk.DeleteBehavior));
        Assert.Equal(78, model.GetEntityTypes().Sum(e => e.GetIndexes().Count()));
        Assert.Equal(70, model.GetEntityTypes().Sum(e => e.GetCheckConstraints().Count()));
    }
}

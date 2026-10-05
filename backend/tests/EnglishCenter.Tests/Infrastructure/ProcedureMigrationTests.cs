namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

public class ProcedureMigrationTests
{
    [Fact]
    public void ProcedureMigration_SevenImmutableResourcesReverseDropsWithoutTableOrModelMutation()
    {
        using var context=new EnglishCenterDesignTimeFactory().CreateDbContext([]);
        var assembly=context.GetService<IMigrationsAssembly>();
        var type=Assert.Single(assembly.Migrations,x=>x.Key.EndsWith("_ReadOnlyQueriesAndInvoiceAllocator",StringComparison.Ordinal)).Value;
        var migration=assembly.CreateMigration(type,context.Database.ProviderName!);
        var creates=migration.UpOperations.Cast<SqlOperation>().ToArray();
        var drops=migration.DownOperations.Cast<SqlOperation>().ToArray();
        Assert.Equal(7,creates.Length);Assert.Equal(7,drops.Length);
        string[] names=["usp_SearchAdmissions","usp_GetEntryTestRoster","usp_GetClassGradeReview","usp_GetFinancialReport","usp_GetAdmissionsReport","usp_GetTrainingReport","usp_AllocateInvoiceNumber"];
        for(var i=0;i<names.Length;i++)
        {
            Assert.Contains(names[i],creates[i].Sql,StringComparison.Ordinal);
            Assert.Equal("DROP PROCEDURE [dbo].["+names[6-i]+"];",drops[i].Sql);
            Assert.False(creates[i].SuppressTransaction);
        }
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.Equal(System.Data.ConnectionState.Closed,context.Database.GetDbConnection().State);
    }
}

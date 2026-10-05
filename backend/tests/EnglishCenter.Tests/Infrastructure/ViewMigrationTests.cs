namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

public class ViewMigrationTests
{
    [Fact]
    public void ViewMigration_ContainsOnlySixImmutableResourcesAndReverseDrops_NoPhysicalModelChange()
    {
        using var context = new EnglishCenterDesignTimeFactory().CreateDbContext([]);
        var assembly = context.GetService<IMigrationsAssembly>();
        var type = Assert.Single(assembly.Migrations, x => x.Key.EndsWith("_ReadOnlyBusinessViews", StringComparison.Ordinal)).Value;
        var migration = assembly.CreateMigration(type, context.Database.ProviderName!);
        var creates = migration.UpOperations.Cast<SqlOperation>().ToArray();
        var drops = migration.DownOperations.Cast<SqlOperation>().ToArray();
        Assert.Equal(6, creates.Length); Assert.Equal(6, drops.Length);
        string[] names = ["vw_AdmissionsRegistrationFacts","vw_EntryTestBookingFacts","vw_CurrentScheduleOccurrences","vw_TrainingClassFacts","vw_FinalizedGradeFacts","vw_FinanceInvoiceFacts"];
        for (var i = 0; i < names.Length; i++)
        {
            Assert.Contains("CREATE OR ALTER VIEW [dbo].[" + names[i] + "]", creates[i].Sql, StringComparison.Ordinal);
            Assert.Equal("DROP VIEW [dbo].[" + names[5-i] + "];", drops[i].Sql);
            Assert.False(creates[i].SuppressTransaction);
        }
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.Equal(System.Data.ConnectionState.Closed, context.Database.GetDbConnection().State);
    }
}

namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

public class RoleMigrationTests
{
    [Fact]
    public void RoleMigration_FourBoundedResourcesTransactionalAndGuardedReverseDrops()
    {
        using var context = new EnglishCenterDesignTimeFactory().CreateDbContext([]);
        var assembly = context.GetService<IMigrationsAssembly>();
        var type = Assert.Single(assembly.Migrations, x => x.Key.EndsWith("_LeastPrivilegeRoles", StringComparison.Ordinal)).Value;
        var migration = assembly.CreateMigration(type, context.Database.ProviderName!);
        var creates = migration.UpOperations.Cast<SqlOperation>().ToArray();
        Assert.Equal(4, creates.Length);
        string[] roles = ["ec_app_runtime", "ec_finance_reporting", "ec_academics_reporting", "ec_director_reporting"];
        for (var i = 0; i < roles.Length; i++)
        {
            Assert.Contains("CREATE ROLE [" + roles[i] + "] AUTHORIZATION [dbo]", creates[i].Sql, StringComparison.Ordinal);
            Assert.Contains("THROW 51212", creates[i].Sql, StringComparison.Ordinal);
            Assert.False(creates[i].SuppressTransaction);
            Assert.DoesNotContain("ON SCHEMA", creates[i].Sql, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("CREATE USER", creates[i].Sql, StringComparison.OrdinalIgnoreCase);
        }
        var down = Assert.IsType<SqlOperation>(Assert.Single(migration.DownOperations));
        Assert.Contains("THROW 51213", down.Sql, StringComparison.Ordinal);
        Assert.True(down.Sql.IndexOf("THROW", StringComparison.Ordinal) < down.Sql.IndexOf("DROP ROLE", StringComparison.Ordinal));
        Assert.False(down.SuppressTransaction);
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.Equal(System.Data.ConnectionState.Closed, context.Database.GetDbConnection().State);
    }
}

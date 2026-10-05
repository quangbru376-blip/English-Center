namespace EnglishCenter.Tests.Infrastructure;

using EnglishCenter.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

public class MigrationChainTests
{
    private static Migration Initial(EnglishCenterDbContext context)
    {
        var assembly = context.GetService<IMigrationsAssembly>();
        return assembly.CreateMigration(Assert.Single(assembly.Migrations, m => m.Key.EndsWith("_InitialSchema", StringComparison.Ordinal)).Value, context.Database.ProviderName!);
    }

    [Fact]
    public void GeneratedMigrationMatchesCompleteModelWithoutSeedOrRawSql()
    {
        using var context = new EnglishCenterDesignTimeFactory().CreateDbContext([]);
        var migration = Initial(context);
        var tables = migration.UpOperations.OfType<CreateTableOperation>().ToArray();
        Assert.Equal(24, tables.Length);
        Assert.Equal(230, tables.Sum(t => t.Columns.Count));
        Assert.Equal(24, tables.Count(t => t.PrimaryKey != null));
        Assert.Equal(2, tables.Sum(t => t.UniqueConstraints.Count));
        Assert.Equal(70, tables.Sum(t => t.CheckConstraints.Count));
        Assert.Equal(78, migration.UpOperations.OfType<CreateIndexOperation>().Count());
        Assert.Equal(20, tables.SelectMany(t => t.Columns).Count(c => c.IsRowVersion));
        Assert.Equal(50, tables.Sum(t => t.ForeignKeys.Count) + migration.UpOperations.OfType<AddForeignKeyOperation>().Count());
        Assert.All(tables.SelectMany(t => t.ForeignKeys).Concat(migration.UpOperations.OfType<AddForeignKeyOperation>()), fk => Assert.Equal(ReferentialAction.NoAction, fk.OnDelete));
        Assert.Empty(migration.UpOperations.OfType<InsertDataOperation>());
        Assert.Empty(migration.UpOperations.OfType<SqlOperation>());
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.Equal(System.Data.ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [Fact]
    public void UpCreatesEveryForeignKeyAfterItsPrincipalAndDefersCycle()
    {
        using var context = new EnglishCenterDesignTimeFactory().CreateDbContext([]);
        var created = new HashSet<string>(StringComparer.Ordinal);
        foreach (var operation in Initial(context).UpOperations)
        {
            if (operation is CreateTableOperation table)
            {
                created.Add(table.Name);
                Assert.All(table.ForeignKeys, fk => Assert.Contains(fk.PrincipalTable, created));
            }
            if (operation is AddForeignKeyOperation fk)
            {
                Assert.Contains(fk.Table, created);
                Assert.Contains(fk.PrincipalTable, created);
            }
        }
        var cycle = Assert.Single(Initial(context).UpOperations.OfType<AddForeignKeyOperation>(), fk => fk.Name == "FK_Attempt_Booking_CompletedBookingAttempt");
        Assert.Equal(new[] { "completed_booking_id", "attempt_id" }, cycle.Columns);
        Assert.Equal(new[] { "booking_id", "attempt_id" }, cycle.PrincipalColumns);
    }

    [Fact]
    public void DownDropsAllReferencingForeignKeysBeforeDroppingPrincipals()
    {
        using var context = new EnglishCenterDesignTimeFactory().CreateDbContext([]);
        var migration = Initial(context);
        var tables = migration.UpOperations.OfType<CreateTableOperation>().ToDictionary(t => t.Name);
        var foreignKeys = tables.Values.SelectMany(t => t.ForeignKeys).Concat(migration.UpOperations.OfType<AddForeignKeyOperation>()).ToList();
        foreach (var operation in migration.DownOperations)
        {
            if (operation is DropForeignKeyOperation dropFk)
                Assert.Equal(1, foreignKeys.RemoveAll(fk => fk.Table == dropFk.Table && fk.Name == dropFk.Name));
            if (operation is DropTableOperation dropTable)
            {
                Assert.DoesNotContain(foreignKeys, fk => fk.PrincipalTable == dropTable.Name && fk.Table != dropTable.Name);
                foreignKeys.RemoveAll(fk => fk.Table == dropTable.Name);
                Assert.True(tables.Remove(dropTable.Name));
            }
        }
        Assert.Empty(tables);
        Assert.Empty(foreignKeys);
    }
}

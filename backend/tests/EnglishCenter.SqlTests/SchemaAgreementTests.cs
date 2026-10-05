namespace EnglishCenter.SqlTests;

using EnglishCenter.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class SchemaAgreementTests
{
    public sealed class ColumnShape
    {
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public short MaxLength { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public string? CollationName { get; set; }
    }

    [Fact]
    public async Task LiveColumnsMatchEveryAcceptedMappedNameTypeNullabilityIdentityAndCollation()
    {
        await using var f=await SqlFixture.CreateAsync();
        var actual=await f.Context.Database.SqlQueryRaw<ColumnShape>("""
            SELECT t.name AS TableName,c.name AS ColumnName,ty.name AS TypeName,
                   c.max_length AS MaxLength,c.precision AS Precision,c.scale AS Scale,
                   c.is_nullable AS IsNullable,c.is_identity AS IsIdentity,c.collation_name AS CollationName
            FROM sys.tables t JOIN sys.columns c ON c.object_id=t.object_id
            JOIN sys.types ty ON ty.user_type_id=c.user_type_id
            WHERE t.schema_id=SCHEMA_ID('dbo') AND t.name<>'__EFMigrationsHistory'
            """).ToListAsync(TestContext.Current.CancellationToken);
        var model=f.Context.GetService<IDesignTimeModel>().Model;
        var expected=model.GetRelationalModel().Tables.ToArray();
        Assert.Equal(24,expected.Length);Assert.Equal(230,actual.Count);
        Assert.Equal(expected.Select(t=>t.Name).Order().ToArray(),actual.Select(c=>c.TableName).Distinct().Order().ToArray());
        foreach(var table in expected)
        foreach(var column in table.Columns)
        {
            var row=Assert.Single(actual,c=>c.TableName==table.Name && c.ColumnName==column.Name);
            var actualType=row.TypeName switch
            {
                "timestamp"=>"rowversion",
                "decimal"=>$"decimal({row.Precision},{row.Scale})",
                "datetime2" or "time"=>$"{row.TypeName}({row.Scale})",
                "varchar" or "nvarchar" or "binary" or "varbinary"=>$"{row.TypeName}({(row.MaxLength==-1?"max":(row.TypeName=="nvarchar"?row.MaxLength/2:row.MaxLength).ToString(System.Globalization.CultureInfo.InvariantCulture))})",
                _=>row.TypeName
            };
            Assert.Equal(column.StoreType,actualType);Assert.Equal(column.IsNullable,row.IsNullable);
            var property=Assert.Single(column.PropertyMappings).Property;
            Assert.Equal(property.GetValueGenerationStrategy()==SqlServerValueGenerationStrategy.IdentityColumn,row.IsIdentity);
            Assert.Equal(property.ClrType==typeof(string)?property.GetCollation()??model.GetCollation():null,row.CollationName);
            if(column.StoreType=="rowversion") Assert.Equal(8,row.MaxLength);
        }
    }
}

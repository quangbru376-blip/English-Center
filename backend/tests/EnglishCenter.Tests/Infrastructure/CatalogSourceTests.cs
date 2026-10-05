namespace EnglishCenter.Tests.Infrastructure;

using System.Text.Json;
using EnglishCenter.Api.Infrastructure.Compatibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class CatalogSourceTests
{
    [Fact]
    public void ManifestPhysicalNamesAndOrderedKeysIndexesForeignKeysMatchIndependentEfSource()
    {
        using var stream = typeof(DatabaseCompatibilityInspector).Assembly.GetManifestResourceStream("EnglishCenter.Api.Infrastructure.Compatibility.ExpectedCatalog.json")!;
        var manifest = JsonSerializer.Deserialize<Dictionary<string,string>>(stream)!;
        using var context = RoleMappingTests.Create();
        var model = context.GetService<IDesignTimeModel>().Model;
        var expectedNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entity in model.GetEntityTypes())
        {
            var table = entity.GetTableName()!;
            var prefix = "dbo/" + table + "/";
            Assert.Contains("table/dbo/" + table, manifest.Keys);
            foreach (var property in entity.GetProperties())
            {
                var key = "column/" + prefix + property.GetColumnName(); expectedNames.Add(key);
                var parts = manifest[key].Split('|');
                var type = property.GetColumnType()!.Split('(')[0];
                Assert.Equal(type == "rowversion" ? "timestamp" : type, parts[0]);
                Assert.Equal(property.IsNullable ? "1" : "0", parts[4]);
                Assert.Equal(property.GetValueGenerationStrategy() == SqlServerValueGenerationStrategy.IdentityColumn ? "1" : "0",parts[5]);
                var storeType = property.GetColumnType()!;
                var size = type switch
                {
                    "int" => 4, "smallint" => 2, "tinyint" => 1, "bigint" => 8, "bit" => 1, "rowversion" => 8, "date" => 3,
                    "datetime2" => 7, "time" => 3, "uniqueidentifier" => 16,
                    "decimal" => int.Parse(storeType[8..storeType.IndexOf(',')],System.Globalization.CultureInfo.InvariantCulture) <= 9 ? 5 : 9,
                    _ => storeType.Contains("max",StringComparison.Ordinal) ? -1 :
                        int.Parse(storeType[(storeType.IndexOf('(')+1)..^1],System.Globalization.CultureInfo.InvariantCulture)*(type=="nvarchar"?2:1)
                };
                Assert.Equal(size.ToString(System.Globalization.CultureInfo.InvariantCulture),parts[1]);
                if (type=="decimal") { var numbers=storeType[8..^1].Split(','); Assert.Equal(numbers[0],parts[2]);Assert.Equal(numbers[1],parts[3]); }
                Assert.Equal("0",parts[6]);
                Assert.Equal(type is "nvarchar" or "varchar" ? "Latin1_General_100_CI_AS_SC" : "",parts[7]);
            }
            foreach (var key in entity.GetKeys())
            {
                var name = "key/" + prefix + key.GetName(); expectedNames.Add(name);
                Assert.Equal((key.IsPrimaryKey() ? "PK" : "UQ") + "|" + string.Join(',',key.Properties.Select(p=>p.GetColumnName())),manifest[name]);
            }
            foreach (var index in entity.GetIndexes())
            {
                var name = "index/" + prefix + index.GetDatabaseName(); expectedNames.Add(name);
                var parts = manifest[name].Split('|');
                Assert.Equal(index.IsUnique ? "1" : "0",parts[1]);
                Assert.Equal("0",parts[4]);
                Assert.Equal(string.Join(',',index.Properties.Select((p,i)=>p.GetColumnName()+":"+(index.IsDescending is { Count: 0 } || index.IsDescending?[i]==true ? "1" : "0")+":0")),parts[6]);
                // SQL Server adds outer parentheses/spacing to these simple predicates.
                Assert.Equal(Filter(index.GetFilter() ?? ""),Filter(parts[5]));
            }
            foreach (var fk in entity.GetForeignKeys())
            {
                var name = "fk/" + prefix + fk.GetConstraintName(); expectedNames.Add(name);
                Assert.Equal("dbo."+fk.PrincipalEntityType.GetTableName()+"|0|0|0|0|"+string.Join(',',fk.Properties.Zip(fk.PrincipalKey.Properties,(a,b)=>a.GetColumnName()+":"+b.GetColumnName())),manifest[name]);
            }
            foreach (var check in entity.GetCheckConstraints()) expectedNames.Add("check/"+prefix+check.Name);
            foreach (var property in entity.GetProperties().Where(p=>p.FindAnnotation(RelationalAnnotationNames.DefaultValue)!=null || p.GetDefaultValueSql()!=null)) expectedNames.Add("default/"+prefix+property.GetColumnName());
        }
        foreach (var family in new[]{"column/","key/","fk/","check/","default/"})
            Assert.Equal(expectedNames.Where(k=>k.StartsWith(family,StringComparison.Ordinal)).Order(),manifest.Keys.Where(k=>k.StartsWith(family,StringComparison.Ordinal)&&!k.Contains("/__EFMigrationsHistory/",StringComparison.Ordinal)).Order());
        Assert.Equal(78,expectedNames.Count(k=>k.StartsWith("index/",StringComparison.Ordinal)));
    }

    private static string Filter(string value) => System.Text.RegularExpressions.Regex.Replace(value.Replace(" ","",StringComparison.Ordinal),@"\(([01])\)","$1").Trim('(',')');
}

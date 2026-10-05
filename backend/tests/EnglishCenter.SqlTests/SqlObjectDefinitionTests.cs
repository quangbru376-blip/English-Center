namespace EnglishCenter.SqlTests;

using System.Text.RegularExpressions;
using EnglishCenter.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class SqlObjectDefinitionTests
{
    [Theory]
    [InlineData("D08","TG",5)] [InlineData("D09","FN",6)]
    [InlineData("D10","VW",6)] [InlineData("D11","SP",7)]
    public async Task LiveObjects_MatchEveryImmutableEmbeddedMigrationResource(string family,string prefix,int count)
    {
        await using var f=await SqlFixture.CreateAsync();
        var assembly=typeof(EnglishCenterDbContext).Assembly;
        for(var i=1;i<=count;i++)
        {
            var resource="EnglishCenter.Api.Infrastructure.Sql."+family+"."+prefix+i.ToString("D2",System.Globalization.CultureInfo.InvariantCulture)+".sql";
            using var stream=assembly.GetManifestResourceStream(resource);Assert.NotNull(stream);
            using var reader=new StreamReader(stream);
            var expected=await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
            var match=Regex.Match(expected,@"CREATE OR ALTER\s+(?:TRIGGER|FUNCTION|VIEW|PROCEDURE)\s+(?:\[dbo\]|dbo)\.(?:\[([^\]]+)\]|([A-Za-z0-9_]+))",RegexOptions.None,TimeSpan.FromSeconds(1));
            Assert.True(match.Success,resource);
            var objectName="dbo."+(match.Groups[1].Success?match.Groups[1].Value:match.Groups[2].Value);
            var actual=await f.Context.Database.SqlQuery<string>($"SELECT definition AS Value FROM sys.sql_modules WHERE object_id=OBJECT_ID({objectName})").SingleAsync(TestContext.Current.CancellationToken);
            Assert.Equal(expected.Replace("\r\n","\n",StringComparison.Ordinal).Trim(),actual.Replace("\r\n","\n",StringComparison.Ordinal).Trim());
        }
    }
}

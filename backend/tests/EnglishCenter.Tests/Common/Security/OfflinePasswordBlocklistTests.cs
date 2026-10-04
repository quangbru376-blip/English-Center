namespace EnglishCenter.Tests.Common.Security;

using System.Text.Json;
using EnglishCenter.Api.Common.Security;
using Xunit;

public class OfflinePasswordBlocklistTests
{
    internal static string SourceDirectory => Path.Combine(AppContext.BaseDirectory,"Resources","PasswordBlocklist");
    [Fact]
    public void ActualPinnedCorpus_LoadsExactUnionAndManifest()
    {
        var blocklist=OfflinePasswordBlocklist.Load(SourceDirectory);
        Assert.Equal(15310,blocklist.Count);
        var expected=new HashSet<string>(StringComparer.Ordinal);
        foreach(var name in new[]{"10k-most-common.txt","probable-v2_top-12000.txt"})
            foreach(var value in File.ReadAllLines(Path.Combine(SourceDirectory,name))) expected.Add(value);
        Assert.Equal(expected.Count,blocklist.Count);
        Assert.True(expected.All(blocklist.Contains));
        Assert.False(blocklist.Contains(null));
        Assert.False(blocklist.Contains("a nonlisted fabricated phrase 82735"));
        using var manifest=JsonDocument.Parse(File.ReadAllText(Path.Combine(SourceDirectory,"manifest.json")));
        Assert.Equal("56da8d991239cc52b0da594db53457504a757670",manifest.RootElement.GetProperty("revision").GetString());
        Assert.Equal(15310,manifest.RootElement.GetProperty("mergedCount").GetInt32());
    }
    [Theory]
    [InlineData("10k-most-common.txt",false),InlineData("probable-v2_top-12000.txt",false),InlineData("LICENSE",false)]
    [InlineData("10k-most-common.txt",true),InlineData("probable-v2_top-12000.txt",true),InlineData("LICENSE",true)]
    public void MissingOrTamperedSource_FailsClosedWithoutContents(string file,bool corrupt)
    {
        var directory=Path.Combine(Path.GetTempPath(),"ec-blocklist-tests",Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            foreach(var name in new[]{"10k-most-common.txt","probable-v2_top-12000.txt","LICENSE"})
                File.Copy(Path.Combine(SourceDirectory,name),Path.Combine(directory,name));
            if(corrupt) File.WriteAllBytes(Path.Combine(directory,file),[0xff,0xfe,0xc0,0xaf]);
            else File.Delete(Path.Combine(directory,file));
            var error=Assert.Throws<InvalidOperationException>(()=>OfflinePasswordBlocklist.Load(directory));
            Assert.Equal("Password blocklist corpus is missing, invalid, or tampered.",error.Message);
            Assert.Null(error.InnerException);
            Assert.DoesNotContain(directory,error.Message);
        }
        finally {Directory.Delete(directory,true);}
    }
    [Fact]
    public void MissingDirectory_FailsClosed() => Assert.Throws<InvalidOperationException>(()=>OfflinePasswordBlocklist.Load(Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"))));
}

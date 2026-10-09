using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Operator;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class LocalUiFilesTests
{
    [Fact]
    public void SourceDriftAndAddedFileCannotBecomeLiveUiWithoutNewReview()
    {
        var root = Path.Combine(Path.GetTempPath(), "m0-ui-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        var entry = Path.Combine(root, "index.html"); var added = Path.Combine(root, "added.js");
        try
        {
            File.WriteAllText(entry, "ABC"); var files = new LocalUiFiles(root);
            Assert.True(files.IsCurrent()); Assert.Null(files.GetFileInfo("index.html").PhysicalPath);
            Assert.False(files.GetFileInfo("../outside.html").Exists);
            Assert.False(files.GetDirectoryContents("/").Exists);
            File.WriteAllText(entry, "XYZ"); Assert.False(files.IsCurrent());
            Assert.Throws<ApiErrorException>(() => files.GetFileInfo("index.html").CreateReadStream());
            File.WriteAllText(entry, "ABC"); Assert.True(files.IsCurrent());
            File.WriteAllText(added, "public sample"); Assert.False(files.IsCurrent()); Assert.False(files.GetFileInfo("added.js").Exists);
            File.Delete(added); Assert.True(files.IsCurrent());
            File.Delete(entry); Assert.False(files.IsCurrent());
        }
        finally { if (File.Exists(entry)) File.Delete(entry); if (File.Exists(added)) File.Delete(added); Directory.Delete(root); }
    }
    [Theory]
    [InlineData("report.docx")] [InlineData("notes.md")] [InlineData("source.map")] [InlineData("data.json")]
    public void NonUiFilesRefuseWholeArtifactSetBeforeServing(string name)
    {
        var root = Path.Combine(Path.GetTempPath(), "m0-ui-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        var entry = Path.Combine(root, "index.html"); var other = Path.Combine(root, name);
        try { File.WriteAllText(entry, "ABC"); File.WriteAllText(other, "public test sample"); Assert.Throws<InvalidOperationException>(() => new LocalUiFiles(root)); }
        finally { File.Delete(entry); File.Delete(other); Directory.Delete(root); }
    }
}

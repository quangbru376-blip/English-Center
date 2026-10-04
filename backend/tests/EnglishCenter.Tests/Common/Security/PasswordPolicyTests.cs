namespace EnglishCenter.Tests.Common.Security;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using Xunit;

public class PasswordPolicyTests
{
    private static PasswordPolicy Create()=>new(OfflinePasswordBlocklist.Load(OfflinePasswordBlocklistTests.SourceDirectory));
    [Theory]
    [InlineData(15),InlineData(128)]
    public void UnicodeCodePointBoundaries_AcceptSupplementaryCharacters(int count)
    {
        var raw=string.Concat(Enumerable.Repeat("😀",count));
        Assert.Same(raw,Create().Validate(raw));
    }
    [Theory]
    [InlineData(14),InlineData(129)]
    public void OutsideUnicodeBoundaries_IsSafeFieldError(int count)
    {
        var raw=string.Concat(Enumerable.Repeat("😀",count));
        var error=Assert.Throws<ApiErrorException>(()=>Create().Validate(raw));
        Assert.Equal(ApiErrorCode.VALIDATION_ERROR,error.ErrorCode);
        Assert.True(error.FieldErrors.ContainsKey("password"));
        Assert.DoesNotContain(raw,error.Message);
    }
    [Fact]
    public void NoCompositionTrimCaseOrNfc_IsAdded()
    {
        var policy=Create();
        foreach(var raw in new[]{new string(' ',15),"  A distinctive phrase 48275  ","a distinctive phrase 48275","é distinctive phrase 48275","é distinctive phrase 48275"})
            Assert.Same(raw,policy.Validate(raw));
    }
    [Theory]
    [InlineData(0),InlineData(1),InlineData(2),InlineData(3)]
    public void InvalidUtf16_IsRejectedWithoutNormalizing(int variant)
    {
        var raw=variant switch{0=>new string((char)0xd800,15),1=>new string((char)0xdc00,15),2=>"long safe phrase "+(char)0xd800, _=>"long safe "+(char)0xd800+"phrase"};
        Assert.Throws<ApiErrorException>(()=>Create().Validate(raw));
    }
    [Fact]
    public void RealListedLongPassword_IsBlockedButRawAlterationIsDistinct()
    {
        var blocklist=OfflinePasswordBlocklist.Load(OfflinePasswordBlocklistTests.SourceDirectory);
        var raw=File.ReadLines(Path.Combine(OfflinePasswordBlocklistTests.SourceDirectory,"10k-most-common.txt"))
            .First(s=>s.Length>=15&&s.Length<126&&!blocklist.Contains(" "+s+" "));
        var policy=new PasswordPolicy(blocklist);
        Assert.Throws<ApiErrorException>(()=>policy.Validate(raw));
        var spaced=" "+raw+" ";
        Assert.Same(spaced,policy.Validate(spaced));
    }
    [Fact]
    public void NullPasswordAndNullDependency_AreRejected()
    {
        Assert.Throws<ApiErrorException>(()=>Create().Validate(null));
        Assert.Throws<ArgumentNullException>(()=>new PasswordPolicy(null!));
    }
}


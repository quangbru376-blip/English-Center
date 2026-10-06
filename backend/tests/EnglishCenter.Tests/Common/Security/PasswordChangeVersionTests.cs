namespace EnglishCenter.Tests.Common.Security;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Services.Auth;
using Xunit;

public class PasswordChangeVersionTests
{
    [Fact]
    public void RowversionIsExactlyEightOpaqueBytesAndVersionCannotWrap()
    {
        var bytes = Enumerable.Range(0, 8).Select(i => (byte)i).ToArray(); Assert.True(PasswordChangeVersionGuard.Matches(bytes, bytes)); var different = bytes.ToArray(); different[7] ^= 1; Assert.False(PasswordChangeVersionGuard.Matches(bytes, different)); Assert.False(PasswordChangeVersionGuard.Matches(bytes.AsSpan(0, 7), bytes));
        Assert.Equal(2, PasswordChangeVersionGuard.Next(1)); Assert.Equal(long.MaxValue, PasswordChangeVersionGuard.Next(long.MaxValue - 1)); Assert.Null(PasswordChangeVersionGuard.Next(long.MaxValue)); Assert.Null(PasswordChangeVersionGuard.Next(0)); Assert.Null(PasswordChangeVersionGuard.Next(-1));
    }
    [Theory]
    [InlineData("{\"currentPassword\":\"raw\",\"newPassword\":\"new\",\"expectedVersions\":null}")]
    [InlineData("{\"currentPassword\":\"raw\",\"newPassword\":\"new\",\"expectedVersions\":{\"account\":\"AAAA\"}}")]
    [InlineData("{\"currentPassword\":\"raw\",\"newPassword\":\"new\",\"expectedVersions\":{\"account\":\"AAAAAAAAAAA=\",\"account\":\"AAAAAAAAAAA=\"}}")]
    [InlineData("{\"currentPassword\":\"\\ud800\",\"newPassword\":\"new\",\"expectedVersions\":{\"account\":\"AAAAAAAAAAA=\"}}")]
    public void StrictCredentialRequestRejectsMalformedNestedAndUnicode(string json) => Assert.Throws<ApiErrorException>(() => PasswordChangeInput.Parse(json));
}

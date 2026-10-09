namespace EnglishCenter.Tests.Common;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Administration;
using EnglishCenter.Api.Services.Catalogue;
using Xunit;

public sealed class M1InputTests
{
    [Theory]
    [InlineData("{\"fullName\":\"Tên\",\"email\":\"a@example.test\",\"role\":\"Student\"}")]
    [InlineData("{\"fullName\":\"Tên\",\"email\":\"a@example.test\",\"role\":\"Teacher\",\"password\":\"not allowed\"}")]
    [InlineData("{\"fullName\":\"Tên\",\"email\":\"a@example.test\",\"role\":\"Teacher\",\"role\":\"Admin\"}")]
    public void StaffCreate_RejectsStudentSecretsAndDuplicateKeys(string json)=>Assert.Throws<ApiErrorException>(()=>StaffAccountInput.Parse(8,0,Guid.NewGuid(),json,4096));
    [Theory]
    [InlineData("0")][InlineData("1.00")][InlineData("1000000000000000000")][InlineData("1e3")][InlineData("-1")]
    public void CourseMoney_IsExactPositiveBoundedString(string money)=>Assert.Throws<ApiErrorException>(()=>CourseInput.Parse(75,0,Guid.NewGuid(),"{\"code\":\"IELTS\",\"name\":\"IELTS\",\"basePrice\":\""+money+"\",\"status\":\"ACTIVE\"}",4096));
    [Fact]
    public void MissingKeyVersionAndUnexpectedFields_AreRejected()
    {
        Assert.Throws<ApiErrorException>(()=>StaffAccountInput.Parse(9,1,Guid.NewGuid(),"{}",4096));
        Assert.Throws<ApiErrorException>(()=>CourseInput.Parse(78,1,Guid.Empty,"{\"expectedVersions\":{\"course\":\"AAAAAAAAAAE=\"}}",4096));
        Assert.Throws<ApiErrorException>(()=>CourseInput.Parse(77,1,Guid.NewGuid(),"{\"name\":\"Name\",\"basePrice\":\"123\",\"code\":\"CHANGED\",\"expectedVersions\":{\"course\":\"AAAAAAAAAAE=\"}}",4096));
    }
    [Theory]
    [InlineData("{\"page\":1e0}")][InlineData("{\"pageSize\":101}")][InlineData("{\"page\":0}")][InlineData("{\"role\":null}")][InlineData("{\"sort\":\"sql\"}")]
    public void AccountSearch_RejectsMalformedOrUnapprovedParameters(string json)=>Assert.Throws<ApiErrorException>(()=>AccountSearch.Parse(json));
    [Fact]
    public void AccountSearch_BlankLiteralFiltersAndMaximumPageRemainBounded()
    {
        var search=AccountSearch.Parse("{\"name\":\" %_ \",\"email\":\" \",\"phone\":\"+84912345678\",\"page\":2147483647}");
        Assert.Equal("%_",search.Name);Assert.Null(search.Email);Assert.Equal("0912345678",search.Phone);Assert.Equal(int.MaxValue,search.Page);Assert.Equal(20,search.PageSize);
    }
}

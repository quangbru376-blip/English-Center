namespace EnglishCenter.Tests.Api.Identity;

using System.Text.Json;
using System.Security.Cryptography;
using EnglishCenter.Api.Api.Identity;
using EnglishCenter.Api.Services.Identity;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Common.Errors;
using Xunit;

public class SignupInputTests
{
    private static Dictionary<string, object?> Valid() => new() { ["fullName"] = "  Nguye\u0302\u0303n An  ", ["dateOfBirth"] = "2000-01-01", ["email"] = " A+tag@EXAMPLE.INVALID ", ["phone"] = "+84 912 345 678", ["password"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)), ["address"] = "   " };
    [Fact]
    public void ExactSignupShapeNormalizesOnlyNonsecretFieldsAndResponseIsOnlyOutcomeMessage()
    {
        var body = Valid(); var input = StudentSignupInput.Parse(JsonSerializer.Serialize(body));
        Assert.Equal("a+tag@example.invalid", input.Email); Assert.Equal("0912345678", input.Phone); Assert.Equal("Nguyễn An", input.FullName); Assert.Null(input.Address); Assert.True(input.Password == (string)body["password"]!);
        using var response = JsonDocument.Parse(JsonSerializer.Serialize(RegisterStudentResult.Created())); Assert.Equal(2, response.RootElement.EnumerateObject().Count()); Assert.Equal("CREATED", response.RootElement.GetProperty("outcome").GetString());
        Assert.Equal(nameof(StudentSignupInput), input.ToString());
    }
    [Theory]
    [InlineData("role", "Student")]
    [InlineData("fullName", null)]
    [InlineData("email", null)]
    [InlineData("phone", null)]
    [InlineData("dateOfBirth", "2000-02-30")]
    [InlineData("dateOfBirth", "2000-1-01")]
    [InlineData("address", 3)]
    [InlineData("password", 3)]
    [InlineData("phone", "123")]
    public void UnknownNullAndWrongTypesOrScalarShapesAreRejected(string field, object? value)
    {
        var body = Valid(); body[field] = value; Assert.Throws<ApiErrorException>(() => StudentSignupInput.Parse(JsonSerializer.Serialize(body)));
    }
    [Fact]
    public void DuplicateOrMissingFieldsAndLoginSelectorsNeverBind()
    {
        var body = Valid(); body.Remove("password"); Assert.Throws<ApiErrorException>(() => StudentSignupInput.Parse(JsonSerializer.Serialize(body)));
        Assert.Throws<ApiErrorException>(() => StudentSignupInput.Parse("{\"email\":\"a@example.invalid\",\"email\":\"b@example.invalid\"}"));
        Assert.Throws<ApiErrorException>(() => LoginInput.Parse("{\"email\":\"a@example.invalid\",\"password\":null}"));
        Assert.Throws<ApiErrorException>(() => LoginInput.Parse("{\"email\":\"a@example.invalid\",\"password\":\"x\",\"role\":\"Admin\"}"));
    }
}

namespace EnglishCenter.Api.Services.Identity;

using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Validation;

public sealed class StudentSignupInput
{
    public string FullName { get; }
    public DateOnly DateOfBirth { get; }
    public string Email { get; }
    public string Phone { get; }
    public string Password { get; }
    public string? Address { get; }
    private StudentSignupInput(string name, DateOnly birth, string email, string phone, string password, string? address)
    { FullName = name; DateOfBirth = birth; Email = email; Phone = phone; Password = password; Address = address; }
    public static StudentSignupInput Parse(string json)
    {
        var body = StrictJsonObject.Parse(json, ["fullName", "dateOfBirth", "email", "phone", "password", "address"], ["fullName", "dateOfBirth", "email", "phone", "password"]);
        return new(body.ReadParsed("fullName", s => NonsecretText.NormalizeRequired(s, 100)),
            body.ReadParsed("dateOfBirth", StrictScalars.ParseLocalDate),
            body.ReadParsed("email", ContactNormalizer.NormalizeEmail), body.ReadParsed("phone", ContactNormalizer.NormalizePhone),
            body.ReadString("password"), OptionalAddress(body));
    }
    private static string? OptionalAddress(StrictJsonObject body)
    {
        try { return NonsecretText.NormalizeOptional(body.ReadNullableString("address"), 255); }
        catch (ArgumentException) { throw ApiErrorException.Validation("address"); }
    }
    public override string ToString() => nameof(StudentSignupInput);
}

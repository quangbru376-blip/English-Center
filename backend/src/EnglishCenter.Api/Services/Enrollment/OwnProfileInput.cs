namespace EnglishCenter.Api.Services.Enrollment;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Validation;

public sealed class OwnProfileInput
{
    public string FullName { get; }
    public DateOnly DateOfBirth { get; }
    public string Phone { get; }
    public string? Address { get; }
    private readonly byte[] accountVersion, profileVersion;
    public byte[] CopyAccountVersion() => accountVersion.ToArray();
    public byte[] CopyProfileVersion() => profileVersion.ToArray();
    private OwnProfileInput(string name, DateOnly birth, string phone, string? address, byte[] account, byte[] profile)
    { FullName = name; DateOfBirth = birth; Phone = phone; Address = address; accountVersion = account; profileVersion = profile; }

    public static OwnProfileInput Parse(string json)
    {
        var fields = new[] { "fullName", "dateOfBirth", "phone", "address", "expectedVersions" };
        var body = StrictJsonObject.Parse(json, fields, fields);
        using var doc = JsonDocument.Parse(json);
        var versions = StrictJsonObject.Parse(doc.RootElement.GetProperty("expectedVersions").GetRawText(), ["account", "profile"], ["account", "profile"]);
        string? address;
        try { address = NonsecretText.NormalizeOptional(body.ReadNullableString("address"), 255); }
        catch (ArgumentException) { throw ApiErrorException.Validation("address"); }
        return new(body.ReadParsed("fullName", s => NonsecretText.NormalizeRequired(s, 100)),
            body.ReadParsed("dateOfBirth", StrictScalars.ParseLocalDate), body.ReadParsed("phone", ContactNormalizer.NormalizePhone), address,
            versions.ReadParsed("account", RowVersionCodec.Decode), versions.ReadParsed("profile", RowVersionCodec.Decode));
    }
    public override string ToString() => nameof(OwnProfileInput);
}

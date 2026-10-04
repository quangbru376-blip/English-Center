using EnglishCenter.Api.Common.Errors;

namespace EnglishCenter.Api.Common.Security;

public sealed class PasswordPolicy
{
    public const int MinCodePoints = 15;
    public const int MaxCodePoints = 128;

    private readonly OfflinePasswordBlocklist _blocklist;

    public PasswordPolicy(OfflinePasswordBlocklist blocklist)
    {
        _blocklist = blocklist ?? throw new ArgumentNullException(nameof(blocklist));
    }



    public string Validate(string? password)
    {
        if (password is null)
        {
            throw ApiErrorException.Validation("password");
        }

        int codePointCount = CountCodePointsAndValidateUtf16(password);
        if (codePointCount is < MinCodePoints or > MaxCodePoints)
        {
            throw ApiErrorException.Validation("password");
        }

        if (_blocklist.Contains(password))
        {
            throw ApiErrorException.Validation("password");
        }

        return password;
    }

    private static int CountCodePointsAndValidateUtf16(string password)
    {
        int codePointCount = 0;
        for (int i = 0; i < password.Length; i++)
        {
            char c = password[i];
            if (char.IsHighSurrogate(c))
            {
                if (i + 1 < password.Length && char.IsLowSurrogate(password[i + 1]))
                {
                    i++;
                }
                else
                {
                    throw ApiErrorException.Validation("password");
                }
            }
            else if (char.IsLowSurrogate(c))
            {
                throw ApiErrorException.Validation("password");
            }

            codePointCount++;
        }

        return codePointCount;
    }
}


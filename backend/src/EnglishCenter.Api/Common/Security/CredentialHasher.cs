using System.Buffers.Binary;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EnglishCenter.Api.Common.Security;

public sealed class CredentialHasher
{
    public const int IterationCount = 220000;
    public const int DecodedHashLength = 61;
    public const int EncodedHashLength = 84;
    public const int SaltLength = 16;
    public const int SubkeyLength = 32;
    public const int PrfHmacSha512 = 2;
    public const byte FormatMarkerV3 = 0x01;

    private static readonly object UserSentinel = new();

    private readonly PasswordPolicy _policy;
    private readonly PasswordHasher<object> _passwordHasher;

    public CredentialHasher(PasswordPolicy policy)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _passwordHasher = new PasswordHasher<object>(
            Options.Create(new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                IterationCount = IterationCount
            }));
    }

    public string Hash(string password)
    {
        string validatedPassword = _policy.Validate(password);
        return _passwordHasher.HashPassword(UserSentinel, validatedPassword);
    }

    public PasswordVerificationResult Verify(string? hash, string? password)
    {
        if (hash is null || password is null)
        {
            return PasswordVerificationResult.Failed;
        }

        if (!IsValidPassword(password))
        {
            return PasswordVerificationResult.Failed;
        }

        if (hash.Length != EncodedHashLength)
        {
            return PasswordVerificationResult.Failed;
        }

        byte[] decoded;
        try
        {
            decoded = Convert.FromBase64String(hash);
        }
        catch
        {
            return PasswordVerificationResult.Failed;
        }

        if (decoded.Length != DecodedHashLength)
        {
            return PasswordVerificationResult.Failed;
        }

        if (!string.Equals(Convert.ToBase64String(decoded), hash, StringComparison.Ordinal))
        {
            return PasswordVerificationResult.Failed;
        }

        if (decoded[0] != FormatMarkerV3)
        {
            return PasswordVerificationResult.Failed;
        }

        int prf = BinaryPrimitives.ReadInt32BigEndian(decoded.AsSpan(1, 4));
        if (prf != PrfHmacSha512)
        {
            return PasswordVerificationResult.Failed;
        }

        int iterations = BinaryPrimitives.ReadInt32BigEndian(decoded.AsSpan(5, 4));
        if (iterations <= 0 || iterations > IterationCount)
        {
            return PasswordVerificationResult.Failed;
        }

        int saltLength = BinaryPrimitives.ReadInt32BigEndian(decoded.AsSpan(9, 4));
        if (saltLength != SaltLength)
        {
            return PasswordVerificationResult.Failed;
        }

        try
        {
            return _passwordHasher.VerifyHashedPassword(UserSentinel, hash, password);
        }
        catch
        {
            return PasswordVerificationResult.Failed;
        }
    }

    private static bool IsValidPassword(string password)
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
                    return false;
                }
            }
            else if (char.IsLowSurrogate(c))
            {
                return false;
            }

            codePointCount++;
            if (codePointCount > PasswordPolicy.MaxCodePoints)
            {
                return false;
            }
        }

        return true;
    }
}


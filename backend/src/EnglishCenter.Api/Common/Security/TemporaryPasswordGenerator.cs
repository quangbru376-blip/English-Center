using System.Security.Cryptography;
using EnglishCenter.Api.Common.Errors;

namespace EnglishCenter.Api.Common.Security;

public sealed class TemporaryPasswordGenerator
{
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
    public const int PasswordLength = 32;
    public const int MaxAttempts = 64;

    private const string FailureMessage = "Failed to generate a valid temporary password.";

    private readonly PasswordPolicy _policy;

    public TemporaryPasswordGenerator(PasswordPolicy policy)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }

    public string Generate()
    {
        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            string candidate = string.Create(PasswordLength, Alphabet, static (buffer, alphabet) =>
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
                }
            });

            try
            {
                return _policy.Validate(candidate);
            }
            catch (ApiErrorException)
            {
                // Re-attempt if candidate violates policy or blocklist.
            }
        }

        throw new InvalidOperationException(FailureMessage);
    }
}

using System.Security.Cryptography;
using System.Text;

namespace EnglishCenter.Api.Common.Security;

public sealed class OfflinePasswordBlocklist
{
    public const string Top10kFileName = "10k-most-common.txt";
    public const string Top12kFileName = "probable-v2_top-12000.txt";
    public const string LicenseFileName = "LICENSE";

    public const string Top10kSha256 = "68782D6A4A19A4768D5F15DD66BD534E7A33055CC755411E33F16D18C50FDCCE";
    public const string Top12kSha256 = "EA4C906EBB0B26790C549A047962573F72CCC26F42212B83D70165D9C03FB72B";
    public const string LicenseSha256 = "3DBDC93D5F8829DE0941744841730A09C106D0732E5AE0E98CA1D77BE7DED66C";

    public const int Top10kRecordCount = 10001;
    public const int Top12kRecordCount = 12645;
    public const int MergedRecordCount = 15310;

    private const string FailureMessage = "Password blocklist corpus is missing, invalid, or tampered.";
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private readonly HashSet<string> _entries;

    private OfflinePasswordBlocklist(HashSet<string> entries)
    {
        _entries = entries;
    }

    public int Count => _entries.Count;

    public bool Contains(string? password) =>
        password is not null && _entries.Contains(password);

    public static OfflinePasswordBlocklist Load(string directory)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                throw new InvalidOperationException(FailureMessage);
            }

            ReadAndValidateBytes(directory, LicenseFileName, LicenseSha256);
            byte[] top10kBytes = ReadAndValidateBytes(directory, Top10kFileName, Top10kSha256);
            byte[] top12kBytes = ReadAndValidateBytes(directory, Top12kFileName, Top12kSha256);

            var top10kRecords = ParseRecords(top10kBytes);
            if (top10kRecords.Count != Top10kRecordCount)
            {
                throw new InvalidOperationException(FailureMessage);
            }

            var top12kRecords = ParseRecords(top12kBytes);
            if (top12kRecords.Count != Top12kRecordCount)
            {
                throw new InvalidOperationException(FailureMessage);
            }

            var merged = new HashSet<string>(top10kRecords.Count + top12kRecords.Count, StringComparer.Ordinal);
            for (int i = 0; i < top10kRecords.Count; i++)
            {
                merged.Add(top10kRecords[i]);
            }

            for (int i = 0; i < top12kRecords.Count; i++)
            {
                merged.Add(top12kRecords[i]);
            }

            if (merged.Count != MergedRecordCount)
            {
                throw new InvalidOperationException(FailureMessage);
            }

            return new OfflinePasswordBlocklist(merged);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InvalidOperationException(FailureMessage);
        }
    }

    private static byte[] ReadAndValidateBytes(string directory, string fileName, string expectedSha256Hex)
    {
        string filePath = Path.Combine(directory, fileName);
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException(FailureMessage);
        }

        byte[] bytes = File.ReadAllBytes(filePath);
        byte[] hash = SHA256.HashData(bytes);
        string actualHex = Convert.ToHexString(hash);

        if (!string.Equals(actualHex, expectedSha256Hex, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(FailureMessage);
        }

        return bytes;
    }

    private static List<string> ParseRecords(byte[] bytes)
    {
        string text = StrictUtf8.GetString(bytes);
        string[] rawLines = text.Split('\n');
        int count = rawLines.Length;
        if (count > 0 && rawLines[^1].Length == 0)
        {
            count--;
        }

        var records = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            string line = rawLines[i];
            if (line.EndsWith('\r'))
            {
                line = line[..^1];
            }

            records.Add(line);
        }

        return records;
    }
}

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public enum ProfileLoadFailure { None, InvalidSource, Unavailable, Malformed, InvalidProfile, Deadline }
public sealed class RuntimeProfileLoadResult
{
    internal RuntimeProfileLoadResult(ProfileLoadFailure failure, RuntimeProfile? profile = null,
        ProfileIssue issue = ProfileIssue.None) { Failure = failure; Profile = profile; Issue = issue; }
    public ProfileLoadFailure Failure { get; }
    public RuntimeProfile? Profile { get; }
    public ProfileIssue Issue { get; }
    public bool IsValid => Failure == ProfileLoadFailure.None;
    public override string ToString() => IsValid ? "profile_valid" : "profile_invalid";
}

// Loading descriptors does not establish dependency readiness or resolve secrets.
public sealed class RuntimeProfileReader
{
    private readonly string path;
    private readonly int maximumBytes;
    private readonly int deadlineMilliseconds;
    public RuntimeProfileReader(string path, int maximumBytes, int deadlineMilliseconds)
    { this.path = path; this.maximumBytes = maximumBytes; this.deadlineMilliseconds = deadlineMilliseconds; }

    public async Task<RuntimeProfileLoadResult> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (maximumBytes <= 0 || deadlineMilliseconds <= 0 || string.IsNullOrWhiteSpace(path) ||
            !Path.IsPathFullyQualified(path)) return new(ProfileLoadFailure.InvalidSource);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(deadlineMilliseconds);
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (directory is null || LocalReadinessFiles.Resolve(directory, Path.GetFileName(path)) is null)
                return new(ProfileLoadFailure.Unavailable);
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            if (stream.Length is < 1 || stream.Length > maximumBytes) return new(ProfileLoadFailure.Malformed);
            var bytes = new byte[(int)stream.Length];
            await stream.ReadExactlyAsync(bytes, deadline.Token);
            if (await stream.ReadAsync(new byte[1], deadline.Token) != 0) return new(ProfileLoadFailure.Malformed);
            var text = new UTF8Encoding(false, true).GetString(bytes);
            using var document = JsonDocument.Parse(text, new JsonDocumentOptions { MaxDepth = 8 });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !HasUniqueProperties(root) ||
                !root.TryGetProperty("kind", out var kind) || kind.ValueKind != JsonValueKind.String ||
                !Enum.TryParse<RuntimeProfileKind>(kind.GetString(), ignoreCase: false, out var parsedKind) ||
                !Enum.IsDefined(parsedKind) || parsedKind.ToString() != kind.GetString())
                return new(ProfileLoadFailure.Malformed);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                MaxDepth = 8
            };
            options.Converters.Add(new JsonStringEnumConverter<RuntimeProfileKind>(allowIntegerValues: false));
            var profile = root.Deserialize<RuntimeProfile>(options);
            var issue = RuntimeProfileValidator.Validate(profile);
            deadline.Token.ThrowIfCancellationRequested();
            return issue == ProfileIssue.None ? new(ProfileLoadFailure.None, profile) :
                new(ProfileLoadFailure.InvalidProfile, issue: issue);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (OperationCanceledException) when (deadline.IsCancellationRequested) { return new(ProfileLoadFailure.Deadline); }
        catch (JsonException) { cancellationToken.ThrowIfCancellationRequested(); return new(ProfileLoadFailure.Malformed); }
        catch (DecoderFallbackException) { cancellationToken.ThrowIfCancellationRequested(); return new(ProfileLoadFailure.Malformed); }
        catch (Exception)
        { cancellationToken.ThrowIfCancellationRequested(); return new(ProfileLoadFailure.Unavailable); }
    }
    private static bool HasUniqueProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
                if (!names.Add(property.Name) || !HasUniqueProperties(property.Value)) return false;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                if (!HasUniqueProperties(item)) return false;
        }
        return true;
    }
}

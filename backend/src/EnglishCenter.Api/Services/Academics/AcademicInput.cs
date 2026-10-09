namespace EnglishCenter.Api.Services.Academics;

using System.Globalization;
using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Validation;

public sealed class AcademicInput
{
    public int TargetId { get; }
    public Guid RequestId { get; }
    public CanonicalIntent Intent { get; }
    internal JsonElement Body { get; }
    private AcademicInput(int target, Guid request, CanonicalIntent intent)
    { TargetId = target; RequestId = request; Intent = intent; using var json = JsonDocument.Parse(intent.CopyBytes()); Body = json.RootElement.GetProperty("body").Clone(); }
    public static AcademicInput Parse(int api, int target, Guid request, string json, int bytes)
    {
        if (api is not (55 or 57 or 58 or 59 or 61 or 64 or 82 or 84 or 85 or 87 or 88 or 89) || request == Guid.Empty) throw ApiErrorException.Validation();
        var key = api < 80 ? "sessionId" : "programId";
        return new(target, request, CanonicalIntent.Bind(api, json, api is 55 or 82 ? new Dictionary<string, int>() : new() { { key, target } }, bytes));
    }
    public override string ToString() => nameof(AcademicInput);
}
public sealed class AcademicQuery
{
    private readonly IQueryCollection query;
    public AcademicQuery(IQueryCollection query, params string[] allowed)
    { if (query.Any(x => x.Value.Count != 1 || !allowed.Contains(x.Key))) throw ApiErrorException.Validation(); this.query = query; }
    public string? Get(string key) => query.TryGetValue(key, out var value) ? value[0] : null;
    public string? Text(string key, int max) { try { return Get(key) is { } raw ? NonsecretText.NormalizeRequired(raw, max) : null; } catch (ArgumentException) { throw ApiErrorException.Validation(key); } }
    public string Choice(string key, string fallback, params string[] values)
    { var value = Get(key) ?? fallback; if (!values.Contains(value)) throw ApiErrorException.Validation(key); return value; }
    public int Number(string key, int fallback = 1, int max = int.MaxValue)
    { var raw = Get(key); if (raw is null) return fallback; if (raw.Length == 0 || raw[0] == '0' || raw.Any(c => !char.IsAsciiDigit(c)) || !int.TryParse(raw, out var value) || value < 1 || value > max) throw ApiErrorException.Validation(key); return value; }
    public int? Id(string key) => Get(key) is null ? null : Number(key);
    public DateOnly? Date(string key) { try { return Get(key) is { } raw ? StrictScalars.ParseLocalDate(raw) : null; } catch (ArgumentException) { throw ApiErrorException.Validation(key); } }
}
public sealed record AcademicPage<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount);
public sealed record VersionedId(int Id, string Version);
public static class AcademicGuard
{
    public static void Version(JsonElement expected, byte[] actual)
    { if (!RowVersionCodec.Decode(expected.GetString()).AsSpan().SequenceEqual(actual)) throw Conflict(); }
    public static void Set(JsonElement expected, IEnumerable<int> actual)
    { if (!expected.EnumerateArray().Select(x => x.GetInt32()).Order().SequenceEqual(actual.Order())) throw Conflict(); }
    public static void Versions(JsonElement expected, IEnumerable<VersionedId> actual)
    { var supplied = expected.EnumerateArray().ToDictionary(x => x.GetProperty("id").GetInt32(), x => x.GetProperty("version").GetString()); var rows = actual.ToArray(); if (supplied.Count != rows.Length || rows.Any(x => !supplied.TryGetValue(x.Id, out var version) || version != x.Version)) throw Conflict(); }
    public static ApiErrorException Conflict() => new(ApiErrorCode.STATE_CONFLICT);
    public static string Instant(DateTime value) => value.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}

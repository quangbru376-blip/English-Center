namespace EnglishCenter.Api.Common.Replay;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;

public sealed class CanonicalIntent
{
    private readonly byte[] _bytes, _digest;
    public CanonicalAction Action { get; }
    private CanonicalIntent(CanonicalAction action, JsonObject body, JsonObject target)
    {
        Action = action;
        var envelope = new JsonObject { ["action"] = action.Code, ["body"] = body, ["contract"] = "EC-C1", ["target"] = target };
        using var stream = new MemoryStream(); Write(stream, envelope); _bytes = stream.ToArray(); _digest = SHA256.HashData(_bytes);
    }
    public byte[] CopyBytes() => _bytes.ToArray();
    public byte[] CopyDigest() => _digest.ToArray();
    public override string ToString() => nameof(CanonicalIntent);
    // apiId and route IDs are selected by the trusted adapter, never from the body.
    public static CanonicalIntent Bind(int apiId, string json, IReadOnlyDictionary<string, int> routeIds, int maxInputBytes)
    {
        if (maxInputBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxInputBytes));
        ArgumentNullException.ThrowIfNull(routeIds);
        try
        {
            var action = CanonicalActionRegistry.ForApi(apiId);
            if (new UTF8Encoding(false, true).GetByteCount(json) > maxInputBytes) throw new ApiErrorException(ApiErrorCode.REQUEST_TOO_LARGE);
            if (routeIds.Count != action.TargetKeys.Count || action.TargetKeys.Any(k => !routeIds.TryGetValue(k, out var id) || id <= 0)) throw ApiErrorException.Validation();
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 64, AllowTrailingCommas = false, CommentHandling = JsonCommentHandling.Disallow });
            var body = CanonicalSchemas.Normalize(apiId, doc.RootElement);
            var target = new JsonObject(); foreach (var key in action.TargetKeys) target[key] = routeIds[key];
            CanonicalBindings.Validate(apiId, body, target);
            return new(action, body, target);
        }
        catch (Exception ex) when (ex is ArgumentException or JsonException or InvalidOperationException or OverflowException) { throw ApiErrorException.Validation(); }
    }
    private static void Write(Stream output, JsonNode? node)
    {
        if (node is null) { Ascii(output, "null"); return; }
        if (node is JsonObject obj)
        {
            output.WriteByte((byte)'{'); var first = true;
            foreach (var (key, value) in obj.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                if (!first) output.WriteByte((byte)','); first = false; output.Write(C1StringEncoder.Encode(key)); output.WriteByte((byte)':'); Write(output, value);
            }
            output.WriteByte((byte)'}'); return;
        }
        if (node is JsonArray array)
        {
            output.WriteByte((byte)'['); for (var i = 0; i < array.Count; i++) { if (i > 0) output.WriteByte((byte)','); Write(output, array[i]); }
            output.WriteByte((byte)']'); return;
        }
        switch (node.GetValueKind())
        {
            case JsonValueKind.String: output.Write(C1StringEncoder.Encode(node.GetValue<string>())); break;
            case JsonValueKind.Number: Ascii(output, node.GetValue<int>().ToString(System.Globalization.CultureInfo.InvariantCulture)); break;
            case JsonValueKind.True: Ascii(output, "true"); break;
            case JsonValueKind.False: Ascii(output, "false"); break;
            default: throw new ArgumentException("Invalid normalized value.");
        }
    }
    private static void Ascii(Stream output, string value) => output.Write(Encoding.ASCII.GetBytes(value));
}

using System.Text;
using System.Text.Json;
using EnglishCenter.Api.Common.Errors;

namespace EnglishCenter.Api.Common.Serialization;

public sealed class StrictJsonObject
{
    private readonly Dictionary<string, JsonElement> _fields;
    private readonly HashSet<string> _allowed;

    private StrictJsonObject(Dictionary<string, JsonElement> fields, HashSet<string> allowed)
    {
        _fields = fields;
        _allowed = allowed;
    }

    public static StrictJsonObject Parse(string? json, IReadOnlyCollection<string> allowed, IReadOnlyCollection<string> required)
    {
        if (allowed is null) throw new ArgumentNullException(nameof(allowed));
        if (required is null) throw new ArgumentNullException(nameof(required));

        var allowedSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in allowed)
        {
            ValidateFieldName(name, nameof(allowed));
            if (!allowedSet.Add(name)) throw new ArgumentException($"Duplicate field name in allowed list: {name}", nameof(allowed));
        }

        var requiredSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in required)
        {
            ValidateFieldName(name, nameof(required));
            if (!allowedSet.Contains(name)) throw new ArgumentException($"Required field '{name}' is not in allowed list.", nameof(required));
            if (!requiredSet.Add(name)) throw new ArgumentException($"Duplicate field name in required list: {name}", nameof(required));
        }

        if (string.IsNullOrWhiteSpace(json)) throw ApiErrorException.Validation();

        byte[] utf8Bytes;
        try { utf8Bytes = new UTF8Encoding(false, true).GetBytes(json); }
        catch (EncoderFallbackException) { throw ApiErrorException.Validation(); }
        var reader = new Utf8JsonReader(utf8Bytes, new JsonReaderOptions
        {
            CommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = false,
            MaxDepth = 64
        });

        try
        {
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                throw ApiErrorException.Validation();

            var scopes = new Stack<HashSet<string>>();
            scopes.Push(new HashSet<string>(StringComparer.Ordinal));

            while (reader.Read())
            {
                if (scopes.Count == 0) throw ApiErrorException.Validation();

                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        scopes.Push(new HashSet<string>(StringComparer.Ordinal));
                        break;
                    case JsonTokenType.EndObject:
                        scopes.Pop();
                        break;
                    case JsonTokenType.PropertyName:
                        string name = reader.GetString()!;
                        if (!scopes.Peek().Add(name) || (scopes.Count == 1 && !allowedSet.Contains(name)))
                            throw ApiErrorException.Validation();
                        break;
                }
            }

            if (scopes.Count != 0) throw ApiErrorException.Validation();
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            throw ApiErrorException.Validation();
        }

        using var doc = JsonDocument.Parse(utf8Bytes, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Disallow,
            AllowTrailingCommas = false,
            MaxDepth = 64
        });

        var fields = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            fields.Add(prop.Name, prop.Value.Clone());
        }

        foreach (var req in required)
        {
            if (!fields.ContainsKey(req)) throw ApiErrorException.Validation(req);
        }

        return new StrictJsonObject(fields, allowedSet);
    }

    public bool Contains(string field)
    {
        ValidateDeclaredField(field);
        return _fields.ContainsKey(field);
    }

    public string ReadString(string field)
    {
        ValidateDeclaredField(field);
        if (!_fields.TryGetValue(field, out var element) || element.ValueKind != JsonValueKind.String)
            throw ApiErrorException.Validation(field);

        try { return element.GetString()!; }
        catch (InvalidOperationException) { throw ApiErrorException.Validation(field); }
    }

    public string? ReadNullableString(string field)
    {
        ValidateDeclaredField(field);
        if (!_fields.TryGetValue(field, out var element) || element.ValueKind == JsonValueKind.Null)
            return null;

        if (element.ValueKind != JsonValueKind.String)
            throw ApiErrorException.Validation(field);

        try { return element.GetString(); }
        catch (InvalidOperationException) { throw ApiErrorException.Validation(field); }
    }

    public bool ReadBoolean(string field)
    {
        ValidateDeclaredField(field);
        if (!_fields.TryGetValue(field, out var element))
            throw ApiErrorException.Validation(field);

        return element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw ApiErrorException.Validation(field)
        };
    }

    public int ReadInt32(string field, int min, int max)
    {
        if (min > max) throw new ArgumentException("Minimum value cannot be greater than maximum value.", nameof(min));
        ValidateDeclaredField(field);
        if (!_fields.TryGetValue(field, out var element)) throw ApiErrorException.Validation(field);

        try
        {
            return StrictScalars.ParseInt32(element, min, max);
        }
        catch (ArgumentException)
        {
            throw ApiErrorException.Validation(field);
        }
    }

    public T ReadParsed<T>(string field, Func<string?, T> parser)
    {
        if (parser is null) throw new ArgumentNullException(nameof(parser));
        string raw = ReadString(field);
        try
        {
            return parser(raw);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
        {
            throw ApiErrorException.Validation(field);
        }
    }

    private void ValidateDeclaredField(string field)
    {
        if (field is null || !_allowed.Contains(field))
            throw new ArgumentException($"Field '{field}' was not declared in allowed schema.", nameof(field));
    }

    private static void ValidateFieldName(string? name, string paramName)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Field name cannot be null or empty.", paramName);

        if (name.Length > 128 || name[0] is < 'a' or > 'z')
            throw new ArgumentException($"Field name '{name}' must be simple ASCII camelCase.", paramName);

        for (int i = 1; i < name.Length; i++)
        {
            if (!char.IsAsciiLetterOrDigit(name[i]))
                throw new ArgumentException($"Field name '{name}' must be simple ASCII camelCase.", paramName);
        }
    }
}

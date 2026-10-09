using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.Data.SqlClient;

namespace EnglishCenter.Api.Infrastructure.Operator;

public static class LocalRuntimeManifest
{
    public static RuntimePatchManifest Load(string root)
    {
        var path = LocalReadinessFiles.Resolve(root, "patch-manifest.json") ?? throw new InvalidOperationException("Unavailable manifest");
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length is < 1 or > 32768) throw new InvalidOperationException("Invalid manifest");
        var bytes = new byte[(int)stream.Length]; stream.ReadExactly(bytes);
        if (stream.ReadByte() != -1) throw new InvalidOperationException("Manifest changed");
        using var document = JsonDocument.Parse(new UTF8Encoding(false, true).GetString(bytes), new JsonDocumentOptions { MaxDepth = 8 });
        if (document.RootElement.ValueKind != JsonValueKind.Object || !Unique(document.RootElement))
            throw new InvalidOperationException("Invalid manifest");
        return document.RootElement.Deserialize<RuntimePatchManifest>(new JsonSerializerOptions
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, MaxDepth = 8 })
            ?? throw new InvalidOperationException("Invalid manifest");
    }

    private static bool Unique(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in value.EnumerateObject())
                if (!names.Add(item.Name) || !Unique(item.Value)) return false;
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (var item in value.EnumerateArray()) if (!Unique(item)) return false;
        return true;
    }

    public static async ValueTask<Version?> ReadSqlVersionAsync(CancellationToken token)
    {
        await using var connection = new SqlConnection(LocalRuntimeReferences.SqlConnection);
        await connection.OpenAsync(token);
        await using var command = connection.CreateCommand();
        command.CommandTimeout = LocalRuntimeReferences.Execution.SqlSeconds;
        command.CommandText = "SELECT CONVERT(varchar(32),SERVERPROPERTY('ProductVersion'))";
        return Version.TryParse(await command.ExecuteScalarAsync(token) as string, out var version) ? version : null;
    }
}

namespace EnglishCenter.Api.Common.Replay;

using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnglishCenter.Api.Common.Serialization;

public sealed class SafeCommandCompletion
{
    public string Outcome => "COMPLETED";
    public Guid OperationId { get; }
    public string ResultMode { get; }
    public IReadOnlyDictionary<string,int> Target { get; }
    private SafeCommandCompletion(Guid operation, string mode, IReadOnlyDictionary<string,int> target)
    { OperationId=operation; ResultMode=mode; Target=new ReadOnlyDictionary<string,int>(new Dictionary<string,int>(target,StringComparer.Ordinal)); }
    public static SafeCommandCompletion Create(CanonicalAction action, Guid operation, IReadOnlyDictionary<string,int> target)
    {
        ArgumentNullException.ThrowIfNull(action); ArgumentNullException.ThrowIfNull(target);
        if(operation==Guid.Empty)throw new ArgumentException("Invalid operation.");
        var required=action.ResultKeys.Where(k=>!k.EndsWith('*')).ToArray();
        var allowed=action.ResultKeys.Select(k=>k.TrimEnd('*')).ToArray();
        if(required.Any(k=>!target.ContainsKey(k)) || target.Any(p=>!allowed.Contains(p.Key,StringComparer.Ordinal)||p.Value<=0))
            throw new ArgumentException("Invalid completion references.");
        return new(operation,action.ResultMode,target);
    }
    public string EncodeReference()=>JsonSerializer.Serialize(new { outcome=Outcome,target=Target });
    public static SafeCommandCompletion Decode(CanonicalAction action, Guid operation, string mode, string reference)
    {
        if(mode!=action.ResultMode)throw new ArgumentException("Invalid completion mode.");
        var root=StrictJsonObject.Parse(reference,["outcome","target"],["outcome","target"]);
        if(root.ReadString("outcome")!="COMPLETED")throw new ArgumentException("Invalid completion outcome.");
        using var doc=JsonDocument.Parse(reference);
        var node=doc.RootElement.GetProperty("target");
        if(node.ValueKind!=JsonValueKind.Object)throw new ArgumentException("Invalid completion target.");
        var values=new Dictionary<string,int>(StringComparer.Ordinal);
        foreach(var p in node.EnumerateObject())values.Add(p.Name,StrictScalars.ParseInt32(p.Value,1,int.MaxValue));
        return Create(action,operation,values);
    }
    public override string ToString()=>nameof(SafeCommandCompletion);
}

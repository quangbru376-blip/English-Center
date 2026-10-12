namespace EnglishCenter.Api.Services.Administration;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Auditing;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Validation;
using EnglishCenter.Api.Domain.Administration;
using EnglishCenter.Api.Services.Academics;

public static class AuditViewerPolicy
{
    private static readonly IReadOnlyDictionary<string,(string Entity,string Fields)> Special=new Dictionary<string,(string,string)>(StringComparer.Ordinal)
    {
        ["SYSTEM.BAND.SEED"]=("Band","bandCode minScore"),["SYSTEM.ADMIN.BOOTSTRAP"]=("Account","roleId status mustChangePassword"),
        ["STUDENT.ACCOUNT.CREATE"]=("Account","roleId status mustChangePassword"),["AUTH.PASSWORD_CHANGE"]=("Account","mustChangePassword securityVersion revokedSessionCount"),
        ["STUDENT.PROFILE.UPDATE"]=("StudentProfile",""),["AUTH.LOGIN"]=("Account",""),["AUTH.LOGOUT"]=("Account",""),["AUTH.ACCESS_DENIED"]=("Account",""),["AUTH.THROTTLED"]=("Account","")
    };
    public static bool Accepts(string action,string entity)
    {
        if(Special.TryGetValue(action,out var special))return special.Entity==entity;
        var effect=action.EndsWith(".EFFECT",StringComparison.Ordinal);var key=effect?action[..^7]:action;
        return AuditPolicy.Actions.TryGetValue(key,out var rule)&&(effect?rule.Entities.Split(' ').Contains(entity,StringComparer.Ordinal):rule.RootEntity==entity);
    }
    public static bool Action(string action)=>Special.ContainsKey(action)||AuditPolicy.Actions.ContainsKey(action)||(action.EndsWith(".EFFECT",StringComparison.Ordinal)&&AuditPolicy.Actions.ContainsKey(action[..^7]));
    public static bool Entity(string entity)=>Special.Values.Any(x=>x.Entity==entity)||AuditPolicy.Actions.Values.Any(x=>x.Entities.Split(' ').Contains(entity,StringComparer.Ordinal));
    public static bool Reference(string? value)=>value is null||value.Length>0&&value[0]!='0'&&value.All(char.IsAsciiDigit)&&int.TryParse(value,NumberStyles.None,CultureInfo.InvariantCulture,out var id)&&id>0;
    public static AuditEventSummary Summary(AuditLog row)=>new(row.AuditId,AcademicGuard.Instant(row.OccurredAt),row.ActorAccountId,row.Action,row.EntityType,Reference(row.EntityId)?row.EntityId:null,row.Result,row.OperationId);
    public static AuditEventDetail Detail(AuditLog row,DateTime now)
    {
        var summary=Summary(row);var changes=new List<AuditFieldChange>();string? reason=null;
        try
        {
            if(row.Details is not null&&JsonNode.Parse(row.Details,new JsonNodeOptions(),new JsonDocumentOptions{MaxDepth=16}) is JsonObject details)
            {
                var fields=Special.TryGetValue(row.Action,out var special)?special.Fields:AuditPolicy.ViewerFields(row.Action,row.EntityType);
                var old=Read(details["old"] as JsonObject,fields,row.EntityType);var next=Read(details["new"] as JsonObject,fields,row.EntityType);
                foreach(var field in old.Select(x=>x.Key).Union(next.Select(x=>x.Key),StringComparer.Ordinal).Order(StringComparer.Ordinal))changes.Add(new(field,old[field]?.DeepClone(),next[field]?.DeepClone()));
                var canonical=row.Action.EndsWith(".EFFECT",StringComparison.Ordinal)?row.Action[..^7]:row.Action;
                if(AuditPolicy.Actions.TryGetValue(canonical,out var rule)&&rule.RequiresReason&&details["reason"] is JsonValue text&&text.TryGetValue<string>(out var raw))reason=NonsecretText.NormalizeRequired(raw,1000);
            }
        }
        catch(JsonException){}catch(ArgumentException){}
        var ip=row.IpExpiresAt is {} expires&&expires>now&&row.IpAddress is {Length:<=45} address&&IPAddress.TryParse(address,out _)?address:null;
        return new(summary.Id,summary.OccurredAt,summary.ActorId,summary.Action,summary.EntityType,summary.EntityId,summary.Result,summary.OperationId,reason,changes,ip);
    }
    private static JsonObject Read(JsonObject? source,string fields,string entity)
    {
        var result=new JsonObject();if(source is null)return result;
        foreach(var field in fields.Split(' ',StringSplitOptions.RemoveEmptyEntries))
        {
            if(!source.TryGetPropertyValue(field,out var node))continue;
            try
            {
                var one=AuditPolicy.ProjectFields(new JsonObject{[field]=node?.DeepClone()},field,allowPlannedClass:entity=="Class");
                if(one[field] is JsonValue value&&field is "score" or "scoreValue" or "finalScore" or "minScore" or "weight" or "basePrice" or "amount" or "amountPaid")
                {var number=value.GetValue<decimal>();one[field]=field is "basePrice" or "amount" or "amountPaid"?ExactNumbers.FormatMoney(number):field=="weight"?ExactNumbers.FormatWeight(number):ExactNumbers.FormatScore(number);}
                result[field]=one[field]?.DeepClone();
            }
            catch(ArgumentException){}catch(InvalidOperationException){}catch(FormatException){}
        }
        return result;
    }
}

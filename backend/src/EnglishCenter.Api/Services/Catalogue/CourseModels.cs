namespace EnglishCenter.Api.Services.Catalogue;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Validation;
using EnglishCenter.Api.Domain.Academics;

public sealed class CourseInput
{
    public int TargetId { get; }
    public Guid RequestId { get; }
    public CanonicalIntent Intent { get; }
    internal JsonElement Body { get; }
    private CourseInput(int target,Guid request,CanonicalIntent intent)
    {TargetId=target;RequestId=request;Intent=intent;using var doc=JsonDocument.Parse(intent.CopyBytes());Body=doc.RootElement.GetProperty("body").Clone();}
    public static CourseInput Parse(int api,int target,Guid request,string json,int maxBytes)
    {
        if(api is not (75 or 77 or 78 or 79)||request==Guid.Empty)throw ApiErrorException.Validation();
        return new(target,request,CanonicalIntent.Bind(api,json,api==75?new Dictionary<string,int>():new(){{"courseId",target}},maxBytes));
    }
}
public sealed record CourseView(int Id,string Code,string Name,string BasePrice,string Status,string Version,
    IReadOnlyList<string> AllowedActions,IReadOnlyDictionary<string,string> DisabledReasons)
{
    internal static CourseView Map(Course c)=>new(c.CourseId,c.CourseCode,c.CourseName,ExactNumbers.FormatMoney(c.BasePrice),c.Status,
        RowVersionCodec.Encode(c.RowVersion),["EDIT_DETAILS",c.Status=="ACTIVE"?"DEACTIVATE":"ACTIVATE"],
        new Dictionary<string,string>{{c.Status=="ACTIVE"?"ACTIVATE":"DEACTIVATE","Trạng thái không cho phép thao tác."}});
}
public sealed record CourseResult(CourseView Course,Guid OperationId)
{[System.Text.Json.Serialization.JsonIgnore] public bool Replayed { get; init; }}
public sealed record CoursePage(IReadOnlyList<CourseView> Items,int Page,int PageSize,int TotalCount);
public sealed record BandView(int Id,string Code,string MinScore,string Version);
public sealed record BandSet(IReadOnlyList<BandView> Items,bool IsThresholdSetFrozen);
public sealed record CourseSearch(string? Code,string? Name,string? Status,string Sort,int Page,int PageSize)
{
    public static CourseSearch Parse(IQueryCollection query)
    {
        var allowed=new[]{"code","name","status","sort","page","pageSize"};
        if(query.Any(p=>!allowed.Contains(p.Key)||p.Value.Count!=1))throw ApiErrorException.Validation();
        string? Get(string key)=>query.TryGetValue(key,out var values)?values[0]:null;
        string? Text(string key,int max)
        {var raw=Get(key);if(raw is null)return null;try{return NonsecretText.NormalizeRequired(raw,max);}catch(ArgumentException){throw ApiErrorException.Validation(key);}}
        var code=Text("code",30)?.ToUpperInvariant();if(code?.Any(c=>!char.IsAsciiLetterUpper(c)&&!char.IsAsciiDigit(c)&&c is not ('_' or '-'))==true)throw ApiErrorException.Validation("code");
        var status=Get("status");if(status is not null&&status is not ("ACTIVE" or "INACTIVE"))throw ApiErrorException.Validation("status");
        var sort=Get("sort")??"CODE_ASC";if(sort is not ("CODE_ASC" or "CODE_DESC"))throw ApiErrorException.Validation("sort");
        int Number(string key,int fallback,int max){var raw=Get(key);if(raw is null)return fallback;if(raw.Length==0||raw[0]=='0'||raw.Any(c=>!char.IsAsciiDigit(c))||!int.TryParse(raw,out var n)||n<1||n>max)throw ApiErrorException.Validation(key);return n;}
        return new(code,Text("name",150),status,sort,Number("page",1,int.MaxValue),Number("pageSize",20,100));
    }
}

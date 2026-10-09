namespace EnglishCenter.Api.Services.Administration;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Validation;

public sealed class StaffAccountInput
{
    public int TargetId { get; }
    public Guid RequestId { get; }
    public CanonicalIntent Intent { get; }
    internal JsonElement Body { get; }
    private StaffAccountInput(int target, Guid request, CanonicalIntent intent)
    {
        TargetId=target; RequestId=request; Intent=intent;
        using var doc=JsonDocument.Parse(intent.CopyBytes()); Body=doc.RootElement.GetProperty("body").Clone();
    }
    public static StaffAccountInput Parse(int api,int target,Guid request,string json,int maxBytes)
    {
        if(api is not (8 or 9 or 11 or 12 or 13)||request==Guid.Empty)throw ApiErrorException.Validation();
        return new(target,request,CanonicalIntent.Bind(api,json,api==8?new Dictionary<string,int>():new(){{"accountId",target}},maxBytes));
    }
    public override string ToString()=>nameof(StaffAccountInput);
}
public sealed record AccountSearch(string? Name,string? Email,string? Phone,string? Role,string? Status,int Page,int PageSize)
{
    public static AccountSearch Parse(string json)
    {
        var obj=StrictJsonObject.Parse(json,["name","email","phone","role","status","page","pageSize"],[]);
        using var fields=JsonDocument.Parse(json);
        string? Text(string field,int max){try{return NonsecretText.NormalizeOptional(fields.RootElement.TryGetProperty(field,out _)?obj.ReadString(field):null,max);}catch(ArgumentException){throw ApiErrorException.Validation(field);}}
        var email=Text("email",254);if(email?.Any(c=>c>127||char.IsControl(c))==true)throw ApiErrorException.Validation("email");
        var phone=Text("phone",30);
        try{if(phone is not null)phone=ContactNormalizer.NormalizePhone(phone);}catch(ArgumentException){throw ApiErrorException.Validation("phone");}
        var role=Text("role",30);var status=Text("status",10);
        if(role is not null&&!Common.Security.FixedRoleCodes.All.Contains(role)||status is not null&&status is not ("ACTIVE" or "LOCKED" or "DISABLED"))throw ApiErrorException.Validation();
        using var doc=JsonDocument.Parse(json);
        return new(Text("name",100),email,phone,role,status,
            doc.RootElement.TryGetProperty("page",out _)?obj.ReadInt32("page",1,int.MaxValue):1,
            doc.RootElement.TryGetProperty("pageSize",out _)?obj.ReadInt32("pageSize",1,100):20);
    }
    public override string ToString()=>nameof(AccountSearch);
}
public sealed record AccountPage(IReadOnlyList<AccountAdminView> Items,int Page,int PageSize,int TotalCount);
// Avoid record-generated ToString exposing a first-response credential.
public sealed class CredentialIssueResult
{
    public int AccountId { get; init; }
    public string Email { get; init; }="";
    public string Role { get; init; }="";
    public string TemporaryPassword { get; init; }="";
    public string TemporaryPasswordExpiresAt { get; init; }="";
    public Guid OperationId { get; init; }
    public string AccountVersion { get; init; }="";
    public override string ToString()=>nameof(CredentialIssueResult);
}

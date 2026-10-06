namespace EnglishCenter.Api.Services.Administration;

using System.Collections.ObjectModel;
using System.Globalization;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Domain.Identity;

public sealed record AccountAdminView(int Id,string FullName,string Email,string? Phone,string Role,string Status,
    string CreatedAt,bool MustChangePassword,string? TemporaryPasswordExpiresAt,string Version,
    IReadOnlyList<string> AllowedActions,IReadOnlyDictionary<string,string> DisabledReasons);
public sealed record AdminAccountCommandResult(AccountAdminView Account,Guid OperationId);
internal static class AdminAccountProjection
{
    internal static AccountAdminView Map(Account account,string role,int actor,int activeAdmins)
    {
        var allowed=new List<string>();var disabled=new Dictionary<string,string>(StringComparer.Ordinal);
        foreach(var action in new[]{"RESET_PASSWORD","LOCK","UNLOCK","DISABLE","RESTORE"})
        {
            var ok=action switch {
                "RESET_PASSWORD"=>true,
                "LOCK"=>AdminLockGuard.CanLock(actor,account.AccountId,role=="Admin",activeAdmins,account.Status),
                "UNLOCK"=>account.Status=="LOCKED",
                "DISABLE"=>account.Status is "ACTIVE" or "LOCKED" && account.AccountId!=actor && !(role=="Admin"&&account.Status=="ACTIVE"&&activeAdmins<2),
                "RESTORE"=>account.Status=="DISABLED",_=>false };
            if(ok)allowed.Add(action);else disabled.Add(action,"Trạng thái hoặc bảo vệ tài khoản không cho phép thao tác.");
        }
        return new(account.AccountId,account.FullName,account.Email,account.Phone,role,account.Status,Instant(account.CreatedAt),
            account.MustChangePassword,account.TempExpiresAt is { } temp?Instant(temp):null,RowVersionCodec.Encode(account.RowVersion),
            allowed.AsReadOnly(),new ReadOnlyDictionary<string,string>(disabled));
    }
    private static string Instant(DateTime at)=>at.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'",CultureInfo.InvariantCulture);
}

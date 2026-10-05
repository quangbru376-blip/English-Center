namespace EnglishCenter.Api.Common.Security;

using Microsoft.AspNetCore.Http;

public static class SafeCookiePolicy
{
    public const string AuthCookieName="__Host-ec-auth";
    public const string CsrfCookieName="__Host-ec-csrf";
    public static CookieOptions AuthOptions() => new() { HttpOnly=true,Secure=true,SameSite=SameSiteMode.Strict,Path="/",IsEssential=true };
}

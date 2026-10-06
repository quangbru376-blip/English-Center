namespace EnglishCenter.Api.Api.Auth;

public static class LogoutRequestShape
{
    public static bool IsEmpty(long? contentLength, bool canHaveBody, int queryCount)
    {
        return queryCount == 0 && !canHaveBody && (contentLength is null or 0);
    }
}

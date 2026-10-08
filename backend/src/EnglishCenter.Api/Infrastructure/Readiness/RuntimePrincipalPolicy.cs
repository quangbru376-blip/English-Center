namespace EnglishCenter.Api.Infrastructure.Readiness;

public static class RuntimePrincipalPolicy
{
    public static bool IsSafe(
        bool? isRuntime,
        bool? isDbo,
        bool? isSysadmin,
        bool? controlsDatabase,
        bool? controlsServer,
        bool? hasExtraDatabaseRole,
        bool? hasDirectEffectiveGrant)
    {
        return isRuntime == true
            && isDbo == false
            && isSysadmin == false
            && controlsDatabase == false
            && controlsServer == false
            && hasExtraDatabaseRole == false
            && hasDirectEffectiveGrant == false;
    }
}

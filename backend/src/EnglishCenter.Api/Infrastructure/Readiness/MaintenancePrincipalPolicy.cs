namespace EnglishCenter.Api.Infrastructure.Readiness;

public static class MaintenancePrincipalPolicy
{
    public static bool IsSafe(
        bool? isDbo,
        bool? isSysadmin,
        bool? controlsDatabase,
        bool? controlsServer,
        bool? isRuntime,
        bool? isDdlAdmin,
        bool? canDeleteAudit,
        bool? canDeleteReplay,
        bool? canClearIp)
    {
        return isDbo == false
            && isSysadmin == false
            && controlsDatabase == false
            && controlsServer == false
            && isRuntime == false
            && isDdlAdmin == false
            && canDeleteAudit == true
            && canDeleteReplay == true
            && canClearIp == true;
    }
}

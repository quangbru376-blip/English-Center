using System;

namespace EnglishCenter.Api.Common.Security;

/// <summary>
/// Pure precondition guard for administrative account locking.
/// </summary>
public static class AdminLockGuard
{
    private const string ActiveStatus = "ACTIVE";

    public static bool CanLock(
        int actorId,
        int targetId,
        bool targetIsAdmin,
        int activeAdminCount,
        string? currentStatus)
    {
        if (actorId <= 0 || targetId <= 0 || actorId == targetId)
        {
            return false;
        }

        if (!string.Equals(currentStatus, ActiveStatus, StringComparison.Ordinal))
        {
            return false;
        }

        if (activeAdminCount < 0)
        {
            return false;
        }

        if (targetIsAdmin && activeAdminCount < 2)
        {
            return false;
        }

        return true;
    }
}

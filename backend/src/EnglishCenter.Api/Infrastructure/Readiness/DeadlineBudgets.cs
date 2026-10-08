namespace EnglishCenter.Api.Infrastructure.Readiness;

public static class DeadlineBudgets
{
    public static bool IsValid(int lockMs, int sqlSeconds, int requestMs, int csvMs, int readinessMs)
    {
        if (lockMs <= 0 || sqlSeconds <= 0 || requestMs <= 0 || csvMs <= 0 || readinessMs <= 0)
        {
            return false;
        }

        long sqlMsLong = (long)sqlSeconds * 1000L;
        if (sqlMsLong > int.MaxValue)
        {
            return false;
        }

        int sqlMs = (int)sqlMsLong;

        return lockMs < sqlMs
            && sqlMs < requestMs
            && lockMs < requestMs
            && csvMs <= requestMs
            && readinessMs <= requestMs;
    }

    public static bool CanAcquireLock(int lockMs, int remainingSqlMs, int remainingRequestMs)
    {
        if (lockMs <= 0 || remainingSqlMs <= 0 || remainingRequestMs <= 0)
        {
            return false;
        }

        return lockMs < remainingSqlMs && lockMs < remainingRequestMs;
    }
}

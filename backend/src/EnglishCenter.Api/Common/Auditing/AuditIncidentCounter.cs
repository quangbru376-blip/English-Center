using System;
using System.Threading;

namespace EnglishCenter.Api.Common.Auditing;

public enum SecurityAuditKind
{
    Login = 0,
    Logout = 1,
    AccessDenied = 2,
    Throttled = 3
}

public sealed class AuditIncidentCounter
{
    private const int CounterCount = 4;
    private readonly long[] _counters = new long[CounterCount];

    public AuditIncidentCounter()
    {
    }

    public void RecordFailure(SecurityAuditKind kind)
    {
        ValidateKind(kind);

        int index = (int)kind;
        long current = Volatile.Read(ref _counters[index]);

        while (true)
        {
            if (current == long.MaxValue)
            {
                return;
            }

            long next = current + 1;
            long original = Interlocked.CompareExchange(ref _counters[index], next, current);
            if (original == current)
            {
                return;
            }

            current = original;
        }
    }

    public long GetFailureCount(SecurityAuditKind kind)
    {
        ValidateKind(kind);
        return Volatile.Read(ref _counters[(int)kind]);
    }

    private static void ValidateKind(SecurityAuditKind kind)
    {
        var index = (int)kind;
        if (index < 0 || index >= CounterCount)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), "Invalid security audit kind.");
        }
    }
}

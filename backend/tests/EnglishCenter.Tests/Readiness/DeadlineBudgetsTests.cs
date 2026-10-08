using EnglishCenter.Api.Infrastructure.Readiness;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class DeadlineBudgetsTests
{
    [Theory]
    [InlineData(1, 1, 1001, 1001, 1001, true)]
    [InlineData(0, 1, 1001, 1001, 1001, false)]
    [InlineData(1, 0, 1001, 1001, 1001, false)]
    [InlineData(1, 1, 0, 1001, 1001, false)]
    [InlineData(1, 1, 1001, 0, 1001, false)]
    [InlineData(1, 1, 1001, 1001, 0, false)]
    [InlineData(-1, 1, 1001, 1001, 1001, false)]
    [InlineData(1000, 1, 1001, 1001, 1001, false)]
    [InlineData(1, 1, 1000, 1000, 1000, false)]
    [InlineData(1, 1, 1001, 1002, 1001, false)]
    [InlineData(1, 1, 1001, 1001, 1002, false)]
    [InlineData(1, int.MaxValue, int.MaxValue, 1, 1, false)]
    [InlineData(1, 2147484, int.MaxValue, 1, 1, false)]
    [InlineData(1, 2147483, int.MaxValue, int.MaxValue, int.MaxValue, true)]
    public void FiniteOrderedBudgetsRejectEqualityOverflowAndMissingValues(
        int locked, int sql, int request, int csv, int readiness, bool expected)
        => Assert.Equal(expected, DeadlineBudgets.IsValid(locked, sql, request, csv, readiness));

    [Theory]
    [InlineData(1, 2, 2, true)] [InlineData(1, 1, 2, false)] [InlineData(1, 2, 1, false)]
    [InlineData(1, 0, 2, false)] [InlineData(1, 2, -1, false)] [InlineData(0, 2, 2, false)]
    [InlineData(int.MaxValue, int.MaxValue, int.MaxValue, false)]
    public void LockUsesRemainingRatherThanOriginalBudgets(int locked, int sql, int request, bool expected)
        => Assert.Equal(expected, DeadlineBudgets.CanAcquireLock(locked, sql, request));
}

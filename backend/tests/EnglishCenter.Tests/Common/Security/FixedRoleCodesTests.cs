namespace EnglishCenter.Tests.Common.Security;

using EnglishCenter.Api.Common.Security;
using Xunit;

public class FixedRoleCodesTests
{
    [Fact]
    public void FixedCodesRejectDuplicatesMissingExtrasAndCaseAndCannotBeMutated()
    {
        Assert.Equal(new[]{"Student","Admissions","AcademicAffairs","Teacher","Accountant","Director","Admin"},FixedRoleCodes.All);
        Assert.True(FixedRoleCodes.MatchesExact(FixedRoleCodes.All.Reverse()));
        Assert.False(FixedRoleCodes.MatchesExact(FixedRoleCodes.All.Concat(["Admin"])));
        Assert.False(FixedRoleCodes.MatchesExact(FixedRoleCodes.All.Skip(1)));
        Assert.False(FixedRoleCodes.MatchesExact(FixedRoleCodes.All.Select(s=>s=="Admin"?"admin":s)));
        Assert.False(FixedRoleCodes.MatchesExact(FixedRoleCodes.All.Concat([null!])));
        Assert.Throws<NotSupportedException>(()=>((IList<string>)FixedRoleCodes.All)[0]="changed");
    }
}

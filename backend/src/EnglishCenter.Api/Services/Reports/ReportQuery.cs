namespace EnglishCenter.Api.Services.Reports;
using System.Globalization;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Academics;

public sealed record ReportQuery(int Api,DateOnly From,DateOnly To,DateTime FromUtc,DateTime ToExclusiveUtc,int? Course,int? Band,int? Class,int? Teacher,string? Status,bool OverdueOnly,int? Page,int? PageSize)
{
    public string Role=>Api is 150 or 154?"Accountant":"Director";
    public static ReportQuery Parse(int api,IQueryCollection input,DateTimeOffset queriedAt)
    {
        if(api is <150 or >154)throw ApiErrorException.Validation();
        var keys=api==153?new[]{"courseId","bandId","classId","teacherId","status","finalizedFromDate","finalizedToDate","page","pageSize"}:api==152?new[]{"courseId","status","fromDate","toDate","page","pageSize"}:api==154?new[]{"courseId","fromDate","toDate","overdueOnly"}:new[]{"courseId","fromDate","toDate","overdueOnly","page","pageSize"};
        var q=new AcademicQuery(input,keys);var fromKey=api==153?"finalizedFromDate":"fromDate";var toKey=api==153?"finalizedToDate":"toDate";
        var from=q.Date(fromKey);var to=q.Date(toKey);if((from is null)!=(to is null))throw ApiErrorException.Validation(from is null?fromKey:toKey);
        var today=DateOnly.FromDateTime(queriedAt.UtcDateTime.AddHours(7));from??=new(today.Year,today.Month,1);to??=from.Value.AddMonths(1).AddDays(-1);
        DateTime lo,hi;try{if(from>to)throw ApiErrorException.Validation(toKey);lo=from.Value.ToDateTime(TimeOnly.MinValue).AddHours(-7);hi=to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(-7);}catch(ArgumentOutOfRangeException){throw ApiErrorException.Validation(toKey);}
        string? status=null;if(api==152&&q.Get("status") is not null)status=q.Choice("status","","PENDING","TESTING","BAND_ASSIGNED","CONFIRMED","ADMITTED","REJECTED","CANCELLED");if(api==153&&q.Get("status") is not null)status=q.Choice("status","","PLANNED","OPEN","ONGOING","COMPLETED","CANCELLED");
        var overdue=false;if(api is 150 or 151 or 154&&q.Get("overdueOnly") is {} flag){if(flag is not("true" or "false"))throw ApiErrorException.Validation("overdueOnly");overdue=flag=="true";}
        return new(api,from.Value,to.Value,lo,hi,q.Id("courseId"),api==153?q.Id("bandId"):null,api==153?q.Id("classId"):null,api==153?q.Id("teacherId"):null,status,overdue,api==154?null:q.Number("page"),api==154?null:q.Number("pageSize",20,100));
    }
    public static string Date(DateOnly date)=>date.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);
}

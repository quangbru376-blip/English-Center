namespace EnglishCenter.Api.Services.Reports;
using System.Globalization;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Services.Classes;
using Row=System.Collections.Generic.IReadOnlyDictionary<string,object?>;

public static class ReportProjection
{
    private static ApiErrorException Integrity()=>new(ApiErrorCode.INTERNAL_ERROR);
    public static int Count(object? value)=>value switch{long n when n>=0&&n<=int.MaxValue=>(int)n,int n when n>=0=>n,_=>throw Integrity()};
    private static int N(Row r,string name)=>Count(r[name]);
    private static int Id(Row r,string name){var id=N(r,name);return id>0?id:throw Integrity();}
    private static string S(Row r,string name)=>r[name] is string s?s:throw Integrity();
    public static string Amount(object? value){if(value is not string s||s.Length==0||s.Length>38||s.Length>1&&s[0]=='0'||s.Any(c=>!char.IsAsciiDigit(c)))throw Integrity();return s;}
    public static string? Percent(object? value,int numerator,int denominator,bool applicable=true)
    {
        if(!applicable||denominator==0){if(value is not null)throw Integrity();return null;}
        if(numerator>denominator||value is not decimal d||d<0||d>100||decimal.Round(100m*numerator/denominator,2,MidpointRounding.AwayFromZero)!=d)throw Integrity();return d.ToString("F2",CultureInfo.InvariantCulture);
    }
    private static JsonObject Metrics(Row r)=>ClassWire.Object(new{revenueAmount=Amount(r["revenueAmount"]),billedAmount=Amount(r["billedAmount"]),currentDebtAmount=Amount(r["currentDebtAmount"]),overdueDebtAmount=Amount(r["overdueDebtAmount"])});
    private static JsonObject Course(Row r)=>ClassWire.Object(new{id=Id(r,"courseId"),code=S(r,"courseCode"),name=S(r,"courseName")});
    private static JsonObject Band(Row r)=>ClassWire.Object(new{id=Id(r,"bandId"),code=S(r,"bandCode"),minScore=r["bandMinScore"] is decimal d?ExactNumbers.FormatScore(d):throw Integrity()});
    private static JsonObject Results(Row r){var total=N(r,"finalizedResultCount");var pass=N(r,"passingResultCount");if(pass>total)throw Integrity();return ClassWire.Object(new{finalizedResultCount=total,passingResultCount=pass,passRatePercent=Percent(r["passRatePercent"],pass,total)});}
    private static JsonObject Page(IEnumerable<JsonObject> items,ReportQuery q,int count)=>ClassWire.Object(new{items,page=q.Page,pageSize=q.PageSize,totalCount=count});
    private static JsonObject Group(Row r)=>ClassWire.Object(new{course=Course(r),band=Band(r),@class=new{id=Id(r,"classId"),code=S(r,"classCode")},status=S(r,"classStatus")});
    private static JsonArray Courses(IReadOnlyList<Row> rows)=>ClassWire.Array(rows.Select(r=>{var c=Course(r);var status=S(r,"status");if(status is not("ACTIVE" or "INACTIVE"))throw Integrity();c["status"]=status;return c;}));
    public static JsonObject Render(ReportQuery q,DateTimeOffset at,IReadOnlyList<IReadOnlyList<Row>> sets)
    {
        var totals=sets[0].Single();var count=N(totals,"totalCount");var rows=sets[1];if(rows.Count>count||q.PageSize is {} size&&rows.Count>size)throw Integrity();var options=new JsonObject{["courses"]=Courses(sets[2])};
        CheckFilter(q.Course,sets[2],"courseId");
        var result=ClassWire.Object(new{queriedAt=ClassWire.At(at.UtcDateTime),todayVn=ReportQuery.Date(DateOnly.FromDateTime(at.UtcDateTime.AddHours(7))),options});
        if(q.Api==153){
            CheckFilter(q.Band,sets[3],"bandId");CheckFilter(q.Class,sets[4],"classId");CheckFilter(q.Teacher,sets[5],"teacherId");
            options["bands"]=ClassWire.Array(sets[3].Select(r=>ClassWire.Object(new{id=Id(r,"bandId"),code=S(r,"bandCode"),minScore=r["minScore"] is decimal d?ExactNumbers.FormatScore(d):throw Integrity()})));
            options["classes"]=ClassWire.Array(sets[4].Select(r=>ClassWire.Object(new{id=Id(r,"classId"),code=S(r,"classCode"),courseId=Id(r,"courseId"),bandId=Id(r,"bandId")})));
            options["teachers"]=ClassWire.Array(sets[5].Select(r=>ClassWire.Object(new{id=Id(r,"teacherId"),displayName=S(r,"displayName")})));
            result["appliedFilters"]=ClassWire.Object(new{courseId=q.Course,bandId=q.Band,classId=q.Class,teacherId=q.Teacher,status=q.Status,finalizedFromDate=ReportQuery.Date(q.From),finalizedToDate=ReportQuery.Date(q.To)});
            var states=new JsonObject{["PLANNED"]=N(totals,"plannedCount"),["OPEN"]=N(totals,"openCount"),["ONGOING"]=N(totals,"ongoingCount"),["COMPLETED"]=N(totals,"completedCount"),["CANCELLED"]=N(totals,"cancelledCount")};var classCount=N(totals,"classCount");if(classCount!=count||states.Sum(x=>(long)x.Value!.GetValue<int>())!=classCount)throw Integrity();
            var inventory=rows.Select(r=>{
                var cancelled=S(r,"classStatus")=="CANCELLED";if(S(r,"classStatus") is not("PLANNED" or "OPEN" or "ONGOING" or "COMPLETED" or "CANCELLED"))throw Integrity();var expected=N(r,"expectedCount");var max=N(r,"maxSize");var placed=N(r,"placedCount");if(expected==0||max is <1 or >15||placed>max)throw Integrity();
                int? held=cancelled?null:N(r,"heldCount");int? current=cancelled?null:N(r,"examCurrentCount");int? examHeld=cancelled?null:N(r,"examHeldCount");if(cancelled&&(r["heldCount"] is not null||r["examCurrentCount"] is not null||r["examHeldCount"] is not null)||current>1||examHeld>current)throw Integrity();
                return ClassWire.Object(new{group=Group(r),maxSize=max,placedCount=placed,lessons=new{expectedCount=expected,heldCount=held,progressPercent=Percent(r["progressPercent"],held??0,expected,!cancelled)},finalExam=new{currentCount=current,heldCount=examHeld}});
            }).ToArray();
            result["inventory"]=ClassWire.Object(new{totals=new{classCount,classCountsByState=states},groups=Page(inventory,q,count)});result["results"]=ClassWire.Object(new{totals=Results(totals),groups=Page(rows.Select(r=>ClassWire.Object(new{group=Group(r),metrics=Results(r)})),q,count)});
        }else{
            result["appliedFilters"]=q.Api==152?ClassWire.Object(new{fromDate=ReportQuery.Date(q.From),toDate=ReportQuery.Date(q.To),courseId=q.Course,status=q.Status}):ClassWire.Object(new{fromDate=ReportQuery.Date(q.From),toDate=ReportQuery.Date(q.To),courseId=q.Course,overdueOnly=q.OverdueOnly});
            JsonObject Admissions(Row r){var registrations=N(r,"registrationCount");var distinct=N(r,"distinctStudentCount");if(distinct>registrations)throw Integrity();return ClassWire.Object(new{registrationCount=registrations,distinctStudentCount=distinct});}
            result["totals"]=q.Api==152?Admissions(totals):Metrics(totals);result["groups"]=Page(rows.Select(r=>ClassWire.Object(new{course=Course(r),metrics=q.Api==152?Admissions(r):Metrics(r)})),q,count);
        }
        result["options"]=options.DeepClone();return result;
    }
    private static void CheckFilter(int? id,IReadOnlyList<Row> rows,string field){if(id is {} value&&!rows.Any(r=>Id(r,field)==value))throw new ApiErrorException(ApiErrorCode.NOT_FOUND);}
}

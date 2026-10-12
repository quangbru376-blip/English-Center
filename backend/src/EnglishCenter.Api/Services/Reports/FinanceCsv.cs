namespace EnglishCenter.Api.Services.Reports;
using System.Text;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
public sealed record FinanceCsvFile(byte[] Bytes,int DataRows,string FileName,DateTimeOffset QueriedAt,ReportQuery Query);
public static class FinanceCsv
{
    public const int MaxDataRows=10000;public const int MaxBodyBytes=5000000;
    private static readonly string[] Header=["Mã khóa","Tên khóa","Doanh thu trong kỳ (VND)","Phải thu trong kỳ (VND)","Công nợ hiện tại (VND)","Công nợ quá hạn (VND)"];
    public static string Cell(string value){var first=value.AsSpan().TrimStart();var unsafeText=value.IndexOfAny(['\t','\r','\n'])>=0||first.Length>0&&"=+-@＝＋－＠".Contains(first[0]);return "\""+(unsafeText?"'":"")+value.Replace("\"","\"\"")+"\"";}
    public static FinanceCsvFile Create(JsonObject report,ReportQuery query,DateTimeOffset at)
    {
        var groups=report["groups"]!["items"]!.AsArray();if(groups.Count+1>MaxDataRows||report["groups"]!["totalCount"]!.GetValue<int>()!=groups.Count)throw new ApiErrorException(ApiErrorCode.CSV_EXPORT_LIMIT);
        using var bytes=new MemoryStream();bytes.Write([0xef,0xbb,0xbf]);
        void Record(string[] cells){var line=string.Join(',',cells.Select(Cell))+"\r\n";var length=Encoding.UTF8.GetByteCount(line);if(bytes.Length+length>MaxBodyBytes)throw new ApiErrorException(ApiErrorCode.CSV_EXPORT_LIMIT);bytes.Write(Encoding.UTF8.GetBytes(line));}
        string[] Values(string code,string name,JsonNode metrics)=>[code,name,ReportProjection.Amount(metrics["revenueAmount"]!.GetValue<string>()),ReportProjection.Amount(metrics["billedAmount"]!.GetValue<string>()),ReportProjection.Amount(metrics["currentDebtAmount"]!.GetValue<string>()),ReportProjection.Amount(metrics["overdueDebtAmount"]!.GetValue<string>())];
        Record(Header);foreach(var row in groups)Record(Values(row!["course"]!["code"]!.GetValue<string>(),row["course"]!["name"]!.GetValue<string>(),row["metrics"]!));Record(Values("","Tổng toàn bộ tập lọc",report["totals"]!));
        return new(bytes.ToArray(),groups.Count+1,"finance-"+at.UtcDateTime.ToString("yyyyMMdd-HHmmss")+".csv",at,query);
    }
}

namespace EnglishCenter.Tests;
using System.Text;
using System.Text.Json.Nodes;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Classes;
using EnglishCenter.Api.Services.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;
public sealed class M4ReportsTests
{
    private static DateTimeOffset At=>new(2026,10,31,17,0,0,TimeSpan.Zero);
    private static ReportQuery Query(string raw="",int api=154)=>ReportQuery.Parse(api,new QueryCollection(QueryHelpers.ParseQuery(raw)),At);
    private static JsonObject Metrics()=>ClassWire.Object(new{revenueAmount="99999999999999999999999999999999999999",billedAmount="0",currentDebtAmount="1",overdueDebtAmount="0"});
    private static JsonObject Report(int rows,string name="")=>ClassWire.Object(new{groups=new{items=Enumerable.Range(1,rows).Select(i=>new{course=new{id=i,code="C",name},metrics=Metrics()}),totalCount=rows},totals=Metrics()});
    [Theory][InlineData("?page=1")][InlineData("?pageSize=20")][InlineData("?fromDate=2026-10-01")][InlineData("?courseId=0")][InlineData("?courseId=01")][InlineData("?courseId=1&courseId=2")][InlineData("?overdueOnly=True")][InlineData("?overdueOnly=1")][InlineData("?overdueOnly=")][InlineData("?fromDate=0001-01-01&toDate=2026-10-01")][InlineData("?fromDate=2026-10-01&toDate=9999-12-31")][InlineData("?fromDate=2026-11-01&toDate=2026-10-01")]
    public void CsvQueryStrictUnknownRepeatedBlankAndDateBoundary(string raw)=>Assert.Equal(ApiErrorCode.VALIDATION_ERROR,Assert.Throws<ApiErrorException>(()=>Query(raw)).Code);
    [Fact]public void DefaultPeriodUsesOneVietnamClockAndExactUtcHalfOpenRange(){var q=Query();Assert.Equal(new DateOnly(2026,11,1),q.From);Assert.Equal(new DateOnly(2026,11,30),q.To);Assert.Equal(new DateTime(2026,10,31,17,0,0),q.FromUtc);Assert.Equal(new DateTime(2026,11,30,17,0,0),q.ToExclusiveUtc);Assert.Null(q.Page);Assert.Null(q.PageSize);}
    [Theory][InlineData(" =SUM(1,2)","\"' =SUM(1,2)\"")][InlineData("＋1","\"'＋1\"")][InlineData("ab\tcd","\"'ab\tcd\"")][InlineData("a,b\"c","\"a,b\"\"c\"")][InlineData("Tên Việt","\"Tên Việt\"")][InlineData("x\ny","\"'x\ny\"")]
    public void CsvQuotesAllTextAndNeutralizesFormulaWithoutChangingInput(string input,string expected)=>Assert.Equal(expected,FinanceCsv.Cell(input));
    [Fact]public void CsvBOMCRLFSixColumnsExactMoneyAndWholeTotalsFooter(){var file=FinanceCsv.Create(Report(1,"a,b\"c"),Query(),At);Assert.Equal(new byte[]{0xef,0xbb,0xbf},file.Bytes[..3]);var text=Encoding.UTF8.GetString(file.Bytes[3..]);Assert.Contains("\"a,b\"\"c\"",text);Assert.EndsWith("\"\",\"Tổng toàn bộ tập lọc\",\"99999999999999999999999999999999999999\",\"0\",\"1\",\"0\"\r\n",text);Assert.Equal(2,file.DataRows);Assert.Equal("finance-20261031-170000.csv",file.FileName);}
    [Fact]public void CsvLogicalRecordCapCountsFooterAcceptsExactAndRejectsOneExtra(){Assert.Equal(10000,FinanceCsv.Create(Report(9999),Query(),At).DataRows);Assert.Equal(ApiErrorCode.CSV_EXPORT_LIMIT,Assert.Throws<ApiErrorException>(()=>FinanceCsv.Create(Report(10000),Query(),At)).Code);Assert.Equal(1,FinanceCsv.Create(Report(0),Query(),At).DataRows);}
    [Fact]public void CsvExactUtf8ByteCapIncludesBomHeaderFooterMultibyteAndRejectsPlusOne(){var overhead=FinanceCsv.Create(Report(1,"ấ"),Query(),At).Bytes.Length;var padding=new string('x',FinanceCsv.MaxBodyBytes-overhead);var exact=FinanceCsv.Create(Report(1,"ấ"+padding),Query(),At);Assert.Equal(FinanceCsv.MaxBodyBytes,exact.Bytes.Length);Assert.Equal(ApiErrorCode.CSV_EXPORT_LIMIT,Assert.Throws<ApiErrorException>(()=>FinanceCsv.Create(Report(1,"ấ"+padding+"x"),Query(),At)).Code);}
    [Theory][InlineData("-1")][InlineData("01")][InlineData("1.0")][InlineData("1e3")][InlineData("")][InlineData("999999999999999999999999999999999999999")]
    public void AggregateMoneyCannotBeRoundedTruncatedOrReformatted(string value)=>Assert.Equal(ApiErrorCode.INTERNAL_ERROR,Assert.Throws<ApiErrorException>(()=>ReportProjection.Amount(value)).Code);
    [Fact]public void CountAndPercentageIntegrityRejectOverflowAndDistinguishNoDataFromZero(){Assert.Equal(int.MaxValue,ReportProjection.Count((long)int.MaxValue));Assert.Throws<ApiErrorException>(()=>ReportProjection.Count((long)int.MaxValue+1));Assert.Null(ReportProjection.Percent(null,0,0));Assert.Equal("0.00",ReportProjection.Percent(0m,0,2));Assert.Equal("66.67",ReportProjection.Percent(66.67m,2,3));Assert.Throws<ApiErrorException>(()=>ReportProjection.Percent(66.66m,2,3));}
}

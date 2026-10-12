namespace EnglishCenter.Api.Infrastructure.Reports;
using System.Data;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Reports;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class SqlReportRepository(EnglishCenterDbContext db,int seconds)
{
    public async Task<IReadOnlyList<IReadOnlyList<IReadOnlyDictionary<string,object?>>>> Read(ReportQuery q,DateOnly today,CancellationToken ct)
    {
        await using var command=db.Database.GetDbConnection().CreateCommand();command.Transaction=db.Database.CurrentTransaction!.GetDbTransaction();command.CommandTimeout=seconds;command.CommandType=CommandType.StoredProcedure;
        command.CommandText=q.Api==152?"dbo.usp_GetAdmissionsReport":q.Api==153?"dbo.usp_GetTrainingReport":"dbo.usp_GetFinancialReport";
        void Parameter(string key,SqlDbType type,object? value,int size=0){var p=new SqlParameter(key,type){Value=value??DBNull.Value};if(size>0)p.Size=size;command.Parameters.Add(p);}
        Parameter("@courseId",SqlDbType.Int,q.Course);Parameter("@page",SqlDbType.Int,q.Page);Parameter("@pageSize",SqlDbType.Int,q.PageSize);
        Parameter(q.Api==153?"@finalizedFromUtc":"@fromUtc",SqlDbType.DateTime2,q.FromUtc);Parameter(q.Api==153?"@finalizedToExclusiveUtc":"@toExclusiveUtc",SqlDbType.DateTime2,q.ToExclusiveUtc);
        if(q.Api==153){Parameter("@bandId",SqlDbType.Int,q.Band);Parameter("@classId",SqlDbType.Int,q.Class);Parameter("@teacherId",SqlDbType.Int,q.Teacher);Parameter("@status",SqlDbType.VarChar,q.Status,16);}
        else if(q.Api==152)Parameter("@status",SqlDbType.VarChar,q.Status,16);
        else{Parameter("@overdueOnly",SqlDbType.Bit,q.OverdueOnly);Parameter("@todayVn",SqlDbType.Date,today.ToDateTime(TimeOnly.MinValue));}
        var result=new List<IReadOnlyList<IReadOnlyDictionary<string,object?>>>();await using var reader=await command.ExecuteReaderAsync(ct);
        do{
            var rows=new List<IReadOnlyDictionary<string,object?>>();while(await reader.ReadAsync(ct)){
                var row=new Dictionary<string,object?>(StringComparer.Ordinal);for(var i=0;i<reader.FieldCount;i++)row.Add(reader.GetName(i),reader.IsDBNull(i)?null:reader.GetValue(i));rows.Add(row);
                if(q.Api==154&&result.Count==0&&row.TryGetValue("totalCount",out var count)&&count is long n&&n>=FinanceCsv.MaxDataRows)throw new ApiErrorException(ApiErrorCode.CSV_EXPORT_LIMIT);
                if(q.Api==154&&result.Count==1&&rows.Count>=FinanceCsv.MaxDataRows)throw new ApiErrorException(ApiErrorCode.CSV_EXPORT_LIMIT);
            }result.Add(rows);
        }while(await reader.NextResultAsync(ct));
        if(result.Count!=(q.Api==153?6:3)||result[0].Count!=1)throw new ApiErrorException(ApiErrorCode.INTERNAL_ERROR);return result;
    }
}

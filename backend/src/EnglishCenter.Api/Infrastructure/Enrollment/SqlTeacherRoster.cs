namespace EnglishCenter.Api.Infrastructure.Enrollment;

using System.Data;
using System.Globalization;
using EnglishCenter.Api.Infrastructure.Academics;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Enrollment;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

internal static class SqlTeacherRoster
{
    public static async Task<AcademicPage<TeacherRosterRow>> Read(EnglishCenterDbContext db, int session, int actor, DateTimeOffset now, SessionRosterFilter filter, CancellationToken ct)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.Transaction = db.Database.CurrentTransaction!.GetDbTransaction(); command.CommandType = CommandType.StoredProcedure;
        command.CommandText = "dbo.usp_GetEntryTestRoster"; command.CommandTimeout = db.Database.GetCommandTimeout() ?? 10;
        void P(string name, SqlDbType type, object? value, int size = 0) => command.Parameters.Add(new SqlParameter(name, type, size) { Value = value ?? DBNull.Value });
        P("@sessionId", SqlDbType.Int, session); P("@actorId", SqlDbType.Int, actor); P("@asOfUtc", SqlDbType.DateTime2, now.UtcDateTime);
        P("@name", SqlDbType.NVarChar, filter.Name, 200); P("@email", SqlDbType.VarChar, filter.Email, 254); P("@phone", SqlDbType.VarChar, filter.Phone, 10);
        P("@bookingStatus", SqlDbType.VarChar, filter.Status, 16); P("@view", SqlDbType.VarChar, filter.View, 8); P("@sort", SqlDbType.VarChar, filter.Sort, 16); P("@page", SqlDbType.Int, filter.Page); P("@pageSize", SqlDbType.Int, filter.PageSize);
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) throw new InvalidOperationException("Missing teacher roster count.");
        var count = reader.GetInt64(0); if (!await reader.NextResultAsync(ct)) throw new InvalidOperationException("Missing teacher roster projection.");
        var columns = new[] { "bookingId", "attemptId", "registrationId", "studentId", "courseId", "attemptNumber", "bookedAt", "bookingStatus", "registrationStatus", "fullName", "email", "phone", "courseCode", "courseName", "courseStatus", "targetScore", "targetGradedAt", "bandId", "bandCode", "bandMinScore" };
        if (reader.FieldCount != columns.Length || columns.Where((name, i) => reader.GetName(i) != name).Any()) throw new InvalidOperationException("Unexpected teacher roster projection.");
        var rows = new List<TeacherRosterRow>();
        while (await reader.ReadAsync(ct)) rows.Add(new(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(5), AcademicGuard.Instant(reader.GetDateTime(6)), reader.GetString(7), reader.GetString(8),
            new(reader.GetInt32(3), reader.GetString(9), reader.GetString(10), reader.GetString(11)), new(reader.GetInt32(4), reader.GetString(12), reader.GetString(13), reader.GetString(14)),
            reader.IsDBNull(15) ? null : reader.GetDecimal(15).ToString("0.00", CultureInfo.InvariantCulture), reader.IsDBNull(16) ? null : AcademicGuard.Instant(reader.GetDateTime(16)),
            reader.IsDBNull(17) ? null : new AcademicBand(reader.GetInt32(17), reader.GetString(18), reader.GetDecimal(19).ToString("0.00", CultureInfo.InvariantCulture))));
        return new(rows, filter.Page, filter.PageSize, count);
    }
}

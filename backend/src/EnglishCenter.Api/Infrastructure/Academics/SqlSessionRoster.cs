namespace EnglishCenter.Api.Infrastructure.Academics;

using System.Data;
using System.Globalization;
using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Validation;
using EnglishCenter.Api.Services.Academics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed record SessionRosterFilter(string? Name, string? Email, string? Phone, string? Status, string View, string Sort, int Page, int PageSize)
{
    public static SessionRosterFilter Parse(string json)
    {
        using var document = JsonDocument.Parse(json); var body = document.RootElement;
        var allowed = new[] { "name", "email", "phone", "bookingStatus", "view", "sort", "page", "pageSize" };
        if (body.ValueKind != JsonValueKind.Object || body.EnumerateObject().Any(x => !allowed.Contains(x.Name)) || body.EnumerateObject().GroupBy(x => x.Name).Any(x => x.Count() > 1)) throw ApiErrorException.Validation();
        string? Text(string key, int max) { if (!body.TryGetProperty(key, out var value)) return null; if (value.ValueKind != JsonValueKind.String) throw ApiErrorException.Validation(key); var raw = value.GetString()!; if (string.IsNullOrWhiteSpace(raw)) return null; try { return NonsecretText.NormalizeRequired(raw, max); } catch (ArgumentException) { throw ApiErrorException.Validation(key); } }
        string Choice(string key, string fallback, params string[] choices) { var value = Text(key, 32) ?? fallback; if (!choices.Contains(value)) throw ApiErrorException.Validation(key); return value; }
        int Number(string key, int fallback, int max) { if (!body.TryGetProperty(key, out var value)) return fallback; if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var n) || n < 1 || n > max) throw ApiErrorException.Validation(key); return n; }
        var status = Text("bookingStatus", 16); if (status is not null && !new[] { "SCHEDULED", "COMPLETED", "ABSENT", "CANCELLED" }.Contains(status)) throw ApiErrorException.Validation("bookingStatus");
        var email = Text("email", 254); if (email is not null && email.Any(c => c > 127)) throw ApiErrorException.Validation("email");
        var phone = Text("phone", 30); try { if (phone is not null) phone = ContactNormalizer.NormalizePhone(phone); } catch (ArgumentException) { throw ApiErrorException.Validation("phone"); }
        return new(Text("name", 200), email, phone, status, Choice("view", "current", "current", "history"), Choice("sort", "BOOKED_ASC", "BOOKED_ASC", "BOOKED_DESC"), Number("page", 1, int.MaxValue), Number("pageSize", 20, 100));
    }
}
internal static class SqlSessionRoster
{
    public static async Task<AcademicPage<AcademicRosterRow>> Read(EnglishCenterDbContext db, int id, int actor, DateTimeOffset now, SessionRosterFilter filter, CancellationToken ct)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand(); command.Transaction = db.Database.CurrentTransaction!.GetDbTransaction(); command.CommandText = "dbo.usp_GetEntryTestRoster"; command.CommandType = CommandType.StoredProcedure; command.CommandTimeout = db.Database.GetCommandTimeout() ?? 10;
        void Parameter(string name, SqlDbType type, object? value, int size = 0) { command.Parameters.Add(new SqlParameter(name, type, size) { Value = value ?? DBNull.Value }); }
        Parameter("@sessionId", SqlDbType.Int, id); Parameter("@actorId", SqlDbType.Int, actor); Parameter("@asOfUtc", SqlDbType.DateTime2, now.UtcDateTime);
        Parameter("@name", SqlDbType.NVarChar, filter.Name, 200); Parameter("@email", SqlDbType.VarChar, filter.Email, 254); Parameter("@phone", SqlDbType.VarChar, filter.Phone, 10); Parameter("@bookingStatus", SqlDbType.VarChar, filter.Status, 16); Parameter("@view", SqlDbType.VarChar, filter.View, 8); Parameter("@sort", SqlDbType.VarChar, filter.Sort, 16); Parameter("@page", SqlDbType.Int, filter.Page); Parameter("@pageSize", SqlDbType.Int, filter.PageSize);
        await using var reader = await command.ExecuteReaderAsync(ct); if (!await reader.ReadAsync(ct)) throw new InvalidOperationException("Missing roster count."); var count = reader.GetInt64(0); if (!await reader.NextResultAsync(ct)) throw new InvalidOperationException("Missing roster projection.");
        var expected = new[] { "bookingId", "attemptId", "registrationId", "studentId", "courseId", "attemptNumber", "bookedAt", "bookingStatus", "registrationStatus", "fullName", "email", "phone", "courseCode", "courseName", "courseStatus", "targetScore", "targetGradedAt", "bandId", "bandCode", "bandMinScore", "invoiceStatus" };
        if (reader.FieldCount != expected.Length || Enumerable.Range(0, reader.FieldCount).Any(i => reader.GetName(i) != expected[i])) throw new InvalidOperationException("Unexpected roster projection.");
        var rows = new List<AcademicRosterRow>();
        while (await reader.ReadAsync(ct)) rows.Add(new(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(5), AcademicGuard.Instant(reader.GetDateTime(6)), reader.GetString(7), reader.GetString(8), new(reader.GetInt32(3), reader.GetString(9), reader.GetString(10), reader.GetString(11)), new(reader.GetInt32(4), reader.GetString(12), reader.GetString(13), reader.GetString(14)), reader.IsDBNull(15) ? null : reader.GetDecimal(15).ToString("0.00", CultureInfo.InvariantCulture), reader.IsDBNull(16) ? null : AcademicGuard.Instant(reader.GetDateTime(16)), reader.IsDBNull(17) ? null : new(reader.GetInt32(17), reader.GetString(18), reader.GetDecimal(19).ToString("0.00", CultureInfo.InvariantCulture)), reader.IsDBNull(20) ? null : reader.GetString(20)));
        return new(rows, filter.Page, filter.PageSize, count);
    }
}

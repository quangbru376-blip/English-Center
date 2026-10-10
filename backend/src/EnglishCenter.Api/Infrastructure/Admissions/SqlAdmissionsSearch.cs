namespace EnglishCenter.Api.Infrastructure.Admissions;

using System.Data;
using System.Globalization;
using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Common.Validation;
using EnglishCenter.Api.Services.Academics;
using EnglishCenter.Api.Services.Admissions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public sealed record AdmissionsSearchFilter(string? Name, string? Email, string? Phone, int? CourseId, string? Status,
    DateTime? FromUtc, DateTime? ToExclusiveUtc, string Sort, int Page, int PageSize)
{
    public static AdmissionsSearchFilter Parse(string json)
    {
        using var doc = JsonDocument.Parse(json); var body = doc.RootElement;
        var allowed = new[] { "name", "email", "phone", "courseId", "status", "createdFromDate", "createdToDate", "sort", "page", "pageSize" };
        if (body.ValueKind != JsonValueKind.Object || body.EnumerateObject().Any(x => !allowed.Contains(x.Name)) || body.EnumerateObject().GroupBy(x => x.Name).Any(x => x.Count() > 1)) throw ApiErrorException.Validation();
        string? Text(string key, int max)
        {
            if (!body.TryGetProperty(key, out var value)) return null;
            if (value.ValueKind != JsonValueKind.String) throw ApiErrorException.Validation(key);
            var raw = value.GetString()!; if (string.IsNullOrWhiteSpace(raw)) return null;
            try { return NonsecretText.NormalizeRequired(raw, max); } catch (ArgumentException) { throw ApiErrorException.Validation(key); }
        }
        int Number(string key, int fallback, int max)
        {
            if (!body.TryGetProperty(key, out var value)) return fallback;
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var n) || n < 1 || n > max) throw ApiErrorException.Validation(key);
            return n;
        }
        DateOnly? Date(string key)
        {
            if (!body.TryGetProperty(key, out var value)) return null;
            if (value.ValueKind != JsonValueKind.String) throw ApiErrorException.Validation(key);
            try { return StrictScalars.ParseLocalDate(value.GetString()); } catch (ArgumentException) { throw ApiErrorException.Validation(key); }
        }
        var from = Date("createdFromDate"); var to = Date("createdToDate");
        if (from > to) throw ApiErrorException.Validation("createdToDate");
        var status = Text("status", 32); if (status is not null && !new[] { "PENDING", "TESTING", "BAND_ASSIGNED", "CONFIRMED", "ADMITTED", "REJECTED", "CANCELLED" }.Contains(status)) throw ApiErrorException.Validation("status");
        var sort = Text("sort", 16) ?? "CREATED_DESC"; if (sort is not ("CREATED_ASC" or "CREATED_DESC")) throw ApiErrorException.Validation("sort");
        var email = Text("email", 254); if (email?.Any(c => c > 127) == true) throw ApiErrorException.Validation("email");
        var phone = Text("phone", 30); try { if (phone is not null) phone = ContactNormalizer.NormalizePhone(phone); } catch (ArgumentException) { throw ApiErrorException.Validation("phone"); }
        return new(Text("name", 100), email, phone, body.TryGetProperty("courseId", out _) ? Number("courseId", 1, int.MaxValue) : null, status,
            from is { } f ? f == DateOnly.MinValue ? DateTime.MinValue : VietnamTime.ToUtc(f, TimeOnly.MinValue).UtcDateTime : null,
            to is { } t && t != DateOnly.MaxValue ? VietnamTime.ToUtc(t.AddDays(1), TimeOnly.MinValue).UtcDateTime : null, sort, Number("page", 1, int.MaxValue), Number("pageSize", 20, 100));
    }
    public override string ToString() => nameof(AdmissionsSearchFilter);
}

internal static class SqlAdmissionsSearch
{
    public static async Task<AcademicPage<AdmissionsSummary>> Read(EnglishCenterDbContext db, int actor, AdmissionsSearchFilter filter, CancellationToken ct)
    {
        await using var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.Transaction = db.Database.CurrentTransaction!.GetDbTransaction(); cmd.CommandTimeout = db.Database.GetCommandTimeout() ?? 10;
        cmd.CommandType = CommandType.StoredProcedure; cmd.CommandText = "dbo.usp_SearchAdmissions";
        void P(string name, SqlDbType type, object? value, int size = 0) => cmd.Parameters.Add(new SqlParameter(name, type, size) { Value = value ?? DBNull.Value });
        P("@actorId", SqlDbType.Int, actor); P("@courseId", SqlDbType.Int, filter.CourseId); P("@status", SqlDbType.VarChar, filter.Status, 32);
        P("@createdFromUtc", SqlDbType.DateTime2, filter.FromUtc); P("@createdToExclusiveUtc", SqlDbType.DateTime2, filter.ToExclusiveUtc);
        P("@name", SqlDbType.NVarChar, filter.Name, 200); P("@email", SqlDbType.VarChar, filter.Email, 254); P("@phone", SqlDbType.VarChar, filter.Phone, 10);
        P("@sort", SqlDbType.VarChar, filter.Sort, 16); P("@page", SqlDbType.Int, filter.Page); P("@pageSize", SqlDbType.Int, filter.PageSize);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (reader.FieldCount != 1 || reader.GetName(0) != "totalCount" || !await reader.ReadAsync(ct)) throw new InvalidOperationException("Invalid admissions count.");
        var count = reader.GetInt64(0); if (await reader.ReadAsync(ct) || !await reader.NextResultAsync(ct)) throw new InvalidOperationException("Invalid admissions count.");
        var columns = new[] { "registrationId", "studentId", "fullName", "email", "phone", "courseId", "courseCode", "courseName", "courseStatus", "registrationStatus", "createdAt", "admissionConfirmedAt", "admissionApprovedAt", "bandId", "bandCode", "bandMinScore", "invoiceId", "invoiceNumber", "invoiceAmount", "invoiceStatus", "classId", "assignmentId", "classCode", "classStatus", "registrationVersion" };
        if (reader.FieldCount != columns.Length || columns.Where((name, i) => reader.GetName(i) != name).Any()) throw new InvalidOperationException("Invalid admissions projection.");
        string? At(int i) => reader.IsDBNull(i) ? null : AcademicGuard.Instant(reader.GetDateTime(i));
        var rows = new List<AdmissionsSummary>();
        while (await reader.ReadAsync(ct)) rows.Add(new(reader.GetInt32(0), new(reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetString(4)),
            new(reader.GetInt32(5), reader.GetString(6), reader.GetString(7), reader.GetString(8)), reader.GetString(9), At(10)!, At(11), At(12),
            reader.IsDBNull(13) ? null : new(reader.GetInt32(13), reader.GetString(14)),
            reader.IsDBNull(16) ? null : new(reader.GetInt32(16), reader.GetString(17), reader.GetDecimal(18).ToString("0", CultureInfo.InvariantCulture), reader.GetString(19)),
            reader.IsDBNull(20) ? null : new(reader.GetInt32(20), reader.GetInt32(21), reader.GetString(22), reader.GetString(23)), RowVersionCodec.Encode((byte[])reader.GetValue(24))));
        if (await reader.NextResultAsync(ct)) throw new InvalidOperationException("Invalid admissions projection.");
        return new(rows, filter.Page, filter.PageSize, count);
    }
}

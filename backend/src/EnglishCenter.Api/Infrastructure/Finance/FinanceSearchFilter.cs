namespace EnglishCenter.Api.Infrastructure.Finance;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Common.Serialization;
using EnglishCenter.Api.Common.Time;
using EnglishCenter.Api.Common.Validation;

public sealed record FinanceSearchFilter(string? InvoiceNumber, string? Name, string? Email, string? Phone, int? CourseId,
    string? Status, DateTime? FromUtc, DateTime? ToExclusiveUtc, bool OverdueOnly, string Sort, int Page, int PageSize)
{
    public static FinanceSearchFilter Parse(string json)
    {
        using var doc = JsonDocument.Parse(json); var body = doc.RootElement;
        string[] allowed = ["invoiceNumber", "name", "email", "phone", "courseId", "status", "issueFromDate", "issueToDate", "overdueOnly", "sort", "page", "pageSize"];
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
        var from = Date("issueFromDate"); var to = Date("issueToDate"); if (from > to) throw ApiErrorException.Validation("issueToDate");
        var status = Text("status", 16); if (status is not null && status is not ("UNPAID" or "PAID" or "CANCELLED")) throw ApiErrorException.Validation("status");
        var sort = Text("sort", 16) ?? "ISSUED_DESC"; if (sort is not ("ISSUED_ASC" or "ISSUED_DESC")) throw ApiErrorException.Validation("sort");
        var email = Text("email", 254); var number = Text("invoiceNumber", 16);
        if (email?.Any(c => c > 127) == true) throw ApiErrorException.Validation("email");
        if (number?.Any(c => c > 127) == true) throw ApiErrorException.Validation("invoiceNumber");
        var phone = Text("phone", 30); try { if (phone is not null) phone = ContactNormalizer.NormalizePhone(phone); } catch (ArgumentException) { throw ApiErrorException.Validation("phone"); }
        var overdue = false; if (body.TryGetProperty("overdueOnly", out var flag)) { if (flag.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw ApiErrorException.Validation("overdueOnly"); overdue = flag.GetBoolean(); }
        return new(number, Text("name", 100), email, phone, body.TryGetProperty("courseId", out _) ? Number("courseId", 1, int.MaxValue) : null, status,
            from is { } f ? f == DateOnly.MinValue ? DateTime.MinValue : VietnamTime.ToUtc(f, TimeOnly.MinValue).UtcDateTime : null,
            to is { } t ? t.ToDateTime(TimeOnly.MinValue).AddHours(17) : null, overdue, sort, Number("page", 1, int.MaxValue), Number("pageSize", 20, 100));
    }
    public override string ToString() => nameof(FinanceSearchFilter);
}

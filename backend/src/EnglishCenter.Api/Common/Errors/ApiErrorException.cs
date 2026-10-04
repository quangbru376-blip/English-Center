namespace EnglishCenter.Api.Common.Errors;



public enum ApiErrorCode
{
    VALIDATION_ERROR = 1,
    NO_CHANGE = 2,
    AUTHENTICATION_FAILED = 3,
    AUTHENTICATION_REQUIRED = 4,
    FORBIDDEN = 5,
    CSRF_INVALID = 6,
    NOT_FOUND = 7,
    RESULT_NOT_CONFIRMED = 8,
    CONFLICT = 9,
    STATE_CONFLICT = 10,
    IDEMPOTENCY_CONTENT_MISMATCH = 11,
    CSV_EXPORT_LIMIT = 12,
    REQUEST_TOO_LARGE = 13,
    RATE_LIMITED = 14,
    SERVICE_UNAVAILABLE = 15,
    INTERNAL_ERROR = 16,

}

public static class ApiErrorRegistry
{
    public static string GetCodeString(this ApiErrorCode code) => code switch
    {
        ApiErrorCode.VALIDATION_ERROR => "VALIDATION_ERROR",
        ApiErrorCode.NO_CHANGE => "NO_CHANGE",
        ApiErrorCode.AUTHENTICATION_FAILED => "AUTHENTICATION_FAILED",
        ApiErrorCode.AUTHENTICATION_REQUIRED => "AUTHENTICATION_REQUIRED",
        ApiErrorCode.FORBIDDEN => "FORBIDDEN",
        ApiErrorCode.CSRF_INVALID => "CSRF_INVALID",
        ApiErrorCode.NOT_FOUND => "NOT_FOUND",
        ApiErrorCode.RESULT_NOT_CONFIRMED => "RESULT_NOT_CONFIRMED",
        ApiErrorCode.CONFLICT => "CONFLICT",
        ApiErrorCode.STATE_CONFLICT => "STATE_CONFLICT",
        ApiErrorCode.IDEMPOTENCY_CONTENT_MISMATCH => "IDEMPOTENCY_CONTENT_MISMATCH",
        ApiErrorCode.CSV_EXPORT_LIMIT => "CSV_EXPORT_LIMIT",
        ApiErrorCode.REQUEST_TOO_LARGE => "REQUEST_TOO_LARGE",
        ApiErrorCode.RATE_LIMITED => "RATE_LIMITED",
        ApiErrorCode.SERVICE_UNAVAILABLE => "SERVICE_UNAVAILABLE",
        ApiErrorCode.INTERNAL_ERROR => "INTERNAL_ERROR",
        _ => throw new ArgumentOutOfRangeException(nameof(code), "Invalid API error code.")
    };

    public static string GetCode(this ApiErrorCode code) => code.GetCodeString();

    public static int GetStatusCode(this ApiErrorCode code) => code switch
    {
        ApiErrorCode.VALIDATION_ERROR or ApiErrorCode.NO_CHANGE => 400,
        ApiErrorCode.AUTHENTICATION_FAILED or ApiErrorCode.AUTHENTICATION_REQUIRED => 401,
        ApiErrorCode.FORBIDDEN or ApiErrorCode.CSRF_INVALID => 403,
        ApiErrorCode.NOT_FOUND or ApiErrorCode.RESULT_NOT_CONFIRMED => 404,
        ApiErrorCode.CONFLICT or ApiErrorCode.STATE_CONFLICT or
        ApiErrorCode.IDEMPOTENCY_CONTENT_MISMATCH or ApiErrorCode.CSV_EXPORT_LIMIT => 409,
        ApiErrorCode.REQUEST_TOO_LARGE => 413,
        ApiErrorCode.RATE_LIMITED => 429,
        ApiErrorCode.SERVICE_UNAVAILABLE => 503,
        ApiErrorCode.INTERNAL_ERROR => 500,
        _ => throw new ArgumentOutOfRangeException(nameof(code), "Invalid API error code.")
    };

    public static string GetMessage(this ApiErrorCode code) => code switch
    {
        ApiErrorCode.VALIDATION_ERROR => "Dữ liệu không hợp lệ.",
        ApiErrorCode.NO_CHANGE => "Không có thay đổi nào được thực hiện.",
        ApiErrorCode.AUTHENTICATION_FAILED => "Xác thực không thành công.",
        ApiErrorCode.AUTHENTICATION_REQUIRED => "Yêu cầu đăng nhập.",
        ApiErrorCode.FORBIDDEN => "Bạn không có quyền thực hiện thao tác này.",
        ApiErrorCode.CSRF_INVALID => "Mã xác thực CSRF không hợp lệ.",
        ApiErrorCode.NOT_FOUND => "Không tìm thấy tài nguyên yêu cầu.",
        ApiErrorCode.RESULT_NOT_CONFIRMED => "Kết quả chưa được xác nhận.",
        ApiErrorCode.CONFLICT => "Xung đột dữ liệu.",
        ApiErrorCode.STATE_CONFLICT => "Trạng thái hiện tại không hợp lệ để thực hiện thao tác.",
        ApiErrorCode.IDEMPOTENCY_CONTENT_MISMATCH => "Dữ liệu yêu cầu không khớp với khóa trùng lặp.",
        ApiErrorCode.CSV_EXPORT_LIMIT => "Vượt quá giới hạn xuất dữ liệu CSV.",
        ApiErrorCode.REQUEST_TOO_LARGE => "Kích thước yêu cầu vượt quá giới hạn cho phép.",
        ApiErrorCode.RATE_LIMITED => "Quá nhiều yêu cầu. Vui lòng thử lại sau.",
        ApiErrorCode.SERVICE_UNAVAILABLE => "Dịch vụ tạm thời không khả dụng. Vui lòng thử lại sau.",
        ApiErrorCode.INTERNAL_ERROR => "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
        _ => throw new ArgumentOutOfRangeException(nameof(code), "Invalid API error code.")
    };

    public static string GetVietnameseMessage(this ApiErrorCode code) => code.GetMessage();
}

public sealed class ApiErrorException : Exception
{


    public ApiErrorCode ErrorCode { get; }
    public ApiErrorCode Code => ErrorCode;
    public string? FieldPath { get; }
    public string? Field => FieldPath;
    public int StatusCode { get; }
    public IReadOnlyDictionary<string, string[]> FieldErrors => FieldPath is null ? new Dictionary<string, string[]>() : new Dictionary<string, string[]> { [FieldPath] = ["Giá trị không hợp lệ."] };

    public ApiErrorException(ApiErrorCode errorCode, string? fieldPath = null)
        : base(ValidateCode(errorCode).GetMessage())
    {
        ValidateFieldPath(fieldPath);
        ErrorCode = errorCode;
        FieldPath = fieldPath;
        StatusCode = errorCode.GetStatusCode();

    }

    public static ApiErrorException Validation(string? field = null) => new(ApiErrorCode.VALIDATION_ERROR, field);

    private static ApiErrorCode ValidateCode(ApiErrorCode code) =>
        Enum.IsDefined(code) ? code : throw new ArgumentOutOfRangeException(nameof(code), "Invalid API error code.");

    private static void ValidateFieldPath(string? fieldPath)
    {
        if (fieldPath is null) return;
        if (fieldPath.Length is < 1 or > 128)
            throw new ArgumentException("Field path length must be between 1 and 128.", nameof(fieldPath));
        if (!char.IsAsciiLetter(fieldPath[0]))
            throw new ArgumentException("Field path must start with an ASCII letter.", nameof(fieldPath));
        foreach (var c in fieldPath)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c != '.' && c != '_' && c != '[' && c != ']')
                throw new ArgumentException("Invalid public field path.", nameof(fieldPath));
        }
    }
}


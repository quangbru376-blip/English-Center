using System.Text.Json.Serialization;

namespace EnglishCenter.Api.Api.Identity;

public sealed class RegisterStudentResult
{
    private RegisterStudentResult(string outcome, string message)
    {
        Outcome = outcome;
        Message = message;
    }

    [JsonPropertyName("outcome")]
    public string Outcome { get; }

    [JsonPropertyName("message")]
    public string Message { get; }

    public static RegisterStudentResult Created()
    {
        return new RegisterStudentResult("CREATED", "Tạo tài khoản thành công. Vui lòng đăng nhập.");
    }
}

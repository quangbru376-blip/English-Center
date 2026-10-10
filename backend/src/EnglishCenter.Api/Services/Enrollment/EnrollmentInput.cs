namespace EnglishCenter.Api.Services.Enrollment;

using System.Text.Json;
using EnglishCenter.Api.Common.Replay;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Services.Auth;
using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Time;
public sealed class EnrollmentInput
{
    public int Api { get; }
    public int RegistrationId { get; }
    public int TargetId { get; }
    public Guid RequestId { get; }
    public CanonicalIntent Intent { get; }
    internal JsonElement Body { get; }
    private EnrollmentInput(int api, int registration, int target, Guid request, CanonicalIntent intent) { Api = api; RegistrationId = registration; TargetId = target; RequestId = request; Intent = intent; using var doc = JsonDocument.Parse(intent.CopyBytes()); Body = doc.RootElement.GetProperty("body").Clone(); }
    public static EnrollmentInput Parse(int api, int registration, int target, Guid request, string json, int bytes)
    {
        if (api is not (25 or 28 or 30 or 31 or 32 or 36 or 70 or 71 or 72) || request == Guid.Empty) throw ApiErrorException.Validation();
        var ids = new Dictionary<string, int>(); if (api is 28 or 30 or 31 or 32 or 36) ids["registrationId"] = registration; if (api is 31 or 32 or 70 or 71) ids["bookingId"] = target; if (api == 72) ids["attemptId"] = target;
        return new(api, registration, target, request, CanonicalIntent.Bind(api, json, ids, bytes));
    }
    public override string ToString() => nameof(EnrollmentInput);
}
public static class EnrollmentAuthority
{
    public static async Task<CurrentSessionSnapshot> RequireAsync(ICurrentSessionRepository repository, VerifiedSessionToken token, BusinessClock clock, string role, CancellationToken ct)
    { var current = await new CurrentSessionService(repository, clock).ReadAsync(token, true, ct); if (current.Context.Role != role) throw new ApiErrorException(ApiErrorCode.FORBIDDEN); return current; }
}

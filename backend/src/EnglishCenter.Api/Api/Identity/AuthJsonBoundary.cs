namespace EnglishCenter.Api.Api.Identity;

using System.Text;
using EnglishCenter.Api.Api.Auth;
using EnglishCenter.Api.Common.Errors;
using Microsoft.Net.Http.Headers;

public sealed record AuthBodyPolicy(int MaxBytes);
public static class AuthJsonBoundary
{
    private static readonly object BodyKey = new();
    public static IApplicationBuilder UseAuthJsonBoundary(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        var policy = context.GetEndpoint()?.Metadata.GetMetadata<AuthBodyPolicy>(); if (policy is null) { await next(context); return; }
        if (policy.MaxBytes <= 0) throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
        if (context.Request.ContentLength > policy.MaxBytes) throw new ApiErrorException(ApiErrorCode.REQUEST_TOO_LARGE);
        if (!MediaTypeHeaderValue.TryParse(context.Request.ContentType, out var media) || !media.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
            (media.Charset.HasValue && !media.Charset.Equals("utf-8", StringComparison.OrdinalIgnoreCase))) throw ApiErrorException.Validation();
        using var body = new MemoryStream(); var buffer = new byte[Math.Min(policy.MaxBytes, 8192)];
        while (true)
        {
            var read = await context.Request.Body.ReadAsync(buffer, context.RequestAborted); if (read == 0) break;
            if (body.Length + read > policy.MaxBytes) throw new ApiErrorException(ApiErrorCode.REQUEST_TOO_LARGE);
            await body.WriteAsync(buffer.AsMemory(0, read), context.RequestAborted);
        }
        try { context.Items[BodyKey] = new UTF8Encoding(false, true).GetString(body.GetBuffer(), 0, (int)body.Length); }
        catch (DecoderFallbackException) { throw ApiErrorException.Validation(); }
        try { await next(context); } finally { context.Items.Remove(BodyKey); }
    });
    public static string Read(HttpContext context)
    {
        CsrfBoundary.RequireVerified(context);
        if (!context.Items.TryGetValue(BodyKey, out var body) || body is not string json) throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
        if (context.Request.Query.Count != 0) throw ApiErrorException.Validation();
        return json;
    }
}

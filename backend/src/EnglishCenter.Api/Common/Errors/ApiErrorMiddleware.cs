namespace EnglishCenter.Api.Common.Errors;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public sealed class ApiErrorResponse
{
    [JsonPropertyName("code")] public string Code { get; init; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; init; } = string.Empty;
    [JsonPropertyName("fieldErrors")] public IReadOnlyDictionary<string, string[]> FieldErrors { get; init; } = new Dictionary<string, string[]>();
    [JsonPropertyName("traceId")] public string TraceId { get; init; } = string.Empty;
}

public sealed class ApiErrorMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static readonly IReadOnlyDictionary<string, string[]> EmptyFieldErrors = new Dictionary<string, string[]>();

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiErrorMiddleware> _logger;

    public ApiErrorMiddleware(RequestDelegate next, ILogger<ApiErrorMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted) throw;
            if (ex is OperationCanceledException && context.RequestAborted.IsCancellationRequested) return;

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        int statusCode;
        string code;
        string message;
        IReadOnlyDictionary<string, string[]> fieldErrors;

        if (ex is ApiErrorException apiEx)
        {
            statusCode = apiEx.StatusCode;
            code = apiEx.ErrorCode.GetCodeString();
            message = apiEx.Message;
            fieldErrors = apiEx.FieldErrors;
        }
        else if (ex is BadHttpRequestException badHttpEx)
        {
            if (badHttpEx.StatusCode == StatusCodes.Status413PayloadTooLarge)
            {
                statusCode = StatusCodes.Status413PayloadTooLarge;
                code = ApiErrorCode.REQUEST_TOO_LARGE.GetCodeString();
                message = ApiErrorCode.REQUEST_TOO_LARGE.GetMessage();
            }
            else
            {
                statusCode = StatusCodes.Status400BadRequest;
                code = ApiErrorCode.VALIDATION_ERROR.GetCodeString();
                message = ApiErrorCode.VALIDATION_ERROR.GetMessage();
            }
            fieldErrors = EmptyFieldErrors;
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            code = ApiErrorCode.INTERNAL_ERROR.GetCodeString();
            message = ApiErrorCode.INTERNAL_ERROR.GetMessage();
            fieldErrors = EmptyFieldErrors;
        }

        var traceId = Guid.NewGuid().ToString("D");
        context.TraceIdentifier = traceId;

        _logger.LogError("TraceId: {TraceId}, Code: {Code}", traceId, code);

        context.Response.Clear();
        context.Response.Headers.CacheControl = "no-store";
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new ApiErrorResponse
        {
            Code = code,
            Message = message,
            FieldErrors = fieldErrors,
            TraceId = traceId
        };

        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json, context.RequestAborted);
    }
}

public static class ApiErrorMiddlewareExtensions
{
    public static IApplicationBuilder UseApiErrorMiddleware(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<ApiErrorMiddleware>();
    }
}


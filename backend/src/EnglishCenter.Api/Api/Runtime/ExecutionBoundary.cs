using System.Security.Cryptography;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Readiness;

namespace EnglishCenter.Api.Api.Runtime;

public sealed class ExecutionBoundaryBinding
{
    private ExecutionBoundaryBinding(ExecutionProfile limits) => Limits = limits;
    public ExecutionProfile Limits { get; }
    public override string ToString() => nameof(ExecutionBoundaryBinding);
    public static ExecutionBoundaryBinding Install(IApplicationBuilder app, RuntimeProfile profile)
    {
        ArgumentNullException.ThrowIfNull(app);
        if (RuntimeProfileValidator.Validate(profile) != ProfileIssue.None) throw new ArgumentException("Invalid runtime profile");
        var limits = profile.Execution!;
        app.Use(async (context, next) =>
        {
            var originalToken = context.RequestAborted; var originalBody = context.Request.Body;
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(originalToken);
            deadline.CancelAfter(limits.RequestMs);
            using var body = new MemoryStream();
            var chunk = new byte[Math.Min(limits.RequestBodyBytes, 8192)];
            context.RequestAborted = deadline.Token;
            try
            {
                if (context.Request.ContentLength > limits.RequestBodyBytes) throw new ApiErrorException(ApiErrorCode.REQUEST_TOO_LARGE);
                while (true)
                {
                    var count = await originalBody.ReadAsync(chunk, deadline.Token);
                    if (count == 0) break;
                    if (body.Length + count > limits.RequestBodyBytes) throw new ApiErrorException(ApiErrorCode.REQUEST_TOO_LARGE);
                    body.Write(chunk, 0, count);
                }
                body.Position = 0; context.Request.Body = body;
                await next(context);
                deadline.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) when (!originalToken.IsCancellationRequested && deadline.IsCancellationRequested)
            {
                if (context.Response.HasStarted) throw;
                throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
            }
            finally
            {
                context.RequestAborted = originalToken; context.Request.Body = originalBody;
                CryptographicOperations.ZeroMemory(chunk);
                if (body.TryGetBuffer(out var bytes)) CryptographicOperations.ZeroMemory(bytes.AsSpan());
            }
        });
        return new(limits);
    }
    public BoundedResponseBuffer CreateResponseBuffer() => new(Limits.ResponseBufferBytes);
}

public sealed class BoundedResponseBuffer : IDisposable
{
    private readonly int capacity;
    private readonly MemoryStream bytes = new();
    public BoundedResponseBuffer(int capacity)
    { if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity)); this.capacity = capacity; }
    public void Append(ReadOnlySpan<byte> value)
    {
        if (bytes.Length + value.Length > capacity) throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
        bytes.Write(value);
    }
    public byte[] ToArray() => bytes.ToArray();
    public void Dispose()
    {
        if (bytes.TryGetBuffer(out var buffer)) CryptographicOperations.ZeroMemory(buffer.AsSpan());
        bytes.Dispose();
    }
}

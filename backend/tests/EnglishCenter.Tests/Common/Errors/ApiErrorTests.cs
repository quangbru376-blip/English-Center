namespace EnglishCenter.Tests.Common.Errors;

using System.Text.Json;
using EnglishCenter.Api.Common.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Xunit;

public class ApiErrorTests
{
    public static TheoryData<ApiErrorCode, int> Registry => new()
    {
        {ApiErrorCode.VALIDATION_ERROR,400}, {ApiErrorCode.NO_CHANGE,400},
        {ApiErrorCode.AUTHENTICATION_FAILED,401}, {ApiErrorCode.AUTHENTICATION_REQUIRED,401},
        {ApiErrorCode.FORBIDDEN,403}, {ApiErrorCode.CSRF_INVALID,403},
        {ApiErrorCode.NOT_FOUND,404}, {ApiErrorCode.RESULT_NOT_CONFIRMED,404},
        {ApiErrorCode.CONFLICT,409}, {ApiErrorCode.STATE_CONFLICT,409},
        {ApiErrorCode.IDEMPOTENCY_CONTENT_MISMATCH,409}, {ApiErrorCode.CSV_EXPORT_LIMIT,409},
        {ApiErrorCode.REQUEST_TOO_LARGE,413}, {ApiErrorCode.RATE_LIMITED,429},
        {ApiErrorCode.SERVICE_UNAVAILABLE,503}, {ApiErrorCode.INTERNAL_ERROR,500}
    };

    [Theory, MemberData(nameof(Registry))]
    public async Task ExactRegistry_ReturnsOnlySafeEnvelope(ApiErrorCode code, int status)
    {
        var (context, logger) = await Handle(new ApiErrorException(code));
        using var json = JsonDocument.Parse(Read(context));
        var root = json.RootElement;
        Assert.Equal(status, context.Response.StatusCode);
        Assert.Equal(new[]{"code","fieldErrors","message","traceId"}, root.EnumerateObject().Select(p=>p.Name).Order().ToArray());
        Assert.Equal(code.ToString(), root.GetProperty("code").GetString());
        Assert.NotEmpty(root.GetProperty("message").GetString()!);
        Assert.Empty(root.GetProperty("fieldErrors").EnumerateObject());
        var trace = root.GetProperty("traceId").GetString()!;
        Assert.True(Guid.TryParseExact(trace,"D",out _));
        Assert.Equal(trace,context.TraceIdentifier);
        Assert.Contains(trace,Assert.Single(logger.Messages));
        Assert.Equal("no-store",context.Response.Headers.CacheControl);
    }

    [Fact]
    public async Task FieldErrors_CannotBeMutatedToLeakInput()
    {
        var error=ApiErrorException.Validation("items[0].name");
        error.FieldErrors["items[0].name"][0]="PRIVATE_INPUT";
        var (context,_) = await Handle(error);
        var body=Read(context);
        Assert.DoesNotContain("PRIVATE_INPUT",body);
        using var json=JsonDocument.Parse(body);
        Assert.Equal("Giá trị không hợp lệ.",json.RootElement.GetProperty("fieldErrors").GetProperty("items[0].name")[0].GetString());
    }

    [Theory]
    [InlineData(""),InlineData("0name"),InlineData("name/secret"),InlineData("họTên"),InlineData("a\n")]
    public void ProgrammerFieldPath_Invalid(string field) => Assert.Throws<ArgumentException>(()=>ApiErrorException.Validation(field));

    [Fact]
    public void InvalidRegistryAndLongField_AreProgrammerErrors()
    {
        Assert.Throws<ArgumentOutOfRangeException>(()=>new ApiErrorException((ApiErrorCode)999));
        Assert.Throws<ArgumentException>(()=>ApiErrorException.Validation(new string('a',129)));
    }

    [Theory]
    [InlineData(false),InlineData(true)]
    public async Task UnexpectedErrorsAndArgumentErrors_NeverExposeDetails(bool argument)
    {
        const string secret="password=PRIVATE_INPUT; SELECT * FROM Account; stack";
        var (context,logger)=await Handle(argument?new ArgumentException(secret):new InvalidOperationException(secret));
        Assert.Equal(500,context.Response.StatusCode);
        Assert.Contains("INTERNAL_ERROR",Read(context));
        Assert.DoesNotContain("PRIVATE_INPUT",Read(context));
        Assert.DoesNotContain("SELECT",Read(context));
        Assert.All(logger.Messages,m=>Assert.DoesNotContain("PRIVATE_INPUT",m));
        Assert.All(logger.Exceptions,Assert.Null);
    }

    [Theory]
    [InlineData(413,"REQUEST_TOO_LARGE",413),InlineData(400,"VALIDATION_ERROR",400)]
    public async Task TransportErrors_AreSanitized(int transport,string code,int status)
    {
        var (context,_)=await Handle(new BadHttpRequestException("PRIVATE_INPUT",transport));
        Assert.Equal(status,context.Response.StatusCode);
        Assert.Contains(code,Read(context));
        Assert.DoesNotContain("PRIVATE_INPUT",Read(context));
    }

    [Fact]
    public async Task AbortedCancellation_WritesNothing()
    {
        var context=NewContext();
        context.RequestAborted=new CancellationToken(true);
        var logger=new RecordingLogger();
        await new ApiErrorMiddleware(_=>throw new OperationCanceledException(),logger).InvokeAsync(context);
        Assert.Empty(Read(context));
        Assert.Empty(logger.Messages);
    }

    [Fact]
    public async Task NonAbortedCancellation_IsSanitizedInternalError()
    {
        var (context,_)=await Handle(new OperationCanceledException("PRIVATE_INPUT"));
        Assert.Equal(500,context.Response.StatusCode);
        Assert.DoesNotContain("PRIVATE_INPUT",Read(context));
    }

    [Fact]
    public async Task StartedResponse_RethrowsOriginal()
    {
        var context=NewContext();
        context.Features.Set<IHttpResponseFeature>(new StartedResponse());
        var failure=new InvalidOperationException("PRIVATE_INPUT");
        var middleware=new ApiErrorMiddleware(_=>throw failure,new RecordingLogger());
        Assert.Same(failure,await Assert.ThrowsAsync<InvalidOperationException>(()=>middleware.InvokeAsync(context)));
    }

    private static DefaultHttpContext NewContext()
    {
        var context=new DefaultHttpContext();
        context.Response.Body=new MemoryStream();
        return context;
    }
    private static string Read(HttpContext context) => System.Text.Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray());
    private static async Task<(DefaultHttpContext,RecordingLogger)> Handle(Exception exception)
    {
        var context=NewContext();
        var logger=new RecordingLogger();
        await new ApiErrorMiddleware(_=>throw exception,logger).InvokeAsync(context);
        return(context,logger);
    }
    private sealed class RecordingLogger : ILogger<ApiErrorMiddleware>
    {
        public List<string> Messages {get;}=[];
        public List<Exception?> Exceptions {get;}=[];
        public IDisposable? BeginScope<TState>(TState state) where TState:notnull => null;
        public bool IsEnabled(LogLevel level)=>true;
        public void Log<TState>(LogLevel level,EventId id,TState state,Exception? exception,Func<TState,Exception?,string> formatter)
        {Messages.Add(formatter(state,exception));Exceptions.Add(exception);}
    }
    private sealed class StartedResponse : HttpResponseFeature
    {public override bool HasStarted=>true;}
}

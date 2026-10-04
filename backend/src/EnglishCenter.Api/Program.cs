using EnglishCenter.Api.Common.Errors;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseApiErrorMiddleware();

app.Run();

public partial class Program
{
}

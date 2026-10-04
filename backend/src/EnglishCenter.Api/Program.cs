using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Errors;

var builder = WebApplication.CreateBuilder(args);
var blocklist = OfflinePasswordBlocklist.Load(Path.Combine(AppContext.BaseDirectory, "Resources", "PasswordBlocklist"));
builder.Services.AddSingleton(blocklist);
builder.Services.AddSingleton<PasswordPolicy>();
builder.Services.AddSingleton<CredentialHasher>();
builder.Services.AddSingleton<TemporaryPasswordGenerator>();
var app = builder.Build();

app.UseApiErrorMiddleware();

app.Run();

public partial class Program
{
}


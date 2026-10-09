using EnglishCenter.Api.Common.Security;
using EnglishCenter.Api.Common.Errors;

if (args.FirstOrDefault() == "--m0-provision-csrf")
{
    var code = args.Length == 1 ? EnglishCenter.Api.Infrastructure.Operator.RuntimeCsrfProvisioner.Run(CancellationToken.None) : 1;
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new { phase = "W4-CSRF", success = code == 0 }));
    Environment.ExitCode = code;
    return;
}
if (args.FirstOrDefault() == "--m0-maintenance")
{
    var code = args.Length == 1 ? await EnglishCenter.Api.Infrastructure.Operator.LocalMaintenanceHost.RunAsync(CancellationToken.None) : 1;
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new { phase = "W5-maintenance", success = code == 0 }));
    Environment.ExitCode = code;
    return;
}
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


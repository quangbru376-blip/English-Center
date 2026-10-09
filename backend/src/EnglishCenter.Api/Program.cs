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
if (args.FirstOrDefault() == "--m0-local-runtime")
{
    var code = args.Length == 1 ? await EnglishCenter.Api.Infrastructure.Operator.LocalRuntimeHost.RunAsync(CancellationToken.None) : 1;
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new { phase = "W5-runtime", success = code == 0 }));
    Environment.ExitCode = code;
    return;
}
if (args.FirstOrDefault() == "--m0-bootstrap")
{
    var checkpoint = "arguments";
    var outcome = args.Length == 1 ? await EnglishCenter.Api.Infrastructure.Operator.LocalAdminBootstrap.RunAsync(Console.In, CancellationToken.None, value => checkpoint = value) : EnglishCenter.Api.Infrastructure.Operator.LocalAdminBootstrap.Outcome.Refused;
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new { phase = "W5-bootstrap", checkpoint, success = outcome != EnglishCenter.Api.Infrastructure.Operator.LocalAdminBootstrap.Outcome.Refused, created = outcome == EnglishCenter.Api.Infrastructure.Operator.LocalAdminBootstrap.Outcome.Created, verified = outcome == EnglishCenter.Api.Infrastructure.Operator.LocalAdminBootstrap.Outcome.Verified }));
    Environment.ExitCode = outcome == EnglishCenter.Api.Infrastructure.Operator.LocalAdminBootstrap.Outcome.Refused ? 1 : 0;
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


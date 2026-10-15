using EnglishCenter.Api.Infrastructure.Operator;
LocalRuntimeReferences.SelectDemo();
Environment.ExitCode = args.FirstOrDefault() switch
{
    "seed" when args.Length == 1 => await DemoSeed.Run(),
    "scenarios" when args.Length == 1 => await DemoScenarios.Run(),
    "chronology" when args.Length == 1 => await DemoScenarios.Run(DemoFixtureStep.Chronology),
    "contacts" when args.Length == 1 => await DemoScenarios.Run(DemoFixtureStep.Contacts),
    "draft-reports" when args.Length == 1 => await DemoScenarios.Run(DemoFixtureStep.DraftReports),
    "check" when args.Length == 1 => await DemoValidation.Run(),
    "csrf" when args.Length == 1 => RuntimeCsrfProvisioner.Run(CancellationToken.None),
    "runtime" when args.Length == 1 => await RunHost(false),
    "maintenance" when args.Length == 1 => await RunHost(true),
    _ => 1
};
static async Task<int> RunHost(bool maintenance)
{
    long replay=0,audit=0,failures=0;bool controlled=false;
    var code=await ManagedOperatorLifetime.RunAsync(async token=>{
        var result=maintenance?await LocalMaintenanceHost.RunAsync(token,false,jobs=>{replay=jobs.CompletedReplayRuns;audit=jobs.CompletedAuditRuns;failures=jobs.FailedRuns;}):await LocalRuntimeHost.RunAsync(token);
        controlled=token.IsCancellationRequested;return result;
    },Console.In,CancellationToken.None);
    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new {host=maintenance?"maintenance":"runtime",code,controlled,replayCommits=replay,auditCommits=audit,failedRuns=failures}));return code;
}

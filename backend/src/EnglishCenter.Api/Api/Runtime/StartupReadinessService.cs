namespace EnglishCenter.Api.Api.Runtime;

// Explicitly registered only alongside the per-request RuntimeReadinessBoundary.
public sealed class StartupReadinessService : BackgroundService
{
    private readonly RuntimeReadinessBoundary boundary;
    private readonly IHostApplicationLifetime lifetime;
    private int refused;
    public bool HasRefusedStartup => Volatile.Read(ref refused) == 1;
    public StartupReadinessService(RuntimeReadinessBoundary boundary, IHostApplicationLifetime lifetime)
    {
        this.boundary = boundary ?? throw new ArgumentNullException(nameof(boundary));
        this.lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var stopping = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, lifetime.ApplicationStopping);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = lifetime.ApplicationStarted.Register(() => started.TrySetResult());
        // A transport dependency needs the listener; business requests still pass the fresh Q gate.
        await started.Task.WaitAsync(stopping.Token);
        try
        {
            var result = await boundary.EvaluateAsync(stopping.Token);
            stopping.Token.ThrowIfCancellationRequested();
            if (result.IsReady) return;
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested) { throw; }
        catch (Exception) { stopping.Token.ThrowIfCancellationRequested(); }
        Volatile.Write(ref refused, 1);
        lifetime.StopApplication();
    }
    public override string ToString() => nameof(StartupReadinessService);
}


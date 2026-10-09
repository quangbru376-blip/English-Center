namespace EnglishCenter.Api.Infrastructure.Operator;

public static class ManagedOperatorLifetime
{
    public static async Task<int> RunAsync(Func<CancellationToken, Task<int>> run, TextReader control, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(run); ArgumentNullException.ThrowIfNull(control); token.ThrowIfCancellationRequested();
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(token);
        var monitor = Task.Run(() => StopOnInputAsync(control, lifetime), CancellationToken.None);
        try { return await run(lifetime.Token); }
        finally { lifetime.Cancel(); EnglishCenter.Api.Infrastructure.Readiness.ProbeWait.Observe(monitor); }
    }
    private static async Task StopOnInputAsync(TextReader control, CancellationTokenSource lifetime)
    {
        try { var one = new char[1]; await control.ReadAsync(one.AsMemory(), lifetime.Token); lifetime.Cancel(); }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        catch (Exception) { try { lifetime.Cancel(); } catch (ObjectDisposedException) { } }
    }
}
using System.Diagnostics;
namespace AsyncMonitor;

public sealed class CheckService
{
    public async Task<CheckResult> CheckServiceAsync(string name, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        // TODO 1
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        // TODO 2
        if (delay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delay), delay, "Длительность не может быть отрицательной.");

        var sw = Stopwatch.StartNew();
        // TODO 3
        await Task.Delay(delay, cancellationToken);
        sw.Stop();

        // TODO 4
        return new CheckResult(name, true, sw.Elapsed, $"Проверка «{name}» выполнена за {sw.ElapsedMilliseconds} мс.");
    }

    public async Task<IReadOnlyList<CheckResult>> RunSequentialAsync(IReadOnlyList<(string Name, TimeSpan Delay)> checks, CancellationToken token = default)
    {
        var results = new List<CheckResult>();
        // TODO 5
        foreach (var (name, delay) in checks)
            results.Add(await CheckServiceAsync(name, delay, token));
        return results;
    }

    public async Task<IReadOnlyList<CheckResult>> RunParallelAsync(IReadOnlyList<(string Name, TimeSpan Delay)> checks, CancellationToken token = default)
    {
        // TODO 6
        var tasks = checks.Select(c => CheckServiceAsync(c.Name, c.Delay, token)).ToList();
        // TODO 7
        return await Task.WhenAll(tasks);
    }
}

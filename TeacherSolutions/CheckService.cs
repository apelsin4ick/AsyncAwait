using System.Diagnostics;
namespace AsyncMonitor;
public sealed class CheckService{
 public async Task<CheckResult> CheckServiceAsync(string name,TimeSpan delay,CancellationToken cancellationToken=default){
  if(string.IsNullOrWhiteSpace(name))throw new ArgumentException("Имя проверки обязательно.",nameof(name));
  if(delay<TimeSpan.Zero)throw new ArgumentOutOfRangeException(nameof(delay));
  var sw=Stopwatch.StartNew();await Task.Delay(delay,cancellationToken);sw.Stop();
  return new(name,true,sw.Elapsed,"Проверка завершена успешно.");
 }
 public async Task<IReadOnlyList<CheckResult>> RunSequentialAsync(IReadOnlyList<(string Name,TimeSpan Delay)> checks,CancellationToken token=default){var r=new List<CheckResult>();foreach(var c in checks)r.Add(await CheckServiceAsync(c.Name,c.Delay,token));return r;}
 public async Task<IReadOnlyList<CheckResult>> RunParallelAsync(IReadOnlyList<(string Name,TimeSpan Delay)> checks,CancellationToken token=default){var tasks=checks.Select(c=>CheckServiceAsync(c.Name,c.Delay,token)).ToArray();return await Task.WhenAll(tasks);}
}

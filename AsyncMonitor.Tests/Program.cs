using AsyncMonitor;
var passed = 0;
var failed = 0;
void Check(bool ok, string name)
{
    if (ok)
    {
        passed++;
        Console.WriteLine("PASS " + name);
    }
    else
    {
        failed++;
        Console.WriteLine("FAIL " + name);
    }
}
var s = new CheckService();
var one = await s.CheckServiceAsync("Тест", TimeSpan.FromMilliseconds(25));
Check(
    one.Name == "Тест" && one.Success && one.Duration >= TimeSpan.Zero,
    "одиночная проверка возвращает результат"
);
var seq = await s.RunSequentialAsync(
    new[]
    {
        ("A", TimeSpan.FromMilliseconds(60)),
        ("B", TimeSpan.FromMilliseconds(60)),
        ("C", TimeSpan.FromMilliseconds(60))
    }
);
Check(
    seq.Count == 3 && seq.Select(x => x.Name).SequenceEqual(new[] { "A", "B", "C" }),
    "последовательный запуск"
);
var all = await s.RunParallelAsync(
    new[]
    {
        ("A", TimeSpan.FromMilliseconds(60)),
        ("B", TimeSpan.FromMilliseconds(60)),
        ("C", TimeSpan.FromMilliseconds(60))
    }
);
Check(all.Count == 3 && all.All(x => x.Success), "параллельный запуск WhenAll");

// Пустое имя
try
{
    await s.CheckServiceAsync("  ", TimeSpan.FromMilliseconds(10));
    Check(false, "пустое имя отклоняется");
}
catch (ArgumentException)
{
    Check(true, "пустое имя отклоняется");
}

// Отрицательная задержка
try
{
    await s.CheckServiceAsync("X", TimeSpan.FromMilliseconds(-1));
    Check(false, "отрицательная задержка отклоняется");
}
catch (ArgumentOutOfRangeException)
{
    Check(true, "отрицательная задержка отклоняется");
}

// CancellationToken
using var cts = new CancellationTokenSource();
var pending = s.CheckServiceAsync("X", TimeSpan.FromSeconds(5), cts.Token);
cts.Cancel();
try
{
    await pending;
    Check(false, "CancellationToken отменяет проверку");
}
catch (OperationCanceledException)
{
    Check(true, "CancellationToken отменяет проверку");
}

Console.WriteLine($"Итого: {passed} пройдено, {failed} ошибок.");
return failed == 0 ? 0 : 1;

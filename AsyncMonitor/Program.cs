using AsyncMonitor;

var service = new CheckService();
var checks = new (string Name, TimeSpan Delay)[]
{
    ("Сеть", TimeSpan.FromMilliseconds(700)),
    ("База данных", TimeSpan.FromMilliseconds(900)),
    ("Файлы", TimeSpan.FromMilliseconds(500))
};

while (true)
{
    Console.WriteLine("\n1 — одна проверка; 2 — последовательно; 3 — параллельно; 4 — долгая проверка с отменой; 0 — выход");
    var cmd = Console.ReadLine();
    if (cmd == "0")
        break;
    try
    {
        if (cmd == "1")
        {
            // TODO 8
            var r = await service.CheckServiceAsync(checks[0].Name, checks[0].Delay);
            Console.WriteLine(new[] { r });
        }
        else if (cmd == "2")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunSequentialAsync(checks);
            sw.Stop();
            Console.WriteLine($"Последовательно: {sw.ElapsedMilliseconds} мс");
            Console.WriteLine(r);
        }
        else if (cmd == "3")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunParallelAsync(checks);
            sw.Stop();
            Console.WriteLine($"Параллельно: {sw.ElapsedMilliseconds} мс");
            Console.WriteLine(r);
        }
        else if (cmd == "4")
        {
            await RunLongCheckAsync(service);
        }
        else
            Console.WriteLine("Неизвестная команда.");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Проверка отменена.");
    }
    catch (Exception ex)
    {
        // TODO 9
        Console.WriteLine($"Ошибка {ex.GetType().Name}: {ex.Message}");
    }
}

// TODO 10
static async Task RunLongCheckAsync(CheckService service)
{
    using var cts = new CancellationTokenSource();
    Console.WriteLine("Долгая проверка (8 с) запущена. Нажмите 'c' для отмены.");
    var task = service.CheckServiceAsync("Долгая проверка", TimeSpan.FromSeconds(8), cts.Token);

    var ticks = 0;
    while (!task.IsCompleted)
    {
        if (!Console.IsInputRedirected && Console.KeyAvailable
            && Console.ReadKey(true).Key == ConsoleKey.C)
        {
            cts.Cancel();
        }
        Console.WriteLine($"  ...поток свободен, тик {++ticks}");
        await Task.Delay(300);
    }

    Print(new[] { await task });

    static void Print(IEnumerable<CheckResult> xs)
    {
        foreach (var x in xs)
            Console.WriteLine($"  [{(x.Success ? "OK" : "FAIL")}] {x.Name} — {x.Duration.TotalMilliseconds:F0} мс — {x.Message}");
    }
}
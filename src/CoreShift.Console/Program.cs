using CoreShift.Data;
﻿using CoreShift.App;
using CoreShift.Core.Progression;
using CoreShift.Core.Systems;
using CoreShift.Core.Upgrades;
using CoreShift.Data.Content;

int ticks = 1800;
uint seed = 12345;
bool bench = false;
bool auto = false;
int renderEvery = 0;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--bench":
            bench = true;
            break;
        case "--auto":
            auto = true;
            break;
        case "--ticks":
            if (i + 1 < args.Length) ticks = int.Parse(args[++i]);
            break;
        case "--seed":
            if (i + 1 < args.Length) seed = uint.Parse(args[++i]);
            break;
        case "--render":
            if (i + 1 < args.Length) renderEvery = int.Parse(args[++i]);
            break;
        case "--help":
        case "-h":
            PrintHelp();
            return;
    }
}

if (bench)
{
    RunBenchmarkTable();
    return;
}

var content = TryLoadContent();

if (auto)
{
    var progression = new ProgressionState();
    var snapshot = GameRunner.Run(new AutoPilotInputSource(), System.Console.Out, ticks, seed, content, progression, renderEvery);
    System.Console.WriteLine(snapshot.ToString());
    System.Console.WriteLine($"Run currency: {progression.LastRunCurrency} | Total currency: {progression.Data.Currency}");
    return;
}

RunInteractive(content, seed);

void RunInteractive(GameContent gameContent, uint runSeed)
{
    var progression = new ProgressionState();
    var world = GameFactory.Build(runSeed, gameContent, progression);
    var upgradeSystem = world.Systems.OfType<UpgradeSystem>().First();

    System.Console.CursorVisible = false;
    System.Console.WriteLine("CoreShift: Chrono-Survival  |  WASD to move, Esc to quit. Press any key to start.");
    System.Console.ReadKey(true);

    var frameDelay = 1000 / 60;
    while (!world.IsGameOver)
    {
        if (System.Console.KeyAvailable)
        {
            var key = System.Console.ReadKey(true).Key;
            if (key == ConsoleKey.Escape) break;
            world.Input = key switch
            {
                ConsoleKey.W or ConsoleKey.UpArrow => InputState.Move(0f, 1f),
                ConsoleKey.S or ConsoleKey.DownArrow => InputState.Move(0f, -1f),
                ConsoleKey.A or ConsoleKey.LeftArrow => InputState.Move(-1f, 0f),
                ConsoleKey.D or ConsoleKey.RightArrow => InputState.Move(1f, 0f),
                _ => InputState.None,
            };
        }

        world.Tick();

        if (upgradeSystem.HasOffers)
        {
            upgradeSystem.Choose(world, 0);
        }

        System.Console.Clear();
        System.Console.WriteLine(AsciiRenderer.Render(world));
        System.Console.WriteLine(world.Snapshot().ToString());
        System.Console.WriteLine("WASD move  |  Esc quit");
        Thread.Sleep(frameDelay);
    }

    System.Console.WriteLine();
    System.Console.WriteLine("Game over. " + world.Snapshot());
    System.Console.WriteLine($"Currency earned: {progression.LastRunCurrency}");
    System.Console.CursorVisible = true;
}

static GameContent TryLoadContent()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    for (int i = 0; i < 8 && directory is not null; i++)
    {
        string candidate = Path.Combine(directory.FullName, "content");
        if (Directory.Exists(candidate))
        {
            try
            {
                return ContentLoader.Load(candidate);
            }
            catch (ContentLoadException ex)
            {
                System.Console.WriteLine("Warning: content load failed (" + ex.Message + "). Using built-in defaults.");
                return GameContent.Default();
            }
        }
        directory = directory.Parent;
    }
    return GameContent.Default();
}

static void RunBenchmarkTable()
{
    System.Console.WriteLine("CoreShift benchmark (per fixed tick, 60 Hz logic)");
    System.Console.WriteLine($"{"Enemies",10} | {"Avg ms",10} | {"p95 ms",10} | {"Entities/s",14} | {"PoolCreated",12} | {"PoolDropped",12}");
    System.Console.WriteLine(new string('-', 82));

    foreach (int enemyCount in new[] { 256, 512, 1024, 2048 })
    {
        var result = Benchmark.Run(enemyCount, ticks: 120, seed: 7);
        System.Console.WriteLine(
            $"{result.EnemyCount,10} | {result.AvgMs,10:0.0000} | {result.P95Ms,10:0.0000} | {result.EntitiesPerSecond,14:0} | {result.PoolCreated,12} | {result.PoolDropped,12}");
    }
}

static void PrintHelp()
{
    System.Console.WriteLine("CoreShift: Chrono-Survival");
    System.Console.WriteLine("  dotnet run --project src/CoreShift.Console            # play interactively (WASD)");
    System.Console.WriteLine("  dotnet run --project src/CoreShift.Console -- --auto --ticks 1800");
    System.Console.WriteLine("  dotnet run --project src/CoreShift.Console -- --bench");
    System.Console.WriteLine("Options: --auto --bench --ticks N --seed N --render N --help");
}

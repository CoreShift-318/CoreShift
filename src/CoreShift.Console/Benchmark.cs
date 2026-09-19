using CoreShift.Data;
using System.Diagnostics;

namespace CoreShift.App;

public readonly struct BenchmarkResult
{
    public int EnemyCount { get; init; }
    public int Ticks { get; init; }
    public double AvgMs { get; init; }
    public double P95Ms { get; init; }
    public double EntitiesPerSecond { get; init; }
    public int PoolCreated { get; init; }
    public int PoolReused { get; init; }
    public int PoolDropped { get; init; }
}

public static class Benchmark
{
    public static BenchmarkResult Run(int enemyCount, int ticks, uint seed = 1)
    {
        if (ticks < 1) ticks = 1;

        var world = GameFactory.BuildBenchmark(seed, enemyCount);
        var times = new double[ticks];
        var stopwatch = new Stopwatch();

        for (int i = 0; i < ticks; i++)
        {
            stopwatch.Restart();
            world.Tick();
            stopwatch.Stop();
            times[i] = stopwatch.Elapsed.TotalMilliseconds;
        }

        double sum = 0;
        for (int i = 0; i < times.Length; i++) sum += times[i];
        double average = sum / times.Length;

        Array.Sort(times);
        int index = (int)(times.Length * 0.95);
        if (index >= times.Length) index = times.Length - 1;
        double p95 = times[index];

        double entitiesPerSecond = average <= 0 ? 0 : enemyCount * 1000.0 / average;

        return new BenchmarkResult
        {
            EnemyCount = enemyCount,
            Ticks = ticks,
            AvgMs = average,
            P95Ms = p95,
            EntitiesPerSecond = entitiesPerSecond,
            PoolCreated = world.EnemyPool.Created,
            PoolReused = world.EnemyPool.Reused,
            PoolDropped = world.EnemyPool.Dropped,
        };
    }
}

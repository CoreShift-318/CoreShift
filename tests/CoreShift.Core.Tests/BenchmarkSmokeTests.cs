using CoreShift.App;

namespace CoreShift.Core.Tests;

public class BenchmarkSmokeTests
{
    [Fact]
    public void Benchmark_RunsAt1024Enemies()
    {
        var result = Benchmark.Run(enemyCount: 1024, ticks: 60, seed: 7);

        Assert.Equal(1024, result.EnemyCount);
        Assert.True(result.AvgMs >= 0);
        Assert.True(result.P95Ms >= 0);
        Assert.Equal(0, result.PoolDropped);
        Assert.True(result.PoolCreated > 0);
    }

    [Fact]
    public void Benchmark_ScalesTo2048()
    {
        var result = Benchmark.Run(enemyCount: 2048, ticks: 30, seed: 11);

        Assert.Equal(2048, result.EnemyCount);
        Assert.Equal(0, result.PoolDropped);
    }
}

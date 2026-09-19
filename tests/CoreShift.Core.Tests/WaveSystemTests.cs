using CoreShift.Core.Ecs;
using CoreShift.Core.Systems;
using CoreShift.Core.Waves;

namespace CoreShift.Core.Tests;

public class WaveSystemTests
{
    [Theory]
    [InlineData(1, 1f)]
    [InlineData(3, 1.3f)]
    public void HealthMultiplier_Scales(int wave, float expected)
        => Assert.Equal(expected, WaveScaling.HealthMultiplier(wave), 3);

    [Theory]
    [InlineData(1, 5)]
    [InlineData(4, 11)]
    public void EnemyCount_Scales(int wave, int expected)
        => Assert.Equal(expected, WaveScaling.EnemyCount(wave));

    [Fact]
    public void EnemyCount_Caps()
        => Assert.Equal(200, WaveScaling.EnemyCount(1000));

    [Fact]
    public void SpeedMultiplier_Scales()
        => Assert.Equal(1.09f, WaveScaling.SpeedMultiplier(4), 3);

    [Fact]
    public void Wave_EventuallySpawnsEnemies()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        var spawner = new WaveSpawner();

        for (int i = 0; i < 600; i++) spawner.Update(world, 1f / 60f);

        int enemies = 0;
        foreach (var _ in world.Entities.With<EnemyTag>()) enemies++;

        Assert.True(enemies > 0);
        Assert.True(world.EnemyPool.Created > 0);
    }

    [Fact]
    public void WaveSystem_SyncsWorldWave()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        var system = new WaveSystem();

        for (int i = 0; i < 1200; i++) system.Update(world, 1f / 60f);

        Assert.True(world.Wave >= 1);
    }

    [Fact]
    public void ContentTable_SpawnsMultipleEnemyTypes()
    {
        var content = CoreShift.Data.Content.GameContent.Default();
        var table = CoreShift.Data.WaveTableFactory.FromContent(content);
        var world = new World(7, new Arena(100, 100));
        world.CreatePlayer();
        var spawner = new WaveSpawner(table);

        for (int i = 0; i < 1500; i++) spawner.Update(world, 1f / 60f);

        var ids = new HashSet<string>();
        foreach (var entity in world.Entities.With<EnemyTag>())
        {
            if (world.Entities.Has<EnemyLink>(entity))
            {
                ids.Add(world.Entities.Get<EnemyLink>(entity).Instance.Spec.Id);
            }
        }

        Assert.True(ids.Count >= 2, "expected at least two enemy types, got: " + string.Join(",", ids));
    }
}

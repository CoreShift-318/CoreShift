using CoreShift.Core.Progression;

namespace CoreShift.Core.Tests;

public class ProgressionSystemTests
{
    [Fact]
    public void CompleteRun_AccumulatesCurrencyAndBest()
    {
        var progression = new ProgressionState(new SaveData());

        int gain = progression.CompleteRun(waveReached: 5, kills: 20, seconds: 120);

        Assert.Equal(70, gain);
        Assert.Equal(70, progression.Data.Currency);
        Assert.Equal(5, progression.Data.BestWave);
        Assert.Equal(20, progression.Data.TotalKills);

        progression.CompleteRun(waveReached: 3, kills: 0, seconds: 10);
        Assert.Equal(5, progression.Data.BestWave);
    }

    [Fact]
    public void Snapshot_ReflectsWorldState()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();

        var snapshot = world.Snapshot();

        Assert.Equal(100f, snapshot.MaxHealth);
        Assert.False(snapshot.IsGameOver);
        Assert.Equal(1, snapshot.Level);
    }

    [Fact]
    public void ProgressionSystem_ReportsOnce()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        var progression = new ProgressionState();
        var system = new CoreShift.Core.Systems.ProgressionSystem(progression);
        world.AddSystem(system);

        world.IsGameOver = true;
        system.Update(world, 1f / 60f);
        system.Update(world, 1f / 60f);

        Assert.Equal(1, progression.Data.BestWave > 0 ? 1 : 0);
        Assert.Equal(10, progression.Data.Currency);
    }

    [Fact]
    public void PermanentUpgrades_TrackRanks()
    {
        var progression = new ProgressionState();
        progression.AddPermanentUpgrade("power", 2);
        progression.AddPermanentUpgrade("power", 1);
        Assert.Equal(3, progression.GetPermanentRank("power"));
    }
}

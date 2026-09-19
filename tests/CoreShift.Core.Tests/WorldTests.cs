using CoreShift.Core.Math;
using CoreShift.Core.Systems;

namespace CoreShift.Core.Tests;

public class WorldTests
{
    private sealed class RecordingSystem : ISystem
    {
        private readonly List<string> _log;
        private readonly string _name;

        public RecordingSystem(List<string> log, string name)
        {
            _log = log;
            _name = name;
        }

        public void Update(World world, float dt) => _log.Add(_name);
    }

    [Fact]
    public void Tick_RunsSystemsInOrder()
    {
        var log = new List<string>();
        var world = new World(1, new Arena(100, 100));
        world.AddSystem(new RecordingSystem(log, "a"));
        world.AddSystem(new RecordingSystem(log, "b"));

        world.Tick();

        Assert.Equal(new[] { "a", "b" }, log);
    }

    [Fact]
    public void Tick_IncrementsCounters()
    {
        var world = new World(1, new Arena(100, 100));
        world.Tick();
        world.Tick();
        Assert.Equal(2, world.TickCount);
        Assert.Equal(2f / 60f, world.ElapsedSeconds, 4);
    }

    [Fact]
    public void Clamp_KeepsInsideArena()
    {
        var arena = new Arena(100, 50);
        var clamped = arena.Clamp(new Vec2(1000, -1000));
        Assert.Equal(50, clamped.X);
        Assert.Equal(-25, clamped.Y);
    }

    [Fact]
    public void GameOver_StopsTicking()
    {
        var log = new List<string>();
        var world = new World(1, new Arena(100, 100));
        world.AddSystem(new RecordingSystem(log, "a"));
        world.IsGameOver = true;
        world.Tick();
        Assert.Empty(log);
    }
}

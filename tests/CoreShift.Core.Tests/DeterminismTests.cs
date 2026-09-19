using CoreShift.App;
using CoreShift.Data;
using CoreShift.Data.Content;

namespace CoreShift.Core.Tests;

public class DeterminismTests
{
    private static ulong Run(uint seed, int ticks)
    {
        var world = GameFactory.Build(seed, GameContent.Default());
        var input = new AutoPilotInputSource();
        for (int i = 0; i < ticks && !world.IsGameOver; i++)
        {
            world.Input = input.Poll(world);
            world.Tick();
        }
        return world.ComputeStateHash();
    }

    [Fact]
    public void SameSeed_ProducesIdenticalState()
    {
        Assert.Equal(Run(99, 800), Run(99, 800));
    }

    [Fact]
    public void DifferentSeed_ProducesDifferentState()
    {
        Assert.NotEqual(Run(99, 800), Run(100, 800));
    }
}

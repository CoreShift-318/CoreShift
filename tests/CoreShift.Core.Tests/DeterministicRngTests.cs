using CoreShift.Core.Rng;

namespace CoreShift.Core.Tests;

public class DeterministicRngTests
{
    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var a = new DeterministicRng(12345);
        var b = new DeterministicRng(12345);
        for (int i = 0; i < 100; i++) Assert.Equal(a.NextUInt(), b.NextUInt());
    }

    [Fact]
    public void DifferentSeeds_Diverge()
    {
        var a = new DeterministicRng(1);
        var b = new DeterministicRng(2);
        bool anyDifferent = false;
        for (int i = 0; i < 10; i++)
        {
            if (a.NextUInt() != b.NextUInt()) anyDifferent = true;
        }
        Assert.True(anyDifferent);
    }

    [Fact]
    public void Reseed_RestartsSequence()
    {
        var rng = new DeterministicRng(42);
        uint first = rng.NextUInt();
        rng.Reseed(42);
        Assert.Equal(first, rng.NextUInt());
    }

    [Fact]
    public void NextFloat_IsInUnitInterval()
    {
        var rng = new DeterministicRng(7);
        for (int i = 0; i < 1000; i++)
        {
            Assert.InRange(rng.NextFloat(), 0f, 0.9999999f);
        }
    }

    [Fact]
    public void NextInt_RespectsBounds()
    {
        var rng = new DeterministicRng(9);
        for (int i = 0; i < 1000; i++) Assert.InRange(rng.NextInt(5, 8), 5, 7);
    }
}

using CoreShift.Core.Rng;
using CoreShift.Core.Waves;

namespace CoreShift.Core.Tests;

public class WaveCompositionTests
{
    private static EnemySpec Spec(string id) => new() { Id = id, MaxHealth = 10, Speed = 1, Radius = 0.5f, XpReward = 1 };

    [Fact]
    public void Pick_ReturnsOnlyDefinedEntries()
    {
        var composition = new WaveComposition();
        composition.Entries.Add(new WaveEntry { Spec = Spec("a"), Weight = 1 });
        composition.Entries.Add(new WaveEntry { Spec = Spec("b"), Weight = 1 });

        var rng = new DeterministicRng(3);
        for (int i = 0; i < 200; i++)
        {
            var picked = composition.Pick(rng);
            Assert.Contains(picked.Id, new[] { "a", "b" });
        }
    }

    [Fact]
    public void Pick_RespectsWeightOfSingleEntry()
    {
        var composition = new WaveComposition();
        composition.Entries.Add(new WaveEntry { Spec = Spec("only"), Weight = 5 });
        Assert.Equal("only", composition.Pick(new DeterministicRng(1)).Id);
    }

    [Fact]
    public void Pick_IsDeterministicForSeed()
    {
        var composition = new WaveComposition();
        composition.Entries.Add(new WaveEntry { Spec = Spec("a"), Weight = 2 });
        composition.Entries.Add(new WaveEntry { Spec = Spec("b"), Weight = 1 });

        var first = new List<string>();
        var second = new List<string>();
        var rngA = new DeterministicRng(77);
        var rngB = new DeterministicRng(77);
        for (int i = 0; i < 50; i++)
        {
            first.Add(composition.Pick(rngA).Id);
            second.Add(composition.Pick(rngB).Id);
        }

        Assert.Equal(first, second);
        Assert.Contains("a", first);
        Assert.Contains("b", first);
    }

    [Fact]
    public void Default_Table_PreservesScaling()
    {
        var table = WaveTable.Default;
        Assert.Equal(5, table.PlanFor(1).Count);
        Assert.Equal(11, table.PlanFor(4).Count);
        Assert.Equal(1f, table.PlanFor(1).HealthMultiplier, 3);
        Assert.Equal(1.45f, table.PlanFor(4).HealthMultiplier, 3);
        Assert.Equal("grunt", table.PlanFor(1).Composition.Pick(new DeterministicRng(9)).Id);
    }

    [Fact]
    public void Plan_ClampsBeyondLastWave()
    {
        var table = WaveTable.Default;
        Assert.NotNull(table.PlanFor(999));
        Assert.Equal(200, table.PlanFor(999).Count);
    }

    [Fact]
    public void EmptyTable_ReturnsSafePlan()
    {
        var table = new WaveTable();
        var plan = table.PlanFor(1);
        Assert.Equal(5, plan.Count);
        Assert.NotNull(plan.Composition.Pick(new DeterministicRng(1)));
    }
}

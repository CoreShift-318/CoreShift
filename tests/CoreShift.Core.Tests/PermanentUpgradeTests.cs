using CoreShift.Core.Progression;

namespace CoreShift.Core.Tests;

public class PermanentUpgradeTests
{
    [Fact]
    public void CostFor_GrowsWithRank()
    {
        var state = new ProgressionState();
        var def = PermanentUpgradeCatalog.Definitions()[0];
        int first = state.CostFor(def);

        state.Data.Currency = 1000;
        Assert.True(state.TryPurchase(def));

        Assert.True(state.CostFor(def) > first);
    }

    [Fact]
    public void TrySpend_FailsWhenInsufficient()
    {
        var state = new ProgressionState();
        state.Data.Currency = 5;

        Assert.False(state.TrySpend(10));
        Assert.Equal(5, state.Data.Currency);

        Assert.True(state.TrySpend(5));
        Assert.Equal(0, state.Data.Currency);
    }

    [Fact]
    public void ApplyTo_AddsPurchasedBonuses()
    {
        var state = new ProgressionState();
        state.Data.Currency = 100_000;
        var power = PermanentUpgradeCatalog.Definitions().Find(d => d.Effect == PermanentUpgradeEffect.StartingDamage)!;
        Assert.True(state.TryPurchase(power));

        var stats = new PlayerStats();
        float before = stats.Damage;
        state.ApplyTo(stats);

        Assert.Equal(before + power.MagnitudePerRank, stats.Damage, 3);
    }

    [Fact]
    public void ApplyTo_ProvidesStartingHealth()
    {
        var state = new ProgressionState();
        state.Data.Currency = 100_000;
        var hull = PermanentUpgradeCatalog.Definitions().Find(d => d.Effect == PermanentUpgradeEffect.StartingHealth)!;
        state.TryPurchase(hull);

        var stats = new PlayerStats();
        state.ApplyTo(stats);

        Assert.Equal(120f, stats.MaxHealth, 3);
    }

    [Fact]
    public void TryPurchase_StopsAtMaxRank_AndChargesOnlyForValidRanks()
    {
        var state = new ProgressionState();
        state.Data.Currency = 1_000_000;
        var def = PermanentUpgradeCatalog.Definitions()[0];

        for (int i = 0; i < def.MaxRank + 3; i++) state.TryPurchase(def);

        Assert.Equal(def.MaxRank, state.GetPermanentRank(def.Id));
    }

    [Fact]
    public void TryPurchase_FailsWithoutCurrency()
    {
        var state = new ProgressionState();
        state.Data.Currency = 0;
        var def = PermanentUpgradeCatalog.Definitions()[0];

        Assert.False(state.TryPurchase(def));
        Assert.Equal(0, state.GetPermanentRank(def.Id));
    }
}

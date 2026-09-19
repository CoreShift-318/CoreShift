using CoreShift.Core.Upgrades;

namespace CoreShift.Core.Tests;

public class UpgradeSystemTests
{
    private static List<UpgradeDef> SampleDefs() => new()
    {
        new UpgradeDef { Id = "hp", Name = "+HP", Description = "", Rarity = 1, Kind = UpgradeKind.MaxHealth, Magnitude = 10f, MaxStacks = 5 },
        new UpgradeDef { Id = "spd", Name = "+Spd", Description = "", Rarity = 1, Kind = UpgradeKind.MoveSpeed, Magnitude = 1f, MaxStacks = 5 },
        new UpgradeDef { Id = "dmg", Name = "+Dmg", Description = "", Rarity = 1, Kind = UpgradeKind.Damage, Magnitude = 2f, MaxStacks = 5 },
        new UpgradeDef { Id = "fr", Name = "+FR", Description = "", Rarity = 1, Kind = UpgradeKind.FireRate, Magnitude = 0.5f, MaxStacks = 5 },
    };

    [Fact]
    public void Offer_GivesThreeDistinctChoices()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        var system = new UpgradeSystem(SampleDefs());

        system.Offer(world);

        Assert.Equal(3, system.CurrentOffers.Count);
        Assert.Equal(3, new HashSet<string>(system.CurrentOffers.Select(o => o.Id)).Count);
    }

    [Fact]
    public void Choose_AppliesEffect_AndTracksStacks()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        float before = world.Stats.Damage;

        var system = new UpgradeSystem(new List<UpgradeDef>
        {
            new UpgradeDef { Id = "dmg", Name = "+Dmg", Description = "", Rarity = 1, Kind = UpgradeKind.Damage, Magnitude = 2f, MaxStacks = 1 },
        });

        system.Offer(world);
        system.Choose(world, 0);

        Assert.Equal(before + 2f, world.Stats.Damage, 3);
        Assert.Equal(1, world.UpgradeStacks["dmg"]);
    }

    [Fact]
    public void MaxStacks_ExcludesDef()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        var defs = new List<UpgradeDef>
        {
            new UpgradeDef { Id = "a", Name = "A", Description = "", Rarity = 1, Kind = UpgradeKind.Damage, Magnitude = 1f, MaxStacks = 1 },
            new UpgradeDef { Id = "b", Name = "B", Description = "", Rarity = 1, Kind = UpgradeKind.MoveSpeed, Magnitude = 1f, MaxStacks = 1 },
            new UpgradeDef { Id = "c", Name = "C", Description = "", Rarity = 1, Kind = UpgradeKind.FireRate, Magnitude = 1f, MaxStacks = 1 },
            new UpgradeDef { Id = "d", Name = "D", Description = "", Rarity = 1, Kind = UpgradeKind.XpGain, Magnitude = 1f, MaxStacks = 1 },
        };
        var system = new UpgradeSystem(defs, guaranteed: true);

        system.Offer(world);
        system.Choose(world, 0);
        world.PendingLevelUps = 1;
        system.Update(world, 0f);

        Assert.DoesNotContain(system.CurrentOffers, o => o.Id == "a");
    }

    [Fact]
    public void MaxHealth_Upgrade_UpdatesPlayerHealth()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        var system = new UpgradeSystem(new List<UpgradeDef>
        {
            new UpgradeDef { Id = "hp", Name = "+HP", Description = "", Rarity = 1, Kind = UpgradeKind.MaxHealth, Magnitude = 20f, MaxStacks = 1 },
        }, guaranteed: true);

        system.Offer(world);
        system.Choose(world, 0);

        var health = world.Entities.Get<CoreShift.Core.Ecs.Health>(world.Player);
        Assert.Equal(120f, health.Max, 3);
        Assert.Equal(120f, health.Current, 3);
    }
}

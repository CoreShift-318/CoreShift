using CoreShift.Core;
using CoreShift.Core.Progression;
using CoreShift.Core.Upgrades;

namespace CoreShift.Core.Tests;

public class UpgradeAndCharacterTests
{
    private static List<UpgradeDef> Defs() => new()
    {
        new UpgradeDef { Id = "a", Name = "A", Description = "", Rarity = 1, Kind = UpgradeKind.Damage, Magnitude = 1f, MaxStacks = 5 },
        new UpgradeDef { Id = "b", Name = "B", Description = "", Rarity = 1, Kind = UpgradeKind.MoveSpeed, Magnitude = 1f, MaxStacks = 5 },
        new UpgradeDef { Id = "c", Name = "C", Description = "", Rarity = 1, Kind = UpgradeKind.FireRate, Magnitude = 1f, MaxStacks = 5 },
        new UpgradeDef { Id = "d", Name = "D", Description = "", Rarity = 1, Kind = UpgradeKind.XpGain, Magnitude = 1f, MaxStacks = 5 },
    };

    [Fact]
    public void Banish_ExcludesDefFromLaterOffers()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        var system = new UpgradeSystem(Defs(), guaranteed: true);

        system.Offer(world);
        string banned = system.CurrentOffers[0].Id;
        Assert.True(system.Banish(world, 0));

        for (int i = 0; i < 20; i++)
        {
            system.Offer(world);
            Assert.DoesNotContain(system.CurrentOffers, o => o.Id == banned);
        }
    }

    [Fact]
    public void Reroll_ProducesOffers()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        var system = new UpgradeSystem(Defs());

        system.Offer(world);
        system.Reroll(world);

        Assert.True(system.CurrentOffers.Count > 0);
    }

    [Fact]
    public void Character_Apply_SetsStatsAndLoadout()
    {
        var character = CharacterCatalog.Find("bruiser");
        Assert.NotNull(character);

        var stats = new PlayerStats();
        float healthBefore = stats.MaxHealth;
        CharacterCatalog.Apply(character!, stats);

        Assert.Equal(healthBefore + character!.MaxHealthBonus, stats.MaxHealth, 3);
        Assert.Equal(WeaponKind.Shotgun, stats.Weapon);
    }

    [Fact]
    public void CharacterCatalog_FindUnknown_ReturnsNull()
    {
        Assert.Null(CharacterCatalog.Find("nobody"));
    }

    [Fact]
    public void CompleteRun_RecordsLeaderboard()
    {
        var progression = new ProgressionState();
        progression.CompleteRun(5, 20, 100, 0);
        progression.CompleteRun(9, 30, 200, 0);
        progression.CompleteRun(3, 5, 40, 0);

        Assert.Equal(9, progression.Data.Runs[0].Wave);
        Assert.Equal(5, progression.Data.Runs[1].Wave);
    }
}

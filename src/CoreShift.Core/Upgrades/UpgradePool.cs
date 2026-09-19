namespace CoreShift.Core.Upgrades;

public static class UpgradePool
{
    public static List<UpgradeDef> Defaults() => new()
    {
        new UpgradeDef { Id = "vitality", Name = "Vitality", Description = "+20 max health", Rarity = 1, Kind = UpgradeKind.MaxHealth, Magnitude = 20f, MaxStacks = 5 },
        new UpgradeDef { Id = "swiftness", Name = "Swiftness", Description = "+15% move speed", Rarity = 1, Kind = UpgradeKind.MoveSpeed, Magnitude = 0.9f, MaxStacks = 5 },
        new UpgradeDef { Id = "might", Name = "Might", Description = "+25% damage", Rarity = 1, Kind = UpgradeKind.Damage, Magnitude = 2.5f, MaxStacks = 5 },
        new UpgradeDef { Id = "haste", Name = "Haste", Description = "+20% fire rate", Rarity = 2, Kind = UpgradeKind.FireRate, Magnitude = 1.6f, MaxStacks = 4 },
        new UpgradeDef { Id = "multishot", Name = "Multishot", Description = "+1 projectile", Rarity = 3, Kind = UpgradeKind.ProjectileCount, Magnitude = 1f, MaxStacks = 3 },
        new UpgradeDef { Id = "scholar", Name = "Scholar", Description = "+25% experience gain", Rarity = 2, Kind = UpgradeKind.XpGain, Magnitude = 0.25f, MaxStacks = 3 },
        new UpgradeDef { Id = "precision", Name = "Precision", Description = "+8% critical chance", Rarity = 2, Kind = UpgradeKind.CritChance, Magnitude = 0.08f, MaxStacks = 5 },
        new UpgradeDef { Id = "execution", Name = "Execution", Description = "+50% critical damage", Rarity = 3, Kind = UpgradeKind.CritDamage, Magnitude = 0.5f, MaxStacks = 4 },
        new UpgradeDef { Id = "incendiary", Name = "Incendiary Rounds", Description = "Shots set enemies on fire", Rarity = 2, Kind = UpgradeKind.BurnRounds, Magnitude = 6f, MaxStacks = 3 },
        new UpgradeDef { Id = "cryo", Name = "Cryo Rounds", Description = "Shots slow enemies", Rarity = 2, Kind = UpgradeKind.SlowRounds, Magnitude = 0.55f, MaxStacks = 2 },
        new UpgradeDef { Id = "magnet", Name = "Magnet", Description = "+1.5 pickup radius", Rarity = 1, Kind = UpgradeKind.PickupRadius, Magnitude = 1.5f, MaxStacks = 4 },
        new UpgradeDef { Id = "weapon_shotgun", Name = "Scatter Cannon", Description = "Swap to a 5-pellet shotgun", Rarity = 3, Kind = UpgradeKind.WeaponShotgun, Magnitude = 0f, MaxStacks = 1 },
        new UpgradeDef { Id = "weapon_laser", Name = "Piercing Laser", Description = "Swap to a piercing laser", Rarity = 3, Kind = UpgradeKind.WeaponLaser, Magnitude = 0f, MaxStacks = 1 },
    };
}

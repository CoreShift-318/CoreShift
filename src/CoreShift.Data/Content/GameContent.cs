using CoreShift.Core.Combat;
using CoreShift.Core.Upgrades;
using CoreShift.Core.Waves;

namespace CoreShift.Data.Content;

public sealed class GameContent
{
    public List<EnemyDefinition> Enemies { get; set; } = new();
    public List<UpgradeDefinition> Upgrades { get; set; } = new();
    public List<WaveDefinition> Waves { get; set; } = new();

    public static GameContent Default() => new()
    {
        Enemies =
        {
            new EnemyDefinition { Id = "grunt", Name = "Grunt", MaxHealth = 20f, Speed = 2f, Damage = 5f, Radius = 0.5f, XpReward = 1, Color = unchecked((int)0xFFE06060) },
            new EnemyDefinition { Id = "brute", Name = "Brute", MaxHealth = 60f, Speed = 1.2f, Damage = 12f, Radius = 0.9f, XpReward = 4, Color = unchecked((int)0xFFB040B0), OnHitStatus = StatusKind.Slow, OnHitMagnitude = 0.5f, OnHitDuration = 2f, OnHitChance = 0.6f },
            new EnemyDefinition { Id = "wisp", Name = "Wisp", MaxHealth = 8f, Speed = 4f, Damage = 3f, Radius = 0.35f, XpReward = 2, Color = unchecked((int)0xFF60C0FF), OnHitStatus = StatusKind.Shock, OnHitMagnitude = 0.25f, OnHitDuration = 3f, OnHitChance = 0.5f },
        },
        Upgrades =
        {
            new UpgradeDefinition { Id = "vitality", Name = "Vitality", Description = "+20 max health", Rarity = 1, Kind = UpgradeKind.MaxHealth, Magnitude = 20f, MaxStacks = 5 },
            new UpgradeDefinition { Id = "swiftness", Name = "Swiftness", Description = "+15% move speed", Rarity = 1, Kind = UpgradeKind.MoveSpeed, Magnitude = 0.9f, MaxStacks = 5 },
            new UpgradeDefinition { Id = "might", Name = "Might", Description = "+25% damage", Rarity = 1, Kind = UpgradeKind.Damage, Magnitude = 2.5f, MaxStacks = 5 },
            new UpgradeDefinition { Id = "haste", Name = "Haste", Description = "+20% fire rate", Rarity = 2, Kind = UpgradeKind.FireRate, Magnitude = 1.6f, MaxStacks = 4 },
            new UpgradeDefinition { Id = "multishot", Name = "Multishot", Description = "+1 projectile", Rarity = 3, Kind = UpgradeKind.ProjectileCount, Magnitude = 1f, MaxStacks = 3 },
            new UpgradeDefinition { Id = "scholar", Name = "Scholar", Description = "+25% experience gain", Rarity = 2, Kind = UpgradeKind.XpGain, Magnitude = 0.25f, MaxStacks = 3 },
            new UpgradeDefinition { Id = "precision", Name = "Precision", Description = "+8% critical chance", Rarity = 2, Kind = UpgradeKind.CritChance, Magnitude = 0.08f, MaxStacks = 5 },
            new UpgradeDefinition { Id = "execution", Name = "Execution", Description = "+50% critical damage", Rarity = 3, Kind = UpgradeKind.CritDamage, Magnitude = 0.5f, MaxStacks = 4 },
            new UpgradeDefinition { Id = "incendiary", Name = "Incendiary Rounds", Description = "Shots set enemies on fire", Rarity = 2, Kind = UpgradeKind.BurnRounds, Magnitude = 6f, MaxStacks = 3 },
            new UpgradeDefinition { Id = "cryo", Name = "Cryo Rounds", Description = "Shots slow enemies", Rarity = 2, Kind = UpgradeKind.SlowRounds, Magnitude = 0.55f, MaxStacks = 2 },
            new UpgradeDefinition { Id = "magnet", Name = "Magnet", Description = "+1.5 pickup radius", Rarity = 1, Kind = UpgradeKind.PickupRadius, Magnitude = 1.5f, MaxStacks = 4 },
            new UpgradeDefinition { Id = "weapon_shotgun", Name = "Scatter Cannon", Description = "Swap to a 5-pellet shotgun", Rarity = 3, Kind = UpgradeKind.WeaponShotgun, Magnitude = 0f, MaxStacks = 1 },
            new UpgradeDefinition { Id = "weapon_laser", Name = "Piercing Laser", Description = "Swap to a piercing laser", Rarity = 3, Kind = UpgradeKind.WeaponLaser, Magnitude = 0f, MaxStacks = 1 },
        },
        Waves =
        {
            new WaveDefinition { Wave = 1, EnemyId = "grunt", Count = 5, SpawnInterval = 1.2f },
            new WaveDefinition { Wave = 2, EnemyId = "grunt", Count = 7, SpawnInterval = 1.15f },
            new WaveDefinition { Wave = 3, EnemyId = "wisp", Count = 9, SpawnInterval = 1.1f },
            new WaveDefinition { Wave = 4, EnemyId = "brute", Count = 11, SpawnInterval = 1.05f },
        },
    };

    public EnemySpec PrimaryEnemySpec()
    {
        return Enemies.Count > 0 ? Enemies[0].ToSpec() : new EnemySpec();
    }

    public EnemySpec? FindEnemySpec(string id)
    {
        for (int i = 0; i < Enemies.Count; i++)
        {
            if (Enemies[i].Id == id) return Enemies[i].ToSpec();
        }
        return null;
    }

    public List<EnemySpec> ToEnemySpecs()
    {
        var specs = new List<EnemySpec>(Enemies.Count);
        for (int i = 0; i < Enemies.Count; i++) specs.Add(Enemies[i].ToSpec());
        return specs;
    }

    public List<UpgradeDef> ToUpgradeDefs()
    {
        var defs = new List<UpgradeDef>(Upgrades.Count);
        for (int i = 0; i < Upgrades.Count; i++) defs.Add(Upgrades[i].ToUpgradeDef());
        return defs;
    }

    public void Validate()
    {
        if (Enemies.Count == 0) throw new ContentLoadException("Content contains no enemies.");
        if (Upgrades.Count == 0) throw new ContentLoadException("Content contains no upgrades.");

        var enemyIds = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < Enemies.Count; i++)
        {
            var enemy = Enemies[i];
            if (string.IsNullOrWhiteSpace(enemy.Id)) throw new ContentLoadException("Enemy at index " + i + " has an empty id.");
            if (!enemyIds.Add(enemy.Id)) throw new ContentLoadException("Duplicate enemy id: '" + enemy.Id + "'.");
            if (enemy.MaxHealth <= 0f) throw new ContentLoadException("Enemy '" + enemy.Id + "' has non-positive health.");
            if (enemy.Radius <= 0f) throw new ContentLoadException("Enemy '" + enemy.Id + "' has non-positive radius.");
        }

        var upgradeIds = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < Upgrades.Count; i++)
        {
            var upgrade = Upgrades[i];
            if (string.IsNullOrWhiteSpace(upgrade.Id)) throw new ContentLoadException("Upgrade at index " + i + " has an empty id.");
            if (!upgradeIds.Add(upgrade.Id)) throw new ContentLoadException("Duplicate upgrade id: '" + upgrade.Id + "'.");
        }

        for (int i = 0; i < Waves.Count; i++)
        {
            var wave = Waves[i];
            if (!enemyIds.Contains(wave.EnemyId))
            {
                throw new ContentLoadException("Wave " + wave.Wave + " references unknown enemy id '" + wave.EnemyId + "'.");
            }
        }
    }
}

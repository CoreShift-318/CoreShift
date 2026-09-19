namespace CoreShift.Core.Progression;

public enum PermanentUpgradeEffect
{
    StartingDamage,
    StartingHealth,
    StartingSpeed,
}

public sealed class PermanentUpgradeDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxRank { get; set; } = 5;
    public int BaseCost { get; set; } = 50;
    public float CostGrowth { get; set; } = 1.6f;
    public PermanentUpgradeEffect Effect { get; set; }
    public float MagnitudePerRank { get; set; } = 1f;
}

public static class PermanentUpgradeCatalog
{
    public static List<PermanentUpgradeDef> Definitions() => new()
    {
        new PermanentUpgradeDef
        {
            Id = "power", Name = "Power Core", Description = "+2 starting damage",
            MaxRank = 5, BaseCost = 50, CostGrowth = 1.6f,
            Effect = PermanentUpgradeEffect.StartingDamage, MagnitudePerRank = 2f,
        },
        new PermanentUpgradeDef
        {
            Id = "vitality", Name = "Reinforced Hull", Description = "+20 starting health",
            MaxRank = 5, BaseCost = 50, CostGrowth = 1.6f,
            Effect = PermanentUpgradeEffect.StartingHealth, MagnitudePerRank = 20f,
        },
        new PermanentUpgradeDef
        {
            Id = "thrusters", Name = "Ion Thrusters", Description = "+0.5 starting move speed",
            MaxRank = 5, BaseCost = 50, CostGrowth = 1.6f,
            Effect = PermanentUpgradeEffect.StartingSpeed, MagnitudePerRank = 0.5f,
        },
    };
}

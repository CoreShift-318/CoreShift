namespace CoreShift.Core.Upgrades;

public sealed class UpgradeDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Rarity { get; set; } = 1;
    public UpgradeKind Kind { get; set; }
    public float Magnitude { get; set; }
    public int MaxStacks { get; set; } = 5;

    public int Weight => System.Math.Max(1, 4 - Rarity);
}

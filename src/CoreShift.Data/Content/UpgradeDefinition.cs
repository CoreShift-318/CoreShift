using CoreShift.Core.Upgrades;

namespace CoreShift.Data.Content;

public sealed class UpgradeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Rarity { get; set; } = 1;
    public UpgradeKind Kind { get; set; }
    public float Magnitude { get; set; }
    public int MaxStacks { get; set; } = 5;

    public UpgradeDef ToUpgradeDef() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        Rarity = Rarity,
        Kind = Kind,
        Magnitude = Magnitude,
        MaxStacks = MaxStacks,
    };
}

using CoreShift.Core.Combat;
using CoreShift.Core.Waves;

namespace CoreShift.Data.Content;

public sealed class EnemyDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float MaxHealth { get; set; } = 20f;
    public float Speed { get; set; } = 2f;
    public float Damage { get; set; } = 5f;
    public float Radius { get; set; } = 0.5f;
    public int XpReward { get; set; } = 1;
    public int Color { get; set; } = unchecked((int)0xFFE06060);

    public StatusKind OnHitStatus { get; set; } = StatusKind.None;
    public float OnHitMagnitude { get; set; }
    public float OnHitDuration { get; set; }
    public float OnHitChance { get; set; } = 1f;

    public EnemySpec ToSpec() => new()
    {
        Id = Id,
        Name = Name,
        MaxHealth = MaxHealth,
        Speed = Speed,
        Damage = Damage,
        Radius = Radius,
        XpReward = XpReward,
        Color = Color,
        OnHitStatus = OnHitStatus,
        OnHitMagnitude = OnHitMagnitude,
        OnHitDuration = OnHitDuration,
        OnHitChance = OnHitChance,
    };
}

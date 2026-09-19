using CoreShift.Core.Ecs;
using CoreShift.Core.Math;

namespace CoreShift.Core.Combat;

public readonly struct HitEvent
{
    public readonly Entity Source;
    public readonly Entity Target;
    public readonly float Damage;
    public readonly bool IsCrit;
    public readonly Vec2 Knockback;
    public readonly StatusApplication OnHit;

    public HitEvent(Entity source, Entity target, float damage)
        : this(source, target, damage, false, Vec2.Zero, StatusApplication.None)
    {
    }

    public HitEvent(Entity source, Entity target, float damage, bool isCrit, Vec2 knockback, StatusApplication onHit)
    {
        Source = source;
        Target = target;
        Damage = damage;
        IsCrit = isCrit;
        Knockback = knockback;
        OnHit = onHit;
    }
}

public readonly struct DamageEvent
{
    public readonly Entity Target;
    public readonly float Amount;
    public readonly bool IsCrit;
    public readonly Vec2 Position;

    public DamageEvent(Entity target, float amount, bool isCrit, Vec2 position)
    {
        Target = target;
        Amount = amount;
        IsCrit = isCrit;
        Position = position;
    }
}

public readonly struct HealEvent
{
    public readonly Entity Target;
    public readonly float Amount;
    public readonly Vec2 Position;

    public HealEvent(Entity target, float amount, Vec2 position)
    {
        Target = target;
        Amount = amount;
        Position = position;
    }
}

public readonly struct DeathEvent
{
    public readonly Vec2 Position;
    public readonly int Color;

    public DeathEvent(Vec2 position, int color)
    {
        Position = position;
        Color = color;
    }
}

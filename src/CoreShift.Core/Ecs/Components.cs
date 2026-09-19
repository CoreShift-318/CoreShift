using CoreShift.Core.Combat;

namespace CoreShift.Core.Ecs;

public struct Transform2
{
    public float X;
    public float Y;

    public Transform2(float x, float y)
    {
        X = x;
        Y = y;
    }
}

public struct Velocity
{
    public float X;
    public float Y;

    public Velocity(float x, float y)
    {
        X = x;
        Y = y;
    }
}

public struct Health
{
    public float Current;
    public float Max;

    public Health(float max)
    {
        Max = max;
        Current = max;
    }

    public Health(float current, float max)
    {
        Current = current;
        Max = max;
    }

    public bool IsDead => Current <= 0f;
}

public struct Collider
{
    public float Radius;

    public Collider(float radius)
    {
        Radius = radius;
    }
}

public struct Damage
{
    public float Amount;
    public bool IsCrit;
    public float Knockback;
    public bool Pierce;
    public StatusApplication OnHit;

    public Damage(float amount)
    {
        Amount = amount;
        IsCrit = false;
        Knockback = 0f;
        Pierce = false;
        OnHit = StatusApplication.None;
    }
}

public struct Lifetime
{
    public float Remaining;

    public Lifetime(float seconds)
    {
        Remaining = seconds;
    }
}

public struct XpReward
{
    public int Amount;

    public XpReward(int amount)
    {
        Amount = amount;
    }
}

public struct EnemyAi
{
    public float Speed;

    public EnemyAi(float speed)
    {
        Speed = speed;
    }
}

public struct Cooldown
{
    public float Remaining;
    public float Interval;

    public Cooldown(float interval)
    {
        Interval = interval;
        Remaining = 0f;
    }
}

public struct PlayerTag { }

public struct AimDirection
{
    public float X;
    public float Y;
}

public struct EnemyTag { }

public struct ProjectileTag { }

public struct EffectTag { }

public struct PickupTag { }

public struct BurnStatus
{
    public float Remaining;
    public float TickTimer;
    public float DamagePerSecond;
}

public struct PoisonStatus
{
    public float Remaining;
    public float TickTimer;
    public float DamagePerSecond;
    public int Stacks;
}

public struct SlowStatus
{
    public float Remaining;
    public float Factor;
}

public struct ShockStatus
{
    public float Remaining;
    public float Amplifier;
}

public struct StunStatus
{
    public float Remaining;
}

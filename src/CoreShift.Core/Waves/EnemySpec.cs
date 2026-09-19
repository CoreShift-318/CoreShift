using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;

namespace CoreShift.Core.Waves;

public sealed class EnemySpec
{
    public string Id = "grunt";
    public string Name = "Grunt";
    public float MaxHealth = 20f;
    public float Speed = 2f;
    public float Damage = 5f;
    public float Radius = 0.5f;
    public int XpReward = 1;
    public int Color = unchecked((int)0xFFE06060);

    public StatusKind OnHitStatus = StatusKind.None;
    public float OnHitMagnitude;
    public float OnHitDuration;
    public float OnHitChance = 1f;
}

public sealed class EnemyInstance
{
    public Entity Entity = Entity.Null;
    public EnemySpec Spec = new();
    public int Wave;
}

public struct EnemyLink
{
    public EnemyInstance Instance;
}

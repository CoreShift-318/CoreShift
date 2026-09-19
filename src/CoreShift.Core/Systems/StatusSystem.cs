using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;

namespace CoreShift.Core.Systems;

public sealed class StatusSystem : ISystem
{
    private readonly List<Entity> _expired = new();

    public void Update(World world, float dt)
    {
        TickBurn(world, dt);
        TickPoison(world, dt);
        TickSlow(world, dt);
        TickShock(world, dt);
        TickStun(world, dt);
    }

    public static void Apply(World world, Entity target, StatusApplication application)
    {
        if (application.IsNone) return;
        if (!world.Entities.IsAlive(target)) return;
        if (application.Chance < 1f && !world.Rng.Chance(application.Chance)) return;

        switch (application.Kind)
        {
            case StatusKind.Burn:
                if (world.Entities.Has<BurnStatus>(target))
                {
                    ref var burn = ref world.Entities.GetRef<BurnStatus>(target);
                    burn.Remaining = MathF.Max(burn.Remaining, application.Duration);
                    burn.DamagePerSecond = MathF.Max(burn.DamagePerSecond, application.Magnitude);
                }
                else
                {
                    world.Entities.Set(target, new BurnStatus
                    {
                        Remaining = application.Duration,
                        TickTimer = 0f,
                        DamagePerSecond = application.Magnitude,
                    });
                }
                break;

            case StatusKind.Poison:
                if (world.Entities.Has<PoisonStatus>(target))
                {
                    ref var poison = ref world.Entities.GetRef<PoisonStatus>(target);
                    poison.Remaining = MathF.Max(poison.Remaining, application.Duration);
                    poison.DamagePerSecond = MathF.Max(poison.DamagePerSecond, application.Magnitude);
                    poison.Stacks++;
                }
                else
                {
                    world.Entities.Set(target, new PoisonStatus
                    {
                        Remaining = application.Duration,
                        TickTimer = 0f,
                        DamagePerSecond = application.Magnitude,
                        Stacks = 1,
                    });
                }
                break;

            case StatusKind.Slow:
                if (world.Entities.Has<SlowStatus>(target))
                {
                    ref var slow = ref world.Entities.GetRef<SlowStatus>(target);
                    slow.Remaining = MathF.Max(slow.Remaining, application.Duration);
                    slow.Factor = MathF.Min(slow.Factor, System.Math.Clamp(application.Magnitude, 0f, 1f));
                }
                else
                {
                    world.Entities.Set(target, new SlowStatus
                    {
                        Remaining = application.Duration,
                        Factor = System.Math.Clamp(application.Magnitude, 0f, 1f),
                    });
                }
                break;

            case StatusKind.Shock:
                if (world.Entities.Has<ShockStatus>(target))
                {
                    ref var shock = ref world.Entities.GetRef<ShockStatus>(target);
                    shock.Remaining = MathF.Max(shock.Remaining, application.Duration);
                    shock.Amplifier = MathF.Max(shock.Amplifier, application.Magnitude);
                }
                else
                {
                    world.Entities.Set(target, new ShockStatus
                    {
                        Remaining = application.Duration,
                        Amplifier = application.Magnitude,
                    });
                }
                break;

            case StatusKind.Stun:
                if (world.Entities.Has<StunStatus>(target))
                {
                    ref var stun = ref world.Entities.GetRef<StunStatus>(target);
                    stun.Remaining = MathF.Max(stun.Remaining, application.Duration);
                }
                else
                {
                    world.Entities.Set(target, new StunStatus { Remaining = application.Duration });
                }
                break;
        }
    }

    private void TickBurn(World world, float dt)
    {
        _expired.Clear();
        foreach (var entity in world.Entities.With<BurnStatus>())
        {
            ref var burn = ref world.Entities.GetRef<BurnStatus>(entity);
            burn.Remaining -= dt;
            burn.TickTimer -= dt;

            if (burn.TickTimer <= 0f)
            {
                burn.TickTimer += 1f;
                if (burn.DamagePerSecond > 0f)
                {
                    world.Hits.Add(new HitEvent(Entity.Null, entity, burn.DamagePerSecond));
                }
            }

            if (burn.Remaining <= 0f) _expired.Add(entity);
        }
        RemoveExpired<BurnStatus>(world);
    }

    private void TickPoison(World world, float dt)
    {
        _expired.Clear();
        foreach (var entity in world.Entities.With<PoisonStatus>())
        {
            ref var poison = ref world.Entities.GetRef<PoisonStatus>(entity);
            poison.Remaining -= dt;
            poison.TickTimer -= dt;

            if (poison.TickTimer <= 0f)
            {
                poison.TickTimer += 1f;
                int stacks = System.Math.Max(1, poison.Stacks);
                float damage = poison.DamagePerSecond * stacks;
                if (damage > 0f)
                {
                    world.Hits.Add(new HitEvent(Entity.Null, entity, damage));
                }
            }

            if (poison.Remaining <= 0f) _expired.Add(entity);
        }
        RemoveExpired<PoisonStatus>(world);
    }

    private void TickSlow(World world, float dt)
    {
        _expired.Clear();
        foreach (var entity in world.Entities.With<SlowStatus>())
        {
            ref var slow = ref world.Entities.GetRef<SlowStatus>(entity);
            slow.Remaining -= dt;
            if (slow.Remaining <= 0f) _expired.Add(entity);
        }
        RemoveExpired<SlowStatus>(world);
    }

    private void TickShock(World world, float dt)
    {
        _expired.Clear();
        foreach (var entity in world.Entities.With<ShockStatus>())
        {
            ref var shock = ref world.Entities.GetRef<ShockStatus>(entity);
            shock.Remaining -= dt;
            if (shock.Remaining <= 0f) _expired.Add(entity);
        }
        RemoveExpired<ShockStatus>(world);
    }

    private void TickStun(World world, float dt)
    {
        _expired.Clear();
        foreach (var entity in world.Entities.With<StunStatus>())
        {
            ref var stun = ref world.Entities.GetRef<StunStatus>(entity);
            stun.Remaining -= dt;
            if (stun.Remaining <= 0f) _expired.Add(entity);
        }
        RemoveExpired<StunStatus>(world);
    }

    private void RemoveExpired<T>(World world) where T : struct
    {
        for (int i = 0; i < _expired.Count; i++)
        {
            world.Entities.Remove<T>(_expired[i]);
        }
    }
}

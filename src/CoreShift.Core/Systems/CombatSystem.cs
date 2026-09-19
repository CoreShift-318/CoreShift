using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Math;
using CoreShift.Core.Waves;

namespace CoreShift.Core.Systems;

public sealed class CombatSystem : ISystem
{
    public void Update(World world, float dt)
    {
        for (int i = 0; i < world.Hits.Count; i++)
        {
            var hit = world.Hits[i];
            var target = hit.Target;

            if (!world.Entities.IsAlive(target)) continue;
            if (!world.Entities.Has<Health>(target)) continue;

            ref var health = ref world.Entities.GetRef<Health>(target);
            if (health.Current <= 0f) continue;

            float damage = hit.Damage;
            if (world.Entities.Has<ShockStatus>(target))
            {
                damage *= 1f + world.Entities.Get<ShockStatus>(target).Amplifier;
            }

            health.Current -= damage;

            var position = PositionOf(world, target);
            world.DamageEvents.Add(new DamageEvent(target, damage, hit.IsCrit, position));

            if (hit.Knockback.Length > 1e-4f && world.Entities.Has<Velocity>(target))
            {
                ref var velocity = ref world.Entities.GetRef<Velocity>(target);
                velocity.X += hit.Knockback.X;
                velocity.Y += hit.Knockback.Y;
            }

            if (!hit.OnHit.IsNone)
            {
                StatusSystem.Apply(world, target, hit.OnHit);
            }

            if (!health.IsDead) continue;

            if (world.Entities.Has<EnemyTag>(target))
            {
                world.Kills++;
                if (world.Entities.Has<XpReward>(target))
                {
                    int reward = world.Entities.Get<XpReward>(target).Amount;
                    AddXp(world, (int)MathF.Round(reward * world.Stats.XpMultiplier));
                }
                world.Deaths.Add(BuildDeathEvent(world, target, position));
                CleanupSystem.DestroyEntity(world, target);
            }
            else if (target == world.Player)
            {
                world.IsGameOver = true;
            }
        }
    }

    private static Vec2 PositionOf(World world, Entity entity)
    {
        if (!world.Entities.Has<Transform2>(entity)) return Vec2.Zero;
        var transform = world.Entities.Get<Transform2>(entity);
        return new Vec2(transform.X, transform.Y);
    }

    private static void AddXp(World world, int amount)
    {
        if (amount <= 0) return;
        world.Xp += amount;
        while (world.Xp >= world.XpToNext)
        {
            world.Xp -= (int)world.XpToNext;
            world.Level++;
            world.PendingLevelUps++;
            world.XpToNext = World.XpRequiredFor(world.Level);
        }
    }

    private static DeathEvent BuildDeathEvent(World world, Entity target, Vec2 position)
    {
        int color = unchecked((int)0xFFE06060);
        if (world.Entities.Has<EnemyLink>(target))
        {
            color = world.Entities.Get<EnemyLink>(target).Instance.Spec.Color;
        }

        return new DeathEvent(position, color);
    }
}

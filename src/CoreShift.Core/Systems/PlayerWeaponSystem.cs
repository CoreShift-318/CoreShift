using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Math;

namespace CoreShift.Core.Systems;

public sealed class PlayerWeaponSystem : ISystem
{
    public const float ProjectileSpeed = 32f;
    private const float ProjectileLifetime = 2f;

    private static Vec2 ResolveAim(World world)
    {
        Vec2 aim;
        var inputAim = new Vec2(world.Input.AimX, world.Input.AimY);

        if (inputAim.Length > 1e-3f)
        {
            aim = inputAim.Normalized;
        }
        else if (world.Entities.Has<AimDirection>(world.Player))
        {
            var stored = world.Entities.Get<AimDirection>(world.Player);
            var storedAim = new Vec2(stored.X, stored.Y);
            aim = storedAim.Length > 1e-3f ? storedAim.Normalized : new Vec2(1f, 0f);
        }
        else
        {
            aim = new Vec2(1f, 0f);
        }

        aim = ApplyAimAssist(world, aim);
        world.Entities.Set(world.Player, new AimDirection { X = aim.X, Y = aim.Y });
        return aim;
    }

    private static Vec2 ApplyAimAssist(World world, Vec2 aim)
    {
        float assist = world.Stats.AimAssist;
        if (assist <= 0f || !world.Entities.Has<Transform2>(world.Player)) return aim;

        var playerTransform = world.Entities.Get<Transform2>(world.Player);
        var playerPos = new Vec2(playerTransform.X, playerTransform.Y);

        Vec2 bestDirection = aim;
        float bestDistance = float.MaxValue;
        bool found = false;

        foreach (var entity in world.Entities.With<EnemyTag>())
        {
            if (!world.Entities.Has<Transform2>(entity)) continue;
            var et = world.Entities.Get<Transform2>(entity);
            var offset = new Vec2(et.X - playerTransform.X, et.Y - playerTransform.Y);
            float distance = offset.Length;
            if (distance < 1e-4f || distance > 18f) continue;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestDirection = offset.Normalized;
                found = true;
            }
        }

        if (!found) return aim;
        if (Vec2.Dot(aim, bestDirection) < 0.94f) return aim; // only within ~20 degrees
        return Vec2.Lerp(aim, bestDirection, assist).Normalized;
    }

    public void Update(World world, float dt)
    {
        if (!world.HasPlayer) return;
        if (!world.Entities.Has<Cooldown>(world.Player)) return;

        var aim = ResolveAim(world);

        if (world.Entities.Get<Cooldown>(world.Player).Remaining > 0f)
        {
            ref var cooling = ref world.Entities.GetRef<Cooldown>(world.Player);
            cooling.Remaining -= dt;
            return;
        }

        if (!world.Input.Firing) return;

        var playerTransform = world.Entities.Get<Transform2>(world.Player);
        var origin = new Vec2(playerTransform.X, playerTransform.Y);

        float fireRate = MathF.Max(0.01f, world.Stats.FireRate);
        float interval;

        switch (world.Stats.Weapon)
        {
            case WeaponKind.Shotgun:
                interval = 1f / (fireRate * 0.5f);
                FireFan(world, origin, aim, pellets: 5, spreadDegrees: 36f, damageScale: 0.7f,
                    speed: 26f, lifetime: 1.2f, radius: 0.3f, pierce: false);
                break;

            case WeaponKind.Laser:
                interval = 1f / (fireRate * 0.8f);
                FireFan(world, origin, aim, pellets: 1, spreadDegrees: 0f, damageScale: 1f,
                    speed: 55f, lifetime: 1f, radius: 0.18f, pierce: true);
                break;

            default:
                interval = 1f / fireRate;
                FireFan(world, origin, aim, pellets: System.Math.Max(1, world.Stats.ProjectileCount),
                    spreadDegrees: 12f, damageScale: 1f, speed: ProjectileSpeed, lifetime: ProjectileLifetime,
                    radius: 0.25f, pierce: false);
                break;
        }

        ref var cooldown = ref world.Entities.GetRef<Cooldown>(world.Player);
        cooldown.Interval = interval;
        cooldown.Remaining = interval;
    }

    private static void FireFan(World world, Vec2 origin, Vec2 aim, int pellets, float spreadDegrees,
        float damageScale, float speed, float lifetime, float radius, bool pierce)
    {
        var onHit = world.Stats.WeaponStatus == StatusKind.None
            ? StatusApplication.None
            : new StatusApplication(
                world.Stats.WeaponStatus,
                world.Stats.WeaponStatusMagnitude,
                world.Stats.WeaponStatusDuration,
                world.Stats.WeaponStatusChance);

        float spread = spreadDegrees * (MathF.PI / 180f);
        float start = -(pellets - 1) * 0.5f * spread;

        for (int i = 0; i < pellets; i++)
        {
            float angle = MathF.Atan2(aim.Y, aim.X) + start + i * spread;
            var direction = new Vec2(MathF.Cos(angle), MathF.Sin(angle));

            bool crit = world.Rng.Chance(world.Stats.CritChance);
            float damage = (crit ? world.Stats.Damage * world.Stats.CritMultiplier : world.Stats.Damage) * damageScale;

            SpawnProjectile(world, origin, direction, damage, crit, onHit, pierce, speed, lifetime, radius);
        }
    }

    private static void SpawnProjectile(World world, Vec2 origin, Vec2 direction, float damage, bool isCrit,
        StatusApplication onHit, bool pierce, float speed, float lifetime, float radius)
    {
        if (!world.ProjectilePool.TryRent(out var instance)) return;

        var entity = world.Entities.Create();
        instance.Entity = entity;
        world.Entities.Set(entity, new ProjectileTag());
        world.Entities.Set(entity, new Transform2(origin.X, origin.Y));
        world.Entities.Set(entity, new Velocity(direction.X * speed, direction.Y * speed));
        world.Entities.Set(entity, new Collider(radius));
        world.Entities.Set(entity, new Damage(damage) { IsCrit = isCrit, Knockback = world.Stats.Knockback, Pierce = pierce, OnHit = onHit });
        world.Entities.Set(entity, new Lifetime(lifetime));
        world.Entities.Set(entity, new ProjectileLink { Instance = instance });
    }
}

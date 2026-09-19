using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Math;
using CoreShift.Core.Spatial;

namespace CoreShift.Core.Systems;

public sealed class CollisionSystem : ISystem
{
    private readonly SpatialHashGrid _grid = new(2f);
    private readonly List<Entity> _candidates = new();
    private readonly List<Entity> _projectiles = new();

    public void Update(World world, float dt)
    {
        _grid.Clear();

        foreach (var entity in world.Entities.With<Collider>())
        {
            if (!world.Entities.Has<Transform2>(entity)) continue;
            var t = world.Entities.Get<Transform2>(entity);
            _grid.Insert(entity, new Vec2(t.X, t.Y));
        }

        ResolveProjectileHits(world);
        ResolveEnemyContact(world);
    }

    private void ResolveProjectileHits(World world)
    {
        _projectiles.Clear();
        foreach (var projectile in world.Entities.With<ProjectileTag>())
        {
            _projectiles.Add(projectile);
        }

        for (int p = 0; p < _projectiles.Count; p++)
        {
            var projectile = _projectiles[p];
            if (!world.Entities.IsAlive(projectile)) continue;
            if (!world.Entities.Has<Transform2>(projectile) || !world.Entities.Has<Collider>(projectile)) continue;

            var pt = world.Entities.Get<Transform2>(projectile);
            var pos = new Vec2(pt.X, pt.Y);
            float pr = world.Entities.Get<Collider>(projectile).Radius;

            _grid.Query(pos, pr + 4f, _candidates);
            for (int i = 0; i < _candidates.Count; i++)
            {
                var candidate = _candidates[i];
                if (candidate == projectile) continue;
                if (!world.Entities.Has<EnemyTag>(candidate)) continue;
                if (!world.Entities.Has<Transform2>(candidate) || !world.Entities.Has<Collider>(candidate)) continue;

                var ct = world.Entities.Get<Transform2>(candidate);
                float cr = world.Entities.Get<Collider>(candidate).Radius;
                if (Vec2.DistanceSquared(pos, new Vec2(ct.X, ct.Y)) > (pr + cr) * (pr + cr)) continue;

                var damageInfo = world.Entities.Has<Damage>(projectile)
                    ? world.Entities.Get<Damage>(projectile)
                    : new Damage(0f);

                var direction = (new Vec2(ct.X, ct.Y) - pos).Normalized;
                var knockback = direction * damageInfo.Knockback;

                world.Hits.Add(new HitEvent(projectile, candidate, damageInfo.Amount, damageInfo.IsCrit, knockback, damageInfo.OnHit));
                if (damageInfo.Pierce) continue;
                CleanupSystem.DestroyEntity(world, projectile);
                break;
            }
        }
    }

    private void ResolveEnemyContact(World world)
    {
        if (!world.HasPlayer || !world.Entities.Has<Transform2>(world.Player) || !world.Entities.Has<Collider>(world.Player))
            return;

        var playerTransform = world.Entities.Get<Transform2>(world.Player);
        var playerPos = new Vec2(playerTransform.X, playerTransform.Y);
        float playerRadius = world.Entities.Get<Collider>(world.Player).Radius;

        _grid.Query(playerPos, playerRadius + 4f, _candidates);
        for (int i = 0; i < _candidates.Count; i++)
        {
            var enemy = _candidates[i];
            if (!world.Entities.Has<EnemyTag>(enemy)) continue;
            if (!world.Entities.Has<Transform2>(enemy) || !world.Entities.Has<Collider>(enemy)) continue;

            if (world.Entities.Has<Cooldown>(enemy))
            {
                var cooldown = world.Entities.Get<Cooldown>(enemy);
                if (cooldown.Remaining > 0f) continue;
            }

            var et = world.Entities.Get<Transform2>(enemy);
            float er = world.Entities.Get<Collider>(enemy).Radius;
            if (Vec2.DistanceSquared(playerPos, new Vec2(et.X, et.Y)) > (playerRadius + er) * (playerRadius + er)) continue;

            var damageInfo = world.Entities.Has<Damage>(enemy)
                ? world.Entities.Get<Damage>(enemy)
                : new Damage(0f);
            world.Hits.Add(new HitEvent(enemy, world.Player, damageInfo.Amount, false, Vec2.Zero, damageInfo.OnHit));

            if (world.Entities.Has<Cooldown>(enemy))
            {
                ref var cooldown = ref world.Entities.GetRef<Cooldown>(enemy);
                cooldown.Remaining = cooldown.Interval;
            }
        }
    }
}

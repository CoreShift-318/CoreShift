using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Waves;

namespace CoreShift.Core.Systems;

public sealed class CleanupSystem : ISystem
{
    private readonly List<Entity> _toDestroy = new();

    public void Update(World world, float dt)
    {
        _toDestroy.Clear();

        foreach (var entity in world.Entities.With<Health>())
        {
            if (world.Entities.Get<Health>(entity).IsDead) _toDestroy.Add(entity);
        }

        foreach (var entity in world.Entities.With<Lifetime>())
        {
            if (world.Entities.Get<Lifetime>(entity).Remaining <= 0f) _toDestroy.Add(entity);
        }

        for (int i = 0; i < _toDestroy.Count; i++)
        {
            DestroyEntity(world, _toDestroy[i]);
        }
    }

    public static void DestroyEntity(World world, Entity entity)
    {
        if (!world.Entities.IsAlive(entity)) return;

        if (world.Entities.Has<EnemyLink>(entity))
        {
            world.EnemyPool.Return(world.Entities.Get<EnemyLink>(entity).Instance);
        }

        if (world.Entities.Has<ProjectileLink>(entity))
        {
            world.ProjectilePool.Return(world.Entities.Get<ProjectileLink>(entity).Instance);
        }

        if (world.Entities.Has<PickupLink>(entity))
        {
            world.PickupPool.Return(world.Entities.Get<PickupLink>(entity).Instance);
        }

        world.Entities.Destroy(entity);
    }
}

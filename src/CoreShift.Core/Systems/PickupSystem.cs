using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Math;

namespace CoreShift.Core.Systems;

public sealed class PickupSystem : ISystem
{
    private const float CollectDistance = 0.7f;
    private const float MagnetSpeed = 14f;

    private readonly List<Entity> _pickups = new();

    public void Update(World world, float dt)
    {
        if (!world.HasPlayer) return;
        if (!world.Entities.Has<Transform2>(world.Player)) return;

        var playerTransform = world.Entities.Get<Transform2>(world.Player);
        var playerPos = new Vec2(playerTransform.X, playerTransform.Y);
        float radius = world.Stats.PickupRadius;

        _pickups.Clear();
        foreach (var entity in world.Entities.With<PickupTag>())
        {
            _pickups.Add(entity);
        }

        for (int i = 0; i < _pickups.Count; i++)
        {
            var entity = _pickups[i];
            if (!world.Entities.IsAlive(entity)) continue;
            if (!world.Entities.Has<Transform2>(entity)) continue;

            ref var transform = ref world.Entities.GetRef<Transform2>(entity);
            var position = new Vec2(transform.X, transform.Y);
            float distance = Vec2.Distance(position, playerPos);

            if (distance <= radius)
            {
                var direction = (playerPos - position);
                if (direction.Length > 1e-4f)
                {
                    direction = direction.Normalized;
                    transform.X += direction.X * MagnetSpeed * dt;
                    transform.Y += direction.Y * MagnetSpeed * dt;
                    position = new Vec2(transform.X, transform.Y);
                    distance = Vec2.Distance(position, playerPos);
                }
            }

            if (distance <= CollectDistance) Collect(world, entity);
        }
    }

    private static void Collect(World world, Entity entity)
    {
        if (!world.Entities.Has<Pickup>(entity))
        {
            CleanupSystem.DestroyEntity(world, entity);
            return;
        }

        var pickup = world.Entities.Get<Pickup>(entity);

        if (pickup.Kind == PickupKind.Health)
        {
            if (world.Entities.Has<Health>(world.Player))
            {
                ref var health = ref world.Entities.GetRef<Health>(world.Player);
                float before = health.Current;
                health.Current = MathF.Min(health.Max, health.Current + pickup.Value);
                float healed = health.Current - before;
                if (healed > 0f)
                {
                    world.HealEvents.Add(new HealEvent(world.Player, healed, PositionOf(world, world.Player)));
                }
            }
        }
        else
        {
            world.RunCredits += (int)pickup.Value;
        }

        CleanupSystem.DestroyEntity(world, entity);
    }

    private static Vec2 PositionOf(World world, Entity entity)
    {
        if (!world.Entities.Has<Transform2>(entity)) return Vec2.Zero;
        var transform = world.Entities.Get<Transform2>(entity);
        return new Vec2(transform.X, transform.Y);
    }
}

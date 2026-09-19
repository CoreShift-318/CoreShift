using CoreShift.Core.Ecs;
using CoreShift.Core.Math;

namespace CoreShift.Core.Systems;

public sealed class MovementSystem : ISystem
{
    public void Update(World world, float dt)
    {
        foreach (var entity in world.Entities.With<Velocity>())
        {
            if (!world.Entities.Has<Transform2>(entity)) continue;

            float scale = 1f;
            if (world.Entities.Has<SlowStatus>(entity))
            {
                scale *= System.Math.Clamp(world.Entities.Get<SlowStatus>(entity).Factor, 0f, 1f);
            }
            if (world.Entities.Has<StunStatus>(entity))
            {
                scale = 0f;
            }

            var velocity = world.Entities.Get<Velocity>(entity);
            ref var transform = ref world.Entities.GetRef<Transform2>(entity);
            transform.X += velocity.X * dt * scale;
            transform.Y += velocity.Y * dt * scale;

            var clamped = world.Arena.Clamp(new Vec2(transform.X, transform.Y));
            transform.X = clamped.X;
            transform.Y = clamped.Y;
        }

        if (world.HasPlayer && world.DashRemaining > 0f && world.Entities.Has<Transform2>(world.Player))
        {
            ref var player = ref world.Entities.GetRef<Transform2>(world.Player);
            player.X += world.DashDirX * world.DashSpeed * dt;
            player.Y += world.DashDirY * world.DashSpeed * dt;

            var clamped = world.Arena.Clamp(new Vec2(player.X, player.Y));
            player.X = clamped.X;
            player.Y = clamped.Y;
        }
    }
}

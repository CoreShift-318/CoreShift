using CoreShift.Core.Ecs;
using CoreShift.Core.Math;

namespace CoreShift.Core.Systems;

public sealed class EnemyAiSystem : ISystem
{
    public void Update(World world, float dt)
    {
        if (!world.HasPlayer) return;

        var playerTransform = world.Entities.Get<Transform2>(world.Player);
        var playerPosition = new Vec2(playerTransform.X, playerTransform.Y);
        float timeScale = world.IsChronoActive ? world.ChronoFactor : 1f;

        foreach (var entity in world.Entities.With<EnemyAi>())
        {
            if (!world.Entities.Has<Transform2>(entity)) continue;

            var transform = world.Entities.Get<Transform2>(entity);
            var ai = world.Entities.Get<EnemyAi>(entity);
            var direction = (playerPosition - new Vec2(transform.X, transform.Y)).Normalized;
            world.Entities.Set(entity, new Velocity(direction.X * ai.Speed * timeScale, direction.Y * ai.Speed * timeScale));

            if (world.Entities.Has<Cooldown>(entity))
            {
                ref var cooldown = ref world.Entities.GetRef<Cooldown>(entity);
                if (cooldown.Remaining > 0f) cooldown.Remaining -= dt;
            }
        }
    }
}

using CoreShift.Core.Ecs;

namespace CoreShift.Core.Systems;

public sealed class ProjectileSystem : ISystem
{
    public void Update(World world, float dt)
    {
        foreach (var entity in world.Entities.With<Lifetime>())
        {
            ref var lifetime = ref world.Entities.GetRef<Lifetime>(entity);
            lifetime.Remaining -= dt;
        }
    }
}

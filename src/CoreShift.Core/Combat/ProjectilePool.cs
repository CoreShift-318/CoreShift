using CoreShift.Core.Ecs;
using CoreShift.Core.Pools;

namespace CoreShift.Core.Combat;

public sealed class ProjectileInstance
{
    public Entity Entity = Entity.Null;
}

public struct ProjectileLink
{
    public ProjectileInstance Instance;
}

public sealed class ProjectilePool
{
    private readonly ObjectPool<ProjectileInstance> _pool;

    public ProjectilePool(int capacity = 4096)
    {
        _pool = new ObjectPool<ProjectileInstance>(capacity, () => new ProjectileInstance());
    }

    public int Created => _pool.Created;
    public int Reused => _pool.Reused;
    public int Active => _pool.Active;
    public int Dropped => _pool.Dropped;

    public bool TryRent(out ProjectileInstance instance) => _pool.TryRent(out instance);

    public void Return(ProjectileInstance instance)
    {
        instance.Entity = Entity.Null;
        _pool.Return(instance);
    }
}

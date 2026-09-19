using CoreShift.Core.Pools;

namespace CoreShift.Core.Waves;

public sealed class EnemyPool
{
    private readonly ObjectPool<EnemyInstance> _pool;

    public EnemyPool(int capacity = 8192)
    {
        _pool = new ObjectPool<EnemyInstance>(capacity, () => new EnemyInstance());
    }

    public int Created => _pool.Created;
    public int Reused => _pool.Reused;
    public int Active => _pool.Active;
    public int Available => _pool.Available;
    public int Capacity => _pool.Capacity;
    public int Dropped => _pool.Dropped;

    public bool TryRent(out EnemyInstance instance) => _pool.TryRent(out instance);

    public void Return(EnemyInstance instance)
    {
        instance.Entity = Ecs.Entity.Null;
        _pool.Return(instance);
    }
}

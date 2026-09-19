using CoreShift.Core.Ecs;
using CoreShift.Core.Pools;

namespace CoreShift.Core.Combat;

public enum PickupKind
{
    Health,
    Credits,
}

public struct Pickup
{
    public PickupKind Kind;
    public float Value;

    public Pickup(PickupKind kind, float value)
    {
        Kind = kind;
        Value = value;
    }
}

public sealed class PickupInstance
{
    public Entity Entity = Entity.Null;
}

public struct PickupLink
{
    public PickupInstance Instance;
}

public sealed class PickupPool
{
    private readonly ObjectPool<PickupInstance> _pool;

    public PickupPool(int capacity = 2048)
    {
        _pool = new ObjectPool<PickupInstance>(capacity, () => new PickupInstance());
    }

    public int Created => _pool.Created;
    public int Reused => _pool.Reused;
    public int Dropped => _pool.Dropped;

    public bool TryRent(out PickupInstance instance) => _pool.TryRent(out instance);

    public void Return(PickupInstance instance)
    {
        instance.Entity = Entity.Null;
        _pool.Return(instance);
    }
}

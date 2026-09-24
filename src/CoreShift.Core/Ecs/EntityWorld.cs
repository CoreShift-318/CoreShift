namespace CoreShift.Core.Ecs;

public sealed class EntityWorld
{
    private uint[] _generations;
    private bool[] _alive;
    private int _capacity;
    private int _nextId;
    private int _entityCount;
    private readonly Stack<int> _free = new();
    private readonly Dictionary<Type, IComponentStore> _stores = new();

    public EntityWorld(int initialCapacity = 1024)
    {
        if (initialCapacity < 1) initialCapacity = 1;
        _capacity = initialCapacity;
        _generations = new uint[_capacity];
        _alive = new bool[_capacity];
    }

    public int EntityCount => _entityCount;

    public Entity Create()
    {
        int id;
        if (_free.Count > 0)
        {
            id = _free.Pop();
        }
        else
        {
            id = _nextId++;
            EnsureCapacity(id);
        }

        unchecked { _generations[id]++; }
        if (_generations[id] == 0u) _generations[id] = 1u;
        _alive[id] = true;
        _entityCount++;
        return new Entity(id, _generations[id]);
    }

    public void Destroy(Entity entity)
    {
        if (!IsAlive(entity)) return;

        _alive[entity.Id] = false;
        _entityCount--;
        foreach (var store in _stores.Values)
        {
            store.RemoveIfPresent(entity.Id);
        }
        _free.Push(entity.Id);
    }

    public bool IsAlive(Entity entity)
    {
        int id = entity.Id;
        return id >= 0 && id < _capacity && _alive[id] && _generations[id] == entity.Generation;
    }

    public bool Has<T>(Entity entity) where T : struct
    {
        if (!_stores.TryGetValue(typeof(T), out var store)) return false;
        return ((ComponentStore<T>)store).Has(entity.Id);
    }

    public T Get<T>(Entity entity) where T : struct => GetStore<T>().Get(entity.Id);

    public ref T GetRef<T>(Entity entity) where T : struct => ref GetStore<T>().Get(entity.Id);

    public void Set<T>(Entity entity, T value) where T : struct => GetStore<T>().Set(entity.Id, value);

    public bool Remove<T>(Entity entity) where T : struct
    {
        if (!_stores.TryGetValue(typeof(T), out var store)) return false;
        return ((ComponentStore<T>)store).Remove(entity.Id);
    }

    /// <summary>Allocation-free iteration over entities that have component <typeparamref name="T"/>.</summary>
    public EntityEnumerable<T> With<T>() where T : struct => new(this, GetStore<T>());

    internal uint GenerationOf(int id) => _generations[id];

    public void Clear()
    {
        foreach (var store in _stores.Values) store.Clear();
        Array.Clear(_generations);
        Array.Clear(_alive);
        _free.Clear();
        _nextId = 0;
        _entityCount = 0;
    }

    private ComponentStore<T> GetStore<T>() where T : struct
    {
        if (!_stores.TryGetValue(typeof(T), out var store))
        {
            store = new ComponentStore<T>(_capacity);
            _stores[typeof(T)] = store;
        }
        return (ComponentStore<T>)store;
    }

    private void EnsureCapacity(int id)
    {
        if (id < _capacity) return;
        int newCapacity = _capacity;
        while (newCapacity <= id) newCapacity *= 2;
        Array.Resize(ref _generations, newCapacity);
        Array.Resize(ref _alive, newCapacity);
        _capacity = newCapacity;
        foreach (var store in _stores.Values) store.EnsureCapacity(newCapacity);
    }
}

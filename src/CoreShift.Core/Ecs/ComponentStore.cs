namespace CoreShift.Core.Ecs;

internal interface IComponentStore
{
    void RemoveIfPresent(int id);
    void Clear();
    void EnsureCapacity(int capacity);
}

/// <summary>
/// Sparse-set storage for one component type, indexed by entity id with an O(1) dense list for
/// allocation-free iteration.
/// </summary>
internal sealed class ComponentStore<T> : IComponentStore where T : struct
{
    private T[] _values;
    private bool[] _present;
    private int[] _entities;
    private int _count;
    private readonly Dictionary<int, int> _index = new();

    public ComponentStore(int capacity)
    {
        _values = new T[capacity];
        _present = new bool[capacity];
        _entities = new int[capacity];
    }

    public int Count => _count;

    public int EntityAt(int i) => _entities[i];

    public bool Has(int id) => id >= 0 && id < _present.Length && _present[id];

    public ref T Get(int id) => ref _values[id];

    public void Set(int id, in T value)
    {
        EnsureCapacityFor(id);
        if (!_present[id])
        {
            _present[id] = true;
            if (_count == _entities.Length) Array.Resize(ref _entities, _entities.Length * 2);
            _index[id] = _count;
            _entities[_count++] = id;
        }
        _values[id] = value;
    }

    public bool Remove(int id)
    {
        if (!Has(id)) return false;

        int idx = _index[id];
        _count--;
        int last = _entities[_count];
        _entities[idx] = last;
        _index[last] = idx;
        _entities[_count] = 0;
        _index.Remove(id);
        _present[id] = false;
        _values[id] = default;
        return true;
    }

    public void RemoveIfPresent(int id) => Remove(id);

    public void EnsureCapacity(int capacity)
    {
        if (capacity <= _values.Length) return;
        Array.Resize(ref _values, capacity);
        Array.Resize(ref _present, capacity);
    }

    private void EnsureCapacityFor(int id)
    {
        if (id < _values.Length) return;
        int newCapacity = _values.Length;
        while (newCapacity <= id) newCapacity *= 2;
        EnsureCapacity(newCapacity);
    }

    public void Clear()
    {
        Array.Clear(_values);
        Array.Clear(_present);
        Array.Clear(_entities);
        _count = 0;
        _index.Clear();
    }
}

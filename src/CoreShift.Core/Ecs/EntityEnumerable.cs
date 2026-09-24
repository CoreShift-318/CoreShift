namespace CoreShift.Core.Ecs;

/// <summary>
/// Allocation-free enumerable over entities that have component <typeparamref name="T"/>.
/// Iterating with <c>foreach</c> does not allocate (struct enumerator), unlike a
/// <c>yield return</c> iterator.
/// </summary>
public readonly struct EntityEnumerable<T> where T : struct
{
    private readonly EntityWorld _world;
    private readonly ComponentStore<T> _store;

    internal EntityEnumerable(EntityWorld world, ComponentStore<T> store)
    {
        _world = world;
        _store = store;
    }

    public Enumerator GetEnumerator() => new(_world, _store);

    public struct Enumerator
    {
        private readonly EntityWorld _world;
        private readonly ComponentStore<T> _store;
        private int _index;

        internal Enumerator(EntityWorld world, ComponentStore<T> store)
        {
            _world = world;
            _store = store;
            _index = -1;
        }

        public bool MoveNext()
        {
            _index++;
            return _index < _store.Count;
        }

        public Entity Current => new(_store.EntityAt(_index), _world.GenerationOf(_store.EntityAt(_index)));
    }
}

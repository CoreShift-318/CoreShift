namespace CoreShift.Core.Pools;

public sealed class ObjectPool<T> where T : class
{
    private readonly Stack<T> _free;
    private readonly Func<T> _factory;
    private readonly Action<T>? _onRent;
    private readonly Action<T>? _onReturn;

    public ObjectPool(int capacity, Func<T> factory, Action<T>? onRent = null, Action<T>? onReturn = null)
    {
        if (capacity < 1) capacity = 1;
        Capacity = capacity;
        _factory = factory;
        _onRent = onRent;
        _onReturn = onReturn;
        _free = new Stack<T>(capacity);
    }

    public int Capacity { get; }
    public int Created { get; private set; }
    public int Reused { get; private set; }
    public int Active { get; private set; }
    public int Available => _free.Count;
    public int Dropped { get; private set; }

    public bool TryRent(out T item)
    {
        if (_free.Count > 0)
        {
            item = _free.Pop();
            Reused++;
        }
        else if (Created < Capacity)
        {
            item = _factory();
            Created++;
        }
        else
        {
            item = default!;
            Dropped++;
            return false;
        }

        Active++;
        _onRent?.Invoke(item);
        return true;
    }

    public void Return(T item)
    {
        if (item is null) return;
        Active--;
        _onReturn?.Invoke(item);
        _free.Push(item);
    }
}

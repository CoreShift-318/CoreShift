using CoreShift.Core.Pools;

namespace CoreShift.Core.Tests;

public class ObjectPoolTests
{
    private sealed class Box
    {
        public int Value;
    }

    [Fact]
    public void TryRent_CreatesUpToCapacity()
    {
        var pool = new ObjectPool<Box>(2, () => new Box());
        Assert.True(pool.TryRent(out _));
        Assert.True(pool.TryRent(out _));
        Assert.False(pool.TryRent(out _));
        Assert.Equal(2, pool.Created);
        Assert.Equal(1, pool.Dropped);
    }

    [Fact]
    public void ReturnedItem_IsReused()
    {
        var pool = new ObjectPool<Box>(1, () => new Box());
        pool.TryRent(out var box);
        box!.Value = 5;
        pool.Return(box);
        Assert.True(pool.TryRent(out var again));
        Assert.Same(box, again);
        Assert.Equal(5, again!.Value);
        Assert.Equal(1, pool.Created);
        Assert.Equal(1, pool.Reused);
    }

    [Fact]
    public void Active_And_Available_TrackCounts()
    {
        var pool = new ObjectPool<Box>(3, () => new Box());
        pool.TryRent(out var a);
        pool.TryRent(out var b);
        Assert.Equal(2, pool.Active);
        Assert.Equal(0, pool.Available);
        pool.Return(a!);
        Assert.Equal(1, pool.Active);
        Assert.Equal(1, pool.Available);
    }

    [Fact]
    public void Callbacks_AreInvoked()
    {
        int rented = 0;
        int returned = 0;
        var pool = new ObjectPool<Box>(1, () => new Box(), _ => rented++, _ => returned++);
        pool.TryRent(out var box);
        pool.Return(box!);
        Assert.Equal(1, rented);
        Assert.Equal(1, returned);
    }
}

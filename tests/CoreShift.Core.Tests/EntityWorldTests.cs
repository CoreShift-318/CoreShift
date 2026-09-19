using CoreShift.Core.Ecs;
using CoreShift.Core.Math;

namespace CoreShift.Core.Tests;

public class EntityWorldTests
{
    [Fact]
    public void Create_ReturnsAliveEntity()
    {
        var world = new EntityWorld();
        var entity = world.Create();
        Assert.True(world.IsAlive(entity));
        Assert.Equal(1, world.EntityCount);
    }

    [Fact]
    public void Destroy_MakesEntityDead()
    {
        var world = new EntityWorld();
        var entity = world.Create();
        world.Destroy(entity);
        Assert.False(world.IsAlive(entity));
        Assert.Equal(0, world.EntityCount);
    }

    [Fact]
    public void ReusedId_HasNewGeneration()
    {
        var world = new EntityWorld();
        var first = world.Create();
        world.Destroy(first);
        var second = world.Create();
        Assert.Equal(first.Id, second.Id);
        Assert.NotEqual(first.Generation, second.Generation);
        Assert.False(world.IsAlive(first));
        Assert.True(world.IsAlive(second));
    }

    [Fact]
    public void Components_RoundTrip()
    {
        var world = new EntityWorld();
        var entity = world.Create();
        world.Set(entity, new Transform2(3, 4));
        ref var transform = ref world.GetRef<Transform2>(entity);
        transform.X = 10;
        Assert.Equal(10, world.Get<Transform2>(entity).X);
        Assert.True(world.Remove<Transform2>(entity));
        Assert.False(world.Has<Transform2>(entity));
    }

    [Fact]
    public void With_EnumeratesOnlyEntitiesHavingComponent()
    {
        var world = new EntityWorld();
        var tagged = world.Create();
        world.Set(tagged, new PlayerTag());
        world.Create();

        int count = 0;
        foreach (var _ in world.With<PlayerTag>()) count++;
        Assert.Equal(1, count);
    }

    [Fact]
    public void Destroy_RemovesComponents_FromAllStores()
    {
        var world = new EntityWorld();
        var entity = world.Create();
        world.Set(entity, new Health(10f));
        world.Set(entity, new Velocity(1f, 1f));
        world.Destroy(entity);
        Assert.Equal(0, world.EntityCount);
        Assert.False(world.Has<Health>(entity));
        Assert.False(world.Has<Velocity>(entity));
    }
}

using CoreShift.Core.Ecs;
using CoreShift.Core.Math;
using CoreShift.Core.Spatial;

namespace CoreShift.Core.Tests;

public class SpatialHashGridTests
{
    [Fact]
    public void Query_ReturnsOnlyNearbyEntities()
    {
        var grid = new SpatialHashGrid(10f);
        var near = new Entity(1, 1);
        var far = new Entity(2, 1);
        grid.Insert(near, new Vec2(0, 0));
        grid.Insert(far, new Vec2(100, 100));

        var results = new List<Entity>();
        int count = grid.Query(new Vec2(0, 0), 5f, results);

        Assert.Equal(1, count);
        Assert.Contains(near, results);
        Assert.DoesNotContain(far, results);
    }

    [Fact]
    public void Clear_RemovesEverything()
    {
        var grid = new SpatialHashGrid(10f);
        grid.Insert(new Entity(1, 1), new Vec2(1, 1));
        grid.Clear();
        Assert.Equal(0, grid.Query(new Vec2(1, 1), 5f, new List<Entity>()));
    }

    [Fact]
    public void NegativeCoordinates_Work()
    {
        var grid = new SpatialHashGrid(10f);
        var entity = new Entity(1, 1);
        grid.Insert(entity, new Vec2(-25, -25));

        var results = new List<Entity>();
        Assert.Equal(1, grid.Query(new Vec2(-25, -25), 3f, results));
    }

    [Fact]
    public void Query_SpansMultipleCells()
    {
        var grid = new SpatialHashGrid(1f);
        grid.Insert(new Entity(1, 1), new Vec2(0.5f, 0.5f));
        grid.Insert(new Entity(2, 1), new Vec2(1.5f, 1.5f));

        var results = new List<Entity>();
        Assert.Equal(2, grid.Query(new Vec2(1f, 1f), 2f, results));
    }
}

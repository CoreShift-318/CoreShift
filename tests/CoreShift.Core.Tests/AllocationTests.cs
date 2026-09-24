using CoreShift.Core.Systems;

namespace CoreShift.Core.Tests;

public class AllocationTests
{
    [Fact]
    public void MovementTicks_DoNotAllocatePerTick()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        world.AddSystem(new InputSystem(5f));
        world.AddSystem(new MovementSystem());
        world.Input = InputState.Move(1f, 0f);

        for (int i = 0; i < 200; i++) world.Tick(); // warm up JIT and buffers

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) world.Tick();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.True(allocated < 4096, $"expected ~0 allocation over 1000 ticks, saw {allocated} bytes");
    }

    [Fact]
    public void EntityIteration_DoesNotAllocate()
    {
        var world = new CoreShift.Core.Ecs.EntityWorld();
        for (int i = 0; i < 512; i++)
        {
            var entity = world.Create();
            world.Set(entity, new CoreShift.Core.Ecs.Velocity(1f, 0f));
        }

        // warm up
        for (int i = 0; i < 100; i++) Count(world);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) Count(world);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.True(allocated < 1024, $"expected ~0 allocation from With<T>() iteration, saw {allocated} bytes");
    }

    private static int Count(CoreShift.Core.Ecs.EntityWorld world)
    {
        int count = 0;
        foreach (var _ in world.With<CoreShift.Core.Ecs.Velocity>()) count++;
        return count;
    }
}

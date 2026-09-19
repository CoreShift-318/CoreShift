using CoreShift.Core.Ecs;
using CoreShift.Core.Systems;

namespace CoreShift.Core.Tests;

public class MovementSystemTests
{
    [Fact]
    public void InputSystem_SetsPlayerVelocity()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        world.Input = new InputState { MoveX = 1, MoveY = 0 };

        var input = new InputSystem(5f);
        for (int i = 0; i < 120; i++) input.Update(world, 1f / 60f);

        var velocity = world.Entities.Get<Velocity>(world.Player);
        Assert.Equal(5f, velocity.X, 2);
        Assert.Equal(0f, velocity.Y, 2);
    }

    [Fact]
    public void DiagonalInput_IsNotFasterThanCardinal()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        world.Input = new InputState { MoveX = 1, MoveY = 1 };

        var input = new InputSystem(6f);
        for (int i = 0; i < 120; i++) input.Update(world, 1f / 60f);

        var velocity = world.Entities.Get<Velocity>(world.Player);
        float magnitude = MathF.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);
        Assert.Equal(6f, magnitude, 2);
    }

    [Fact]
    public void MovementSystem_IntegratesAndClamps()
    {
        var world = new World(1, new Arena(100, 100));
        var player = world.CreatePlayer();
        world.Entities.Set(player, new Transform2(49, 0));
        world.Entities.Set(player, new Velocity(1000, 0));

        new MovementSystem().Update(world, 1f / 60f);

        Assert.Equal(50, world.Entities.Get<Transform2>(player).X, 3);
    }

    [Fact]
    public void EnemyAi_SteersTowardPlayer()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        world.Entities.Set(world.Player, new Transform2(0, 0));

        var enemy = world.Entities.Create();
        world.Entities.Set(enemy, new Transform2(10, 0));
        world.Entities.Set(enemy, new EnemyTag());
        world.Entities.Set(enemy, new EnemyAi(2f));

        new EnemyAiSystem().Update(world, 1f / 60f);

        var velocity = world.Entities.Get<Velocity>(enemy);
        Assert.True(velocity.X < 0f);
    }
}

using CoreShift.Core.Ecs;
using CoreShift.Core.Math;

namespace CoreShift.Core.Systems;

public sealed class InputSystem : ISystem
{
    private readonly float _overrideSpeed;

    public InputSystem(float overrideSpeed = 0f)
    {
        _overrideSpeed = overrideSpeed;
    }

    public void Update(World world, float dt)
    {
        if (!world.HasPlayer) return;

        float speed = _overrideSpeed > 0f ? _overrideSpeed : world.Stats.MoveSpeed;

        // Normalize so diagonals are not faster than cardinals.
        var input = new Vec2(world.Input.MoveX, world.Input.MoveY);
        if (input.Length > 1f) input = input.Normalized;

        var target = new Velocity(input.X * speed, input.Y * speed);

        var current = world.Entities.Has<Velocity>(world.Player)
            ? world.Entities.Get<Velocity>(world.Player)
            : new Velocity(0f, 0f);

        // Ease toward the target for acceleration/deceleration instead of snapping.
        float blend = world.Stats.Acceleration <= 0f
            ? 1f
            : System.Math.Clamp(world.Stats.Acceleration * dt, 0f, 1f);

        var next = new Velocity(
            current.X + (target.X - current.X) * blend,
            current.Y + (target.Y - current.Y) * blend);

        world.Entities.Set(world.Player, next);
    }
}

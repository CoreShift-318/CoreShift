using CoreShift.Core.Math;

namespace CoreShift.Core.Systems;

public sealed class AbilitySystem : ISystem
{
    private bool _wasDown;

    public void Update(World world, float dt)
    {
        if (world.AbilityCooldownRemaining > 0f)
            world.AbilityCooldownRemaining = MathF.Max(0f, world.AbilityCooldownRemaining - dt);
        if (world.DashRemaining > 0f)
            world.DashRemaining = MathF.Max(0f, world.DashRemaining - dt);
        if (world.ChronoRemaining > 0f)
            world.ChronoRemaining = MathF.Max(0f, world.ChronoRemaining - dt);

        bool down = world.Input.Ability;
        bool pressed = down && !_wasDown;
        _wasDown = down;

        if (!pressed || world.AbilityCooldownRemaining > 0f || !world.HasPlayer) return;

        world.AbilityCooldownRemaining = world.Stats.AbilityCooldown;

        if (world.Stats.Ability == AbilityKind.Dash)
        {
            var direction = new Vec2(world.Input.MoveX, world.Input.MoveY);
            if (direction.Length < 1e-3f) direction = new Vec2(world.Input.AimX, world.Input.AimY);
            if (direction.Length < 1e-3f) direction = new Vec2(1f, 0f);
            direction = direction.Normalized;

            world.DashDirX = direction.X;
            world.DashDirY = direction.Y;
            world.DashSpeed = 34f;
            world.DashRemaining = 0.15f;
        }
        else
        {
            world.ChronoRemaining = 3f;
        }
    }
}

using CoreShift.Core;
using CoreShift.Core.Ecs;
using CoreShift.Core.Math;
using CoreShift.Core.Progression;
using CoreShift.Core.Systems;
using CoreShift.Core.Upgrades;
using CoreShift.Data;
using CoreShift.Data.Content;

namespace CoreShift.App;

public interface IInputSource
{
    InputState Poll(World world);
}

public sealed class ScriptedInputSource : IInputSource
{
    private readonly IReadOnlyList<InputState> _inputs;
    private int _index;

    public ScriptedInputSource(IReadOnlyList<InputState> inputs)
    {
        _inputs = inputs;
    }

    public static ScriptedInputSource Circle(int repeats = 60) => new(BuildCircle(repeats));

    public InputState Poll() => Poll(null!);

    public InputState Poll(World world) => _index < _inputs.Count ? _inputs[_index++] : InputState.None;

    private static List<InputState> BuildCircle(int repeats)
    {
        var inputs = new List<InputState>();
        var directions = new[]
        {
            InputState.Move(1, 0),
            InputState.Move(0, 1),
            InputState.Move(-1, 0),
            InputState.Move(0, -1),
        };
        for (int r = 0; r < repeats; r++)
        {
            foreach (var direction in directions) inputs.Add(direction);
        }
        return inputs;
    }
}

/// <summary>
/// Headless input: orbits slowly, aims at the nearest enemy, and holds fire.
/// Lets scripted/console runs exercise manual combat without a player.
/// </summary>
public sealed class AutoPilotInputSource : IInputSource
{
    private int _tick;

    public InputState Poll(World world)
    {
        _tick++;

        var move = ((_tick / 45) % 4) switch
        {
            0 => new Vec2(1f, 0f),
            1 => new Vec2(0f, 1f),
            2 => new Vec2(-1f, 0f),
            _ => new Vec2(0f, -1f),
        };

        var aim = move;
        if (world.HasPlayer && world.Entities.Has<Transform2>(world.Player))
        {
            var playerTransform = world.Entities.Get<Transform2>(world.Player);
            var playerPos = new Vec2(playerTransform.X, playerTransform.Y);

            Entity nearest = Entity.Null;
            float best = float.MaxValue;
            foreach (var enemy in world.Entities.With<EnemyTag>())
            {
                if (!world.Entities.Has<Transform2>(enemy)) continue;
                var et = world.Entities.Get<Transform2>(enemy);
                float distance = Vec2.Distance(playerPos, new Vec2(et.X, et.Y));
                if (distance < best)
                {
                    best = distance;
                    nearest = enemy;
                }
            }

            if (!nearest.IsNull)
            {
                var et = world.Entities.Get<Transform2>(nearest);
                float distance = Vec2.Distance(playerPos, new Vec2(et.X, et.Y));
                float travel = distance / PlayerWeaponSystem.ProjectileSpeed;
                var predicted = new Vec2(et.X, et.Y);
                if (world.Entities.Has<Velocity>(nearest))
                {
                    var ev = world.Entities.Get<Velocity>(nearest);
                    predicted = new Vec2(et.X + ev.X * travel, et.Y + ev.Y * travel);
                }
                var direction = predicted - playerPos;
                if (direction.Length > 1e-4f) aim = direction.Normalized;
            }
        }

        return new InputState
        {
            MoveX = move.X,
            MoveY = move.Y,
            AimX = aim.X,
            AimY = aim.Y,
            Firing = true,
        };
    }
}

public static class GameRunner
{
    public static GameSnapshot Run(
        IInputSource input,
        TextWriter output,
        int maxTicks,
        uint seed = 12345,
        GameContent? content = null,
        ProgressionState? progression = null,
        int renderEvery = 0)
    {
        content ??= GameContent.Default();
        var world = GameFactory.Build(seed, content, progression);
        var upgradeSystem = world.Systems.OfType<UpgradeSystem>().First();

        int ticks = 0;
        while (ticks < maxTicks && !world.IsGameOver)
        {
            world.Input = input.Poll(world);
            world.Tick();

            if (upgradeSystem.HasOffers) upgradeSystem.Choose(world, 0);

            ticks++;
            if (renderEvery > 0 && ticks % renderEvery == 0)
            {
                output.WriteLine(AsciiRenderer.Render(world));
            }
        }

        return world.Snapshot();
    }
}

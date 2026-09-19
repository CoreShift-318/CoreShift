using System.Numerics;
using CoreShift.Core;
using CoreShift.Core.Ecs;
using CoreShift.Core.Math;
using CoreShift.Core.Waves;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class PlayScene : Scene
{
    private float _accumulator;
    private float _lastHealth = -1f;
    private int _lastProjectiles;

    public void Enter(GameApp app)
    {
        _accumulator = 0f;
        _lastProjectiles = 0;
        _lastHealth = CurrentHealth(app);
    }

    public void Update(GameApp app, float dt)
    {
        var world = app.World!;
        app.UpdateCamera(world, dt);

        if (world.IsGameOver)
        {
            app.ChangeScene(new GameOverScene());
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            app.ChangeScene(new PauseScene(this));
            return;
        }

        world.Input = InputMapper.ReadGameplay(app.Camera, world);

        if (app.HitStop > 0f)
        {
            app.HitStop -= dt;
            app.Particles.Update(dt);
            app.Floating.Update(dt);
            return;
        }

        _accumulator += dt;
        int guard = 0;
        while (_accumulator >= World.FixedDeltaSeconds && guard++ < 8)
        {
            world.Tick();
            _accumulator -= World.FixedDeltaSeconds;
            app.EmitEffectEvents();

            if (app.UpgradeSystem!.HasOffers || world.IsGameOver) break;
        }

        app.Particles.Update(dt);
        app.Floating.Update(dt);
        EmitMuzzleFlash(app);

        float health = CurrentHealth(app);
        if (_lastHealth >= 0f && health < _lastHealth)
        {
            float intensity = 1f + MathF.Min(3f, (_lastHealth - health) / 8f);
            app.AddShake(0.10f * intensity);
        }
        _lastHealth = health;

        if (app.UpgradeSystem!.HasOffers)
        {
            app.ChangeScene(new LevelUpScene(this));
            return;
        }

        if (world.IsGameOver)
        {
            app.ChangeScene(new GameOverScene());
        }
    }

    public void Draw(GameApp app)
    {
        var world = app.World!;
        app.Renderer.DrawBackground(app.Time);
        app.Renderer.DrawWorld(world, app.Time);
        app.Particles.Draw(app.Camera);
        app.Floating.Draw(app.Camera);
        app.Hud.Draw(world, app.Session.Progression, app.Time);
        DrawOffscreenIndicators(app);
        DrawLowHealthVignette(world);
        DrawMouseReticle(app);
    }

    private static void DrawOffscreenIndicators(GameApp app)
    {
        var world = app.World!;
        float width = Raylib.GetScreenWidth();
        float height = Raylib.GetScreenHeight();
        const float margin = 28f;
        var center = new Vector2(width * 0.5f, height * 0.5f);

        foreach (var entity in world.Entities.With<EnemyTag>())
        {
            if (!world.Entities.Has<Transform2>(entity)) continue;

            var transform = world.Entities.Get<Transform2>(entity);
            var screen = app.Camera.ToScreen(new Vec2(transform.X, transform.Y));
            if (screen.X > margin && screen.X < width - margin && screen.Y > margin && screen.Y < height - margin)
            {
                continue;
            }

            var direction = screen - center;
            if (direction.Length() < 1e-3f) continue;
            direction = Vector2.Normalize(direction);

            float tx = MathF.Abs(direction.X) > 1e-4f ? (width * 0.5f - margin) / MathF.Abs(direction.X) : float.MaxValue;
            float ty = MathF.Abs(direction.Y) > 1e-4f ? (height * 0.5f - margin) / MathF.Abs(direction.Y) : float.MaxValue;
            var position = center + direction * MathF.Min(tx, ty);

            var perpendicular = new Vector2(-direction.Y, direction.X);
            _ = perpendicular;

            Color color = Palette.Danger;
            if (world.Entities.Has<EnemyLink>(entity))
            {
                color = Palette.FromArgb(world.Entities.Get<EnemyLink>(entity).Instance.Spec.Color);
            }
            var marker = Palette.Alpha(color, 225);
            Raylib.DrawCircleV(position, 7f, marker);
            Raylib.DrawLineEx(position, position + direction * 13f, 4f, marker);
        }
    }

    private static void DrawLowHealthVignette(World world)
    {
        float fraction = world.Snapshot().HealthFraction;
        if (fraction >= 0.35f) return;

        int width = Raylib.GetScreenWidth();
        int height = Raylib.GetScreenHeight();
        byte strength = (byte)(150 * (1f - fraction / 0.35f));

        for (int inset = 0; inset < 56; inset += 4)
        {
            byte alpha = (byte)(strength * (1f - inset / 56f) * 0.22f);
            Raylib.DrawRectangleLines(inset, inset, width - inset * 2, height - inset * 2, Palette.Alpha(Palette.Danger, alpha));
        }
    }

    private static void DrawMouseReticle(GameApp app)
    {
        if (!InputMapper.MouseAiming) return;
        var mouse = Raylib.GetMousePosition();
        Raylib.BeginBlendMode(BlendMode.Additive);
        Raylib.DrawCircleLines((int)mouse.X, (int)mouse.Y, 10f, Palette.Alpha(Palette.Player, 200));
        Raylib.DrawCircleLines((int)mouse.X, (int)mouse.Y, 4f, Palette.Alpha(Palette.Player, 120));
        Raylib.EndBlendMode();
    }

    private void EmitMuzzleFlash(GameApp app)
    {
        var world = app.World!;
        int projectiles = 0;
        foreach (var _ in world.Entities.With<ProjectileTag>()) projectiles++;

        if (projectiles > _lastProjectiles)
        {
            app.Particles.EmitBurst(PlayerPosition(app), Palette.Projectile, 3, 7f);
            Audio.Play(Sfx.Shoot);
        }
        _lastProjectiles = projectiles;
    }

    private static float CurrentHealth(GameApp app)
    {
        var world = app.World;
        if (world is null || !world.HasPlayer || !world.Entities.Has<Health>(world.Player)) return -1f;
        return world.Entities.Get<Health>(world.Player).Current;
    }

    private static Vec2 PlayerPosition(GameApp app)
    {
        var world = app.World!;
        if (!world.HasPlayer || !world.Entities.Has<Transform2>(world.Player)) return Vec2.Zero;
        var transform = world.Entities.Get<Transform2>(world.Player);
        return new Vec2(transform.X, transform.Y);
    }
}

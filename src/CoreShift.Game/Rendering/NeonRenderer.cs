using System.Numerics;
using CoreShift.Core;
using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Waves;
using CoreShift.Core.Math;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class NeonRenderer
{
    private readonly SpriteRenderer _sprites;
    private readonly (float X, float Y, float Depth)[] _stars;

    public NeonRenderer(GameCamera camera, SpriteRenderer sprites)
    {
        Camera = camera;
        _sprites = sprites;

        var rng = new CoreShift.Core.Rng.DeterministicRng(0x5EEDu);
        _stars = new (float, float, float)[160];
        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i] = (rng.NextFloat(), rng.NextFloat(), rng.Range(0.2f, 0.7f));
        }
    }

    public GameCamera Camera { get; }

    public void DrawBackground(float time)
    {
        int width = (int)Camera.ScreenWidth;
        int height = (int)Camera.ScreenHeight;

        for (int y = 0; y < height; y++)
        {
            float t = y / (float)height;
            Raylib.DrawRectangle(0, y, width, 1, Palette.Lerp(Palette.BackgroundTop, Palette.Background, t));
        }

        DrawStars();
        DrawGrid(time);

        Raylib.BeginBlendMode(BlendMode.Additive);
        var center = new Vector2(Camera.ScreenWidth * 0.5f + Camera.Offset.X, Camera.ScreenHeight * 0.5f + Camera.Offset.Y);
        Raylib.DrawCircleV(center, Camera.ScreenWidth * 0.45f, Palette.Alpha(new Color(80, 30, 140, 255), 26));
        Raylib.EndBlendMode();
    }

    private void DrawStars()
    {
        float width = Camera.ScreenWidth;
        float height = Camera.ScreenHeight;

        Raylib.BeginBlendMode(BlendMode.Additive);
        for (int i = 0; i < _stars.Length; i++)
        {
            var star = _stars[i];
            float px = (star.X * width - Camera.Center.X * Camera.PixelsPerUnit * star.Depth) % width;
            if (px < 0f) px += width;
            float py = (star.Y * height + Camera.Center.Y * Camera.PixelsPerUnit * star.Depth) % height;
            if (py < 0f) py += height;

            byte alpha = (byte)(50 + 150 * star.Depth);
            Raylib.DrawCircleV(new Vector2(px + Camera.Offset.X, py + Camera.Offset.Y), 1f + star.Depth * 1.6f, Palette.Alpha(Palette.Text, alpha));
        }
        Raylib.EndBlendMode();
    }

    private void DrawGrid(float time)
    {
        const float cell = 3f;
        float scroll = (time * 0.6f) % cell;
        float halfWidth = Camera.ScreenWidth * 0.5f / Camera.PixelsPerUnit;
        float halfHeight = Camera.ScreenHeight * 0.5f / Camera.PixelsPerUnit;
        float centerX = Camera.Center.X;
        float centerY = Camera.Center.Y;

        for (float x = centerX - halfWidth - cell; x <= centerX + halfWidth + cell; x += cell)
        {
            var a = Camera.ToScreen(new Vec2(x, centerY - halfHeight - cell + scroll));
            var b = Camera.ToScreen(new Vec2(x, centerY + halfHeight + cell + scroll));
            Raylib.DrawLineEx(a, b, 1f, Palette.Alpha(Palette.Grid, 55));
        }

        for (float y = centerY - halfHeight - cell; y <= centerY + halfHeight + cell; y += cell)
        {
            var a = Camera.ToScreen(new Vec2(centerX - halfWidth - cell, y + scroll));
            var b = Camera.ToScreen(new Vec2(centerX + halfWidth + cell, y + scroll));
            Raylib.DrawLineEx(a, b, 1f, Palette.Alpha(Palette.Grid, 40));
        }
    }

    public void DrawWorld(World world, float time)
    {
        DrawProjectiles(world);
        DrawPickups(world, time);
        DrawStatuses(world, time);
        DrawEnemies(world, time);
        DrawPlayer(world, time);
    }

    private void DrawPickups(World world, float time)
    {
        float pulse = 1f + 0.18f * MathF.Sin(time * 6f);

        foreach (var entity in world.Entities.With<PickupTag>())
        {
            if (!world.Entities.Has<Transform2>(entity) || !world.Entities.Has<Pickup>(entity)) continue;

            var transform = world.Entities.Get<Transform2>(entity);
            var position = Camera.ToScreen(new Vec2(transform.X, transform.Y));
            var pickup = world.Entities.Get<Pickup>(entity);

            if (pickup.Kind == PickupKind.Health)
            {
                GlowCircle(position, 7f * pulse, Palette.Health);
                Raylib.DrawRectangle((int)position.X - 5, (int)position.Y - 1, 10, 2, Palette.PlayerCore);
                Raylib.DrawRectangle((int)position.X - 1, (int)position.Y - 5, 2, 10, Palette.PlayerCore);
            }
            else
            {
                GlowCircle(position, 6f * pulse, Palette.Gold);
            }
        }
    }

    private void DrawStatuses(World world, float time)
    {
        foreach (var entity in world.Entities.With<Transform2>())
        {
            if (!world.Entities.Has<Collider>(entity) && entity != world.Player) continue;

            float radius = world.Entities.Has<Collider>(entity)
                ? world.Entities.Get<Collider>(entity).Radius * Camera.PixelsPerUnit
                : 12f;
            var transform = world.Entities.Get<Transform2>(entity);
            var position = Camera.ToScreen(new Vec2(transform.X, transform.Y));

            if (world.Entities.Has<BurnStatus>(entity))
            {
                float flicker = 1f + 0.3f * MathF.Sin(time * 26f);
                Raylib.BeginBlendMode(BlendMode.Additive);
                Raylib.DrawCircleV(position, radius * 1.8f * flicker, Palette.Alpha(new Color(255, 130, 40, 255), 55));
                Raylib.EndBlendMode();
            }

            if (world.Entities.Has<PoisonStatus>(entity))
            {
                Raylib.BeginBlendMode(BlendMode.Additive);
                Raylib.DrawCircleV(position, radius * 1.6f, Palette.Alpha(new Color(120, 240, 90, 255), 50));
                Raylib.EndBlendMode();
            }

            if (world.Entities.Has<SlowStatus>(entity))
            {
                Raylib.DrawRing(position, radius * 1.9f, radius * 2.2f, 0f, 360f, 32, Palette.Alpha(new Color(120, 200, 255, 255), 170));
            }

            if (world.Entities.Has<StunStatus>(entity))
            {
                for (int k = 0; k < 3; k++)
                {
                    float angle = time * 4f + k * (MathF.Tau / 3f);
                    var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (radius + 6f);
                    Raylib.DrawCircleV(position + offset, 2.5f, Palette.Gold);
                }
            }

            if (world.Entities.Has<ShockStatus>(entity))
            {
                Raylib.BeginBlendMode(BlendMode.Additive);
                for (int k = 0; k < 4; k++)
                {
                    float angle = time * 9f + k * 1.7f;
                    var inner = position;
                    var outer = position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (radius * 2.6f);
                    Raylib.DrawLineEx(inner, outer, 1.5f, Palette.Alpha(new Color(120, 240, 255, 255), 160));
                }
                Raylib.EndBlendMode();
            }
        }
    }

    private void DrawProjectiles(World world)
    {
        foreach (var entity in world.Entities.With<ProjectileTag>())
        {
            if (!world.Entities.Has<Transform2>(entity)) continue;
            var transform = world.Entities.Get<Transform2>(entity);
            var position = Camera.ToScreen(new Vec2(transform.X, transform.Y));

            Raylib.BeginBlendMode(BlendMode.Additive);
            Raylib.DrawCircleV(position, 9f, Palette.Alpha(Palette.Projectile, 60));
            Raylib.DrawCircleV(position, 5f, Palette.Alpha(Palette.Projectile, 130));
            Raylib.EndBlendMode();
            Raylib.DrawCircleV(position, 2.5f, Palette.PlayerCore);
        }
    }

    private void DrawEnemies(World world, float time)
    {
        foreach (var entity in world.Entities.With<EnemyTag>())
        {
            if (!world.Entities.Has<Transform2>(entity) || !world.Entities.Has<Collider>(entity)) continue;

            var transform = world.Entities.Get<Transform2>(entity);
            float radius = world.Entities.Get<Collider>(entity).Radius;
            var position = Camera.ToScreen(new Vec2(transform.X, transform.Y));
            float pixelRadius = radius * Camera.PixelsPerUnit;

            Color color = Palette.Danger;
            string id = "grunt";
            if (world.Entities.Has<EnemyLink>(entity))
            {
                var spec = world.Entities.Get<EnemyLink>(entity).Instance.Spec;
                color = Palette.FromArgb(spec.Color);
                id = spec.Id;
            }

            Raylib.BeginBlendMode(BlendMode.Additive);
            Raylib.DrawCircleV(position, pixelRadius * 1.3f, Palette.Alpha(color, 34));
            Raylib.DrawCircleV(position, pixelRadius * 0.8f, Palette.Alpha(color, 70));
            Raylib.EndBlendMode();

            bool moving = false;
            float angleDeg = 0f;
            if (world.Entities.Has<Velocity>(entity))
            {
                var velocity = world.Entities.Get<Velocity>(entity);
                if (velocity.X * velocity.X + velocity.Y * velocity.Y > 0.04f)
                {
                    moving = true;
                    angleDeg = MathF.Atan2(-velocity.Y, velocity.X) * (180f / MathF.PI);
                }
            }
            if (!moving && world.HasPlayer && world.Entities.Has<Transform2>(world.Player))
            {
                var playerTransform = world.Entities.Get<Transform2>(world.Player);
                float dx = playerTransform.X - transform.X;
                float dy = playerTransform.Y - transform.Y;
                if (dx * dx + dy * dy > 1e-6f) angleDeg = MathF.Atan2(-dy, dx) * (180f / MathF.PI);
            }

            FigureStyle style = id switch
            {
                "brute" => FigureStyle.Brute,
                "wisp" => FigureStyle.Wisp,
                _ => FigureStyle.Grunt,
            };
            float figureScale = id switch
            {
                "brute" => pixelRadius * 1.6f,
                "wisp" => pixelRadius * 2.4f,
                _ => pixelRadius * 1.9f,
            };
            float phase = time * 10f + entity.Id * 1.7f;

            string? sprite = id switch
            {
                "brute" => "robot1_stand",
                "wisp" => "robot1_gun",
                _ => "zoimbie1_hold",
            };
            if (sprite is not null && _sprites.Has(sprite))
            {
                Color tint = id == "grunt" ? Color.White : FigureRenderer.Brighten(color, 0.55f);
                float height = id switch
                {
                    "brute" => pixelRadius * 3.8f,
                    "wisp" => pixelRadius * 4.5f,
                    _ => pixelRadius * 3.4f,
                };
                _sprites.Draw(sprite, position, angleDeg, height, tint, moving, phase);
            }
            else
            {
                FigureRenderer.Draw(position, angleDeg, phase, color, style, moving, figureScale);
            }

            if (world.Entities.Has<Health>(entity))
            {
                var health = world.Entities.Get<Health>(entity);
                if (health.Current < health.Max)
                {
                    float fraction = Math.Clamp(health.Current / health.Max, 0f, 1f);
                    float barWidth = figureScale * 1.6f;
                    float barX = position.X - barWidth * 0.5f;
                    float barY = position.Y - figureScale * 1.7f - 9f;
                    Raylib.DrawRectangleRec(new Rectangle(barX, barY, barWidth, 3.5f), Palette.Alpha(Palette.Background, 200));
                    Raylib.DrawRectangleRec(new Rectangle(barX, barY, barWidth * fraction, 3.5f), Palette.Danger);
                }
            }
        }
    }

    private void DrawPlayer(World world, float time)
    {
        if (!world.HasPlayer) return;
        if (!world.Entities.Has<Transform2>(world.Player)) return;

        var transform = world.Entities.Get<Transform2>(world.Player);
        var position = Camera.ToScreen(new Vec2(transform.X, transform.Y));
        float pixelRadius = world.Entities.Has<Collider>(world.Player)
            ? world.Entities.Get<Collider>(world.Player).Radius * Camera.PixelsPerUnit
            : 10f;

        Raylib.BeginBlendMode(BlendMode.Additive);
        Raylib.DrawCircleV(position, pixelRadius * 1.4f, Palette.Alpha(Palette.Player, 34));
        Raylib.DrawCircleV(position, pixelRadius * 0.9f, Palette.Alpha(Palette.Player, 70));
        Raylib.EndBlendMode();

        float angleDeg = 0f;
        if (world.Entities.Has<AimDirection>(world.Player))
        {
            var aim = world.Entities.Get<AimDirection>(world.Player);
            if (aim.X * aim.X + aim.Y * aim.Y > 1e-6f)
            {
                angleDeg = MathF.Atan2(-aim.Y, aim.X) * (180f / MathF.PI);
            }
        }

        bool moving = false;
        if (world.Entities.Has<Velocity>(world.Player))
        {
            var velocity = world.Entities.Get<Velocity>(world.Player);
            moving = velocity.X * velocity.X + velocity.Y * velocity.Y > 0.04f;
        }

        if (_sprites.Has("survivor1_gun"))
        {
            _sprites.Draw("survivor1_gun", position, angleDeg, pixelRadius * 3.4f, Color.White, moving, time * 12f);
        }
        else
        {
            FigureRenderer.Draw(position, angleDeg, time * 12f, Palette.Player, FigureStyle.Player, moving, pixelRadius * 1.9f);
        }

        if (world.Entities.Has<Health>(world.Player))
        {
            var health = world.Entities.Get<Health>(world.Player);
            float fraction = Math.Clamp(health.Current / health.Max, 0f, 1f);
            Raylib.DrawRing(position, pixelRadius * 2.3f, pixelRadius * 2.55f, -90f, -90f + 360f * fraction, 48, Palette.Health);
        }

        DrawAimIndicator(world, position, pixelRadius);
    }

    private void DrawAimIndicator(World world, Vector2 position, float pixelRadius)
    {
        var aim = new Vector2(1f, 0f);
        if (world.Entities.Has<AimDirection>(world.Player))
        {
            var stored = world.Entities.Get<AimDirection>(world.Player);
            var value = new Vector2(stored.X, stored.Y);
            if (value.Length() > 1e-4f) aim = Vector2.Normalize(value);
        }

        // World +Y is up, screen +Y is down; flip for drawing.
        var screenAim = new Vector2(aim.X, -aim.Y);
        var tip = position + screenAim * (3.4f * Camera.PixelsPerUnit);
        Raylib.BeginBlendMode(BlendMode.Additive);
        Raylib.DrawLineEx(position, tip, 2f, Palette.Alpha(Palette.Player, 70));
        Raylib.DrawCircleV(tip, 7f, Palette.Alpha(Palette.Player, 60));
        Raylib.EndBlendMode();
        Raylib.DrawCircleLines((int)tip.X, (int)tip.Y, 6f, Palette.Alpha(Palette.Player, 220));
    }

    public static void GlowCircle(Vector2 position, float radius, Color color)
    {
        Raylib.BeginBlendMode(BlendMode.Additive);
        Raylib.DrawCircleV(position, radius * 2.4f, Palette.Alpha(color, 40));
        Raylib.DrawCircleV(position, radius * 1.5f, Palette.Alpha(color, 90));
        Raylib.EndBlendMode();
        Raylib.DrawCircleV(position, radius, color);
    }
}

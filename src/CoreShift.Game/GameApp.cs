using System.Numerics;
using CoreShift.Core;
using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Math;
using CoreShift.Core.Progression;
using CoreShift.Core.Systems;
using CoreShift.Core.Upgrades;
using CoreShift.Core.Waves;
using CoreShift.Data;
using CoreShift.Data.Content;
using Raylib_cs;

namespace CoreShift.Game;

public sealed class GameApp
{
    public const int ScreenWidth = 1280;
    public const int ScreenHeight = 720;
    public const string Version = "0.1.0";
    private const float PixelsPerUnit = 30f;

    private readonly Random _shakeRandom = new();
    private int _screenshotIndex;
    private int _lastLevel = 1;
    private int _lastCredits;

    public GameApp(GameContent content)
    {
        Content = content;
        Session = new GameSession();
        Camera = new GameCamera(ScreenWidth, ScreenHeight, PixelsPerUnit);
    }

    public GameContent Content { get; }
    public GameSession Session { get; }
    public GameCamera Camera { get; }
    public SpriteRenderer Sprites { get; private set; } = null!;
    public NeonRenderer Renderer { get; private set; } = null!;

    private void InitializeGraphics()
    {
        // Must run after InitWindow: GL textures cannot be created without a context.
        Sprites = new SpriteRenderer();
        Renderer = new NeonRenderer(Camera, Sprites);
    }
    public Hud Hud { get; } = new();
    public ParticleSystem Particles { get; } = new();
    public FloatingTextSystem Floating { get; } = new();

    public World? World { get; private set; }
    public UpgradeSystem? UpgradeSystem { get; private set; }
    public Scene Current { get; private set; } = new MainMenuScene();

    public uint Seed { get; set; } = 12345;
    public bool AutoChooseUpgrades { get; set; }
    public bool QuitRequested { get; private set; }
    public float Time { get; private set; }
    public float Shake { get; private set; }
    public float HitStop { get; set; }
    public int TotalDamageEvents { get; private set; }
    private float _fade = 1f;

    public void Run()
    {
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "CoreShift: Chrono-Survival v" + Version);
        Raylib.SetExitKey(KeyboardKey.Null);
        Raylib.SetTargetFPS(60);
        Audio.Initialize();
        ApplyAudioSettings();
        Audio.StartMusic();
        InitializeGraphics();
        ChangeScene(new MainMenuScene());

        while (!Raylib.WindowShouldClose() && !QuitRequested)
        {
            float dt = Raylib.GetFrameTime();
            Time += dt;
            UpdateShake(dt);
            Audio.UpdateMusic();

            Current.Update(this, dt);

            if (Raylib.IsKeyPressed(KeyboardKey.F12)) SaveScreenshot(NextScreenshotPath());

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Palette.Background);
            Current.Draw(this);
            if (_fade > 0f)
            {
                _fade = MathF.Max(0f, _fade - dt * 2.5f);
                Raylib.DrawRectangle(0, 0, ScreenWidth, ScreenHeight, Palette.Alpha(Palette.Background, (byte)(255 * _fade)));
            }
            if (Session.Progression.Data.ShowFps)
            {
                Raylib.DrawText($"FPS {Raylib.GetFPS()}", ScreenWidth - 96, ScreenHeight - 24, 16, Palette.TextDim);
            }
            Raylib.EndDrawing();
        }

        Sprites.Dispose();
        Audio.Shutdown();
        Raylib.CloseWindow();
    }

    public void ApplyAudioSettings()
    {
        var data = Session.Progression.Data;
        Audio.SetVolumes(data.MasterVolume, data.SfxVolume);
        Audio.SetMusicVolume(data.MusicVolume * data.MasterVolume);
    }

    public void RunScreenshot(string outputPath, string sceneName, int ticks)
    {
        Raylib.SetConfigFlags(ConfigFlags.HiddenWindow);
        Raylib.InitWindow(ScreenWidth, ScreenHeight, "CoreShift");
        Raylib.SetExitKey(KeyboardKey.Null);
        Raylib.SetTargetFPS(60);
        InitializeGraphics();

        switch (sceneName)
        {
            case "menu":
                ChangeScene(new MainMenuScene());
                Time = 1.3f;
                break;
            case "hub":
                Session.Progression.Data.Currency = 420;
                ChangeScene(new HubScene());
                Time = 1.3f;
                break;
            case "settings":
                ChangeScene(new SettingsScene());
                Time = 1.3f;
                break;
            case "characters":
                ChangeScene(new CharacterSelectScene());
                Time = 1.3f;
                break;
            case "levelup":
                StartRun();
                AdvanceTicks(ticks);
                World!.PendingLevelUps = 1;
                UpgradeSystem!.Offer(World);
                ChangeScene(new LevelUpScene(new PlayScene()));
                break;
            case "status":
                StartRun();
                World!.Stats.WeaponStatus = StatusKind.Burn;
                World.Stats.WeaponStatusMagnitude = 6f;
                World.Stats.WeaponStatusDuration = 4f;
                World.Stats.CritChance = 0.4f;
                AdvanceTicks(ticks);
                break;
            case "aimtest":
                StartRun();
                for (int i = 0; i < 5 && World is not null; i++)
                {
                    World.Input = new InputState { AimX = 0f, AimY = 1f, Firing = false };
                    World.Tick();
                }
                break;
            case "enemies":
                StartRun();
                {
                    var player = World!.Player;
                    var pt = World.Entities.Get<Transform2>(player);
                    var grunt = Content.FindEnemySpec("grunt") ?? new EnemySpec { Id = "grunt" };
                    var brute = Content.FindEnemySpec("brute") ?? new EnemySpec { Id = "brute" };
                    var wisp = Content.FindEnemySpec("wisp") ?? new EnemySpec { Id = "wisp" };
                    EnemyFactory.Spawn(World, grunt, new Vec2(pt.X + 4f, pt.Y + 1f), 1);
                    EnemyFactory.Spawn(World, brute, new Vec2(pt.X + 3f, pt.Y - 4f), 1);
                    EnemyFactory.Spawn(World, wisp, new Vec2(pt.X - 4f, pt.Y + 2f), 1);
                    for (int i = 0; i < 3; i++)
                    {
                        World.Input = InputState.None;
                        World.Tick();
                    }
                }
                break;
            case "gameover":
                StartRun();
                AdvanceTicks(ticks);
                World!.IsGameOver = true;
                ChangeScene(new GameOverScene());
                break;
            default:
                StartRun();
                AdvanceTicks(ticks);
                break;
        }

        for (int i = 0; i < 4; i++)
        {
            Time += 1f / 60f;
            UpdateShake(1f / 60f);
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Palette.Background);
            Current.Draw(this);
            Raylib.EndDrawing();
        }

        SaveScreenshot(outputPath);
        Sprites.Dispose();
        Raylib.CloseWindow();
        System.Console.WriteLine($"  damage events: {TotalDamageEvents}, floating texts: {Floating.Count}");
        if (World is not null)
        {
            int enemies = 0;
            int offscreen = 0;
            foreach (var entity in World.Entities.With<EnemyTag>())
            {
                if (!World.Entities.Has<Transform2>(entity)) continue;
                enemies++;
                var t = World.Entities.Get<Transform2>(entity);
                var s = Camera.ToScreen(new Vec2(t.X, t.Y));
                if (s.X < 28f || s.X > ScreenWidth - 28f || s.Y < 28f || s.Y > ScreenHeight - 28f) offscreen++;
            }
            System.Console.WriteLine($"  enemies: {enemies}, offscreen: {offscreen}, camera: ({Camera.Center.X:0.#},{Camera.Center.Y:0.#})");
        }
    }

    public void ChangeScene(Scene scene)
    {
        Current = scene;
        _fade = 1f;
        if (scene is PlayScene) Raylib.HideCursor();
        else Raylib.ShowCursor();
        scene.Enter(this);
    }

    public void StartRun()
    {
        Session.Progression.BeginRun();
        World = GameFactory.Build(Seed, Content, Session.Progression);
        World.Stats.AimAssist = Session.Progression.Data.AimAssistEnabled ? 0.35f : 0f;
        UpgradeSystem = null;
        foreach (var system in World.Systems)
        {
            if (system is UpgradeSystem upgrades) UpgradeSystem = upgrades;
        }
        Floating.Clear();
        HitStop = 0f;
        _lastLevel = 1;
        _lastCredits = 0;
        Camera.Center = CoreShift.Core.Math.Vec2.Zero;
        ChangeScene(new PlayScene());
    }

    public void RequestQuit() => QuitRequested = true;

    public void UpdateCamera(World world, float dt)
    {
        if (!world.HasPlayer || !world.Entities.Has<Transform2>(world.Player)) return;

        var transform = world.Entities.Get<Transform2>(world.Player);
        var target = Camera.ClampToArena(new Vec2(transform.X, transform.Y), world.Arena.Width, world.Arena.Height);
        float blend = Math.Clamp(dt * 8f, 0f, 1f);
        Camera.Center = new Vec2(
            Camera.Center.X + (target.X - Camera.Center.X) * blend,
            Camera.Center.Y + (target.Y - Camera.Center.Y) * blend);
    }

    public void AddShake(float amount) => Shake = MathF.Min(0.8f, Shake + amount);

    public void AddHitStop(float seconds) => HitStop = MathF.Max(HitStop, seconds);

    public void EmitEffectEvents()
    {
        if (World is null) return;

        for (int i = 0; i < World.Deaths.Count; i++)
        {
            var death = World.Deaths[i];
            Particles.EmitBurst(death.Position, Palette.FromArgb(death.Color), 16, 9f);
        }
        if (World.Deaths.Count > 0) Audio.Play(Sfx.Kill);

        for (int i = 0; i < World.Hits.Count; i++)
        {
            var hit = World.Hits[i];
            if (!World.Entities.IsAlive(hit.Target) || !World.Entities.Has<Transform2>(hit.Target)) continue;
            var transform = World.Entities.Get<Transform2>(hit.Target);
            var position = new Vec2(transform.X, transform.Y);

            Color color = Palette.Projectile;
            if (World.Entities.Has<EnemyLink>(hit.Target))
            {
                color = Palette.FromArgb(World.Entities.Get<EnemyLink>(hit.Target).Instance.Spec.Color);
            }
            Particles.EmitBurst(position, color, 2, 5f);
        }

        for (int i = 0; i < World.DamageEvents.Count; i++)
        {
            var damage = World.DamageEvents[i];
            TotalDamageEvents++;
            int amount = System.Math.Max(1, (int)MathF.Round(damage.Amount));
            if (damage.Target == World.Player) Audio.Play(Sfx.Hurt);
            else Audio.Play(Sfx.Hit);

            if (damage.IsCrit)
            {
                Floating.Add(damage.Position, amount + "!", Palette.Gold, 26);
                AddHitStop(0.05f);
            }
            else
            {
                Floating.Add(damage.Position, amount.ToString(), Palette.Text, 20);
                if (damage.Amount >= 25f) AddHitStop(0.03f);
            }
        }

        for (int i = 0; i < World.HealEvents.Count; i++)
        {
            var heal = World.HealEvents[i];
            Floating.Add(heal.Position, "+" + (int)MathF.Round(heal.Amount), Palette.Health, 22);
        }
        if (World.HealEvents.Count > 0) Audio.Play(Sfx.Pickup);

        if (World.Level > _lastLevel)
        {
            Audio.Play(Sfx.LevelUp);
            _lastLevel = World.Level;
        }
        if (World.RunCredits > _lastCredits)
        {
            Audio.Play(Sfx.Pickup);
            _lastCredits = World.RunCredits;
        }
    }

    private void AdvanceTicks(int ticks)
    {
        for (int i = 0; i < ticks && World is not null && !World.IsGameOver; i++)
        {
            World.Input = ScriptedInput(World, i);
            World.Tick();
            EmitEffectEvents();
            if (UpgradeSystem!.HasOffers) UpgradeSystem.Choose(World, 0);
            Particles.Update(World.FixedDeltaSeconds);
            Floating.Update(World.FixedDeltaSeconds);
        }
    }

    private static InputState ScriptedInput(World world, int tick)
    {
        var move = ((tick / 45) % 4) switch
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

    private void UpdateShake(float dt)
    {
        if (!Session.Progression.Data.ShakeEnabled)
        {
            Shake = 0f;
            Camera.Offset = new Vector2(0f, 0f);
            return;
        }

        Shake = MathF.Max(0f, Shake - dt * 2.2f);
        float magnitude = Shake * Shake * 16f;
        Camera.Offset = magnitude <= 0.01f
            ? new Vector2(0f, 0f)
            : new Vector2(
                (float)(_shakeRandom.NextDouble() * 2.0 - 1.0) * magnitude,
                (float)(_shakeRandom.NextDouble() * 2.0 - 1.0) * magnitude);
    }

    private string NextScreenshotPath()
    {
        _screenshotIndex++;
        return Path.Combine(UserPaths.ScreenshotsDirectory, $"coreshift-{_screenshotIndex:00}.png");
    }

    public static void SaveScreenshot(string path)
    {
        string full = Path.GetFullPath(path);
        string? directory = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        // Raylib prepends its cached working directory to the filename, so pass a relative path.
        string relative = Path.GetRelativePath(Directory.GetCurrentDirectory(), full);
        Raylib.TakeScreenshot(relative);
    }
}

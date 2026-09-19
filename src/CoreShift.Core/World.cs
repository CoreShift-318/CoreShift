using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Math;
using CoreShift.Core.Progression;
using CoreShift.Core.Rng;
using CoreShift.Core.Systems;
using CoreShift.Core.Waves;

namespace CoreShift.Core;

public sealed class World
{
    public const float FixedDeltaSeconds = 1f / 60f;

    public World(uint seed, Arena arena)
    {
        Arena = arena;
        Rng = new DeterministicRng(seed);
        Entities = new EntityWorld();
        EnemyPool = new EnemyPool();
        ProjectilePool = new ProjectilePool();
        PickupPool = new PickupPool();
        Stats = new PlayerStats();
    }

    public Arena Arena { get; }
    public DeterministicRng Rng { get; }
    public EntityWorld Entities { get; }
    public EnemyPool EnemyPool { get; }
    public ProjectilePool ProjectilePool { get; }
    public PickupPool PickupPool { get; }
    public PlayerStats Stats { get; }

    public List<ISystem> Systems { get; } = new();
    public List<HitEvent> Hits { get; } = new();
    public List<DeathEvent> Deaths { get; } = new();
    public List<DamageEvent> DamageEvents { get; } = new();
    public List<HealEvent> HealEvents { get; } = new();
    public Dictionary<string, int> UpgradeStacks { get; } = new();

    public InputState Input;
    public Entity Player { get; set; } = Entity.Null;

    public float ElapsedSeconds { get; private set; }
    public int TickCount { get; private set; }
    public bool IsGameOver { get; set; }

    public int Xp;
    public int Level = 1;
    public float XpToNext = XpRequiredFor(1);
    public int PendingLevelUps;
    public int Wave = 1;
    public int Kills;
    public int RunCredits;

    public float AbilityCooldownRemaining;
    public float DashRemaining;
    public float DashDirX;
    public float DashDirY;
    public float DashSpeed;
    public float ChronoRemaining;
    public float ChronoFactor = 0.4f;

    public bool IsChronoActive => ChronoRemaining > 0f;

    public bool HasPlayer => !Player.IsNull && Entities.IsAlive(Player);

    public static int XpRequiredFor(int level) => 100 * level;

    public ulong ComputeStateHash()
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        ulong hash = offset;

        void Mix(ulong value)
        {
            hash ^= value;
            hash *= prime;
        }

        Mix((ulong)TickCount);
        Mix((ulong)(uint)Xp);
        Mix((ulong)(uint)Level);
        Mix((ulong)(uint)Kills);
        Mix((ulong)(uint)Wave);
        Mix((ulong)(uint)RunCredits);
        Mix(Rng.State);

        foreach (var entity in Entities.With<Transform2>())
        {
            var transform = Entities.Get<Transform2>(entity);
            Mix((ulong)(uint)entity.Id);
            Mix((ulong)(uint)BitConverter.SingleToInt32Bits(transform.X));
            Mix((ulong)(uint)BitConverter.SingleToInt32Bits(transform.Y));
        }

        foreach (var entity in Entities.With<Health>())
        {
            var health = Entities.Get<Health>(entity);
            Mix((ulong)(uint)entity.Id);
            Mix((ulong)(uint)BitConverter.SingleToInt32Bits(health.Current));
        }

        return hash;
    }

    public void AddSystem(ISystem system) => Systems.Add(system);

    public void Tick()
    {
        if (IsGameOver) return;

        Hits.Clear();
        Deaths.Clear();
        DamageEvents.Clear();
        HealEvents.Clear();
        float dt = FixedDeltaSeconds;
        for (int i = 0; i < Systems.Count; i++)
        {
            Systems[i].Update(this, dt);
        }
        ElapsedSeconds += dt;
        TickCount++;
    }

    public Entity CreatePlayer()
    {
        var entity = Entities.Create();
        Player = entity;
        Entities.Set(entity, new PlayerTag());
        Entities.Set(entity, new Transform2(Arena.Center.X, Arena.Center.Y));
        Entities.Set(entity, new Velocity(0f, 0f));
        Entities.Set(entity, new Health(Stats.MaxHealth));
        Entities.Set(entity, new Collider(0.5f));
        Entities.Set(entity, new Cooldown(1f / MathF.Max(0.01f, Stats.FireRate)));
        return entity;
    }

    public GameSnapshot Snapshot()
    {
        float health = 0f;
        float maxHealth = Stats.MaxHealth;
        if (HasPlayer && Entities.Has<Health>(Player))
        {
            var h = Entities.Get<Health>(Player);
            health = MathF.Max(0f, h.Current);
        }

        return new GameSnapshot
        {
            Health = health,
            MaxHealth = maxHealth,
            Level = Level,
            Xp = Xp,
            XpToNext = XpToNext,
            Wave = Wave,
            EntityCount = Entities.EntityCount,
            Kills = Kills,
            RunCredits = RunCredits,
            IsGameOver = IsGameOver,
        };
    }

    public void Reset()
    {
        Entities.Clear();
        Hits.Clear();
        Deaths.Clear();
        DamageEvents.Clear();
        HealEvents.Clear();
        UpgradeStacks.Clear();
        Input = InputState.None;
        Player = Entity.Null;
        ElapsedSeconds = 0f;
        TickCount = 0;
        IsGameOver = false;
        Xp = 0;
        Level = 1;
        XpToNext = XpRequiredFor(1);
        PendingLevelUps = 0;
        Wave = 1;
        Kills = 0;
        RunCredits = 0;
        AbilityCooldownRemaining = 0f;
        DashRemaining = 0f;
        DashSpeed = 0f;
        ChronoRemaining = 0f;
    }
}

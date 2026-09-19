using CoreShift.Core;
using CoreShift.Core.Math;
using CoreShift.Core.Progression;
using CoreShift.Core.Systems;
using CoreShift.Core.Upgrades;
using CoreShift.Core.Waves;
using CoreShift.Data.Content;

namespace CoreShift.Data;

public static class GameFactory
{
    public static World Build(
        uint seed,
        GameContent content,
        ProgressionState? progression = null,
        bool withWaves = true,
        bool guaranteedUpgrades = false)
    {
        var world = new World(seed, new Arena(64f, 36f));

        progression?.ApplyTo(world.Stats);
        var character = CharacterCatalog.Find(progression?.Data.SelectedCharacter);
        if (character is not null) CharacterCatalog.Apply(character, world.Stats);
        world.CreatePlayer();

        world.AddSystem(new InputSystem());
        world.AddSystem(new AbilitySystem());
        world.AddSystem(new MovementSystem());
        if (withWaves) world.AddSystem(new WaveSystem(new WaveSpawner(WaveTableFactory.FromContent(content))));
        world.AddSystem(new EnemyAiSystem());
        world.AddSystem(new PlayerWeaponSystem());
        world.AddSystem(new ProjectileSystem());
        world.AddSystem(new StatusSystem());
        world.AddSystem(new CollisionSystem());
        world.AddSystem(new CombatSystem());
        world.AddSystem(new LootSystem());
        world.AddSystem(new UpgradeSystem(content.ToUpgradeDefs(), guaranteedUpgrades));
        if (progression is not null) world.AddSystem(new ProgressionSystem(progression));
        world.AddSystem(new PickupSystem());
        world.AddSystem(new CleanupSystem());

        return world;
    }

    public static World BuildBenchmark(uint seed, int enemyCount)
    {
        var world = new World(seed, new Arena(200f, 120f));
        world.CreatePlayer();
        world.AddSystem(new MovementSystem());
        world.AddSystem(new EnemyAiSystem());
        world.AddSystem(new CollisionSystem());
        world.AddSystem(new CleanupSystem());

        var spec = new EnemySpec
        {
            Id = "bench",
            Name = "Bench",
            MaxHealth = 1_000_000_000f,
            Speed = 1.5f,
            Damage = 0f,
            Radius = 0.5f,
            XpReward = 0,
        };

        for (int i = 0; i < enemyCount; i++)
        {
            var position = new Vec2(world.Rng.Range(-100f, 100f), world.Rng.Range(-60f, 60f));
            EnemyFactory.Spawn(world, spec, position, 1);
        }

        return world;
    }
}

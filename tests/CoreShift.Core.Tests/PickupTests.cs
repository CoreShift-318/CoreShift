using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Progression;
using CoreShift.Core.Systems;

namespace CoreShift.Core.Tests;

public class PickupTests
{
    private static World MakeWorld(float pickupRadius = 2f)
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        world.Stats.PickupRadius = pickupRadius;
        return world;
    }

    private static Entity SpawnPickup(World world, PickupKind kind, float value, float x, float y)
    {
        var entity = world.Entities.Create();
        world.Entities.Set(entity, new PickupTag());
        world.Entities.Set(entity, new Transform2(x, y));
        world.Entities.Set(entity, new Velocity(0f, 0f));
        world.Entities.Set(entity, new Pickup(kind, value));
        world.Entities.Set(entity, new Lifetime(20f));
        return entity;
    }

    [Fact]
    public void LootDrops_AreDeterministicForSeed()
    {
        int FirstRun()
        {
            var world = new World(4242, new Arena(100, 100));
            world.CreatePlayer();
            for (int i = 0; i < 200; i++)
            {
                world.Deaths.Add(new DeathEvent(new CoreShift.Core.Math.Vec2(i % 10, i / 10), unchecked((int)0xFFE06060)));
            }
            new LootSystem().Update(world, 1f / 60f);
            int count = 0;
            foreach (var _ in world.Entities.With<PickupTag>()) count++;
            return count;
        }

        Assert.Equal(FirstRun(), FirstRun());
    }

    [Fact]
    public void HealthPickup_HealsPlayer()
    {
        var world = MakeWorld();
        var playerHealth = world.Entities.Get<Health>(world.Player);
        playerHealth.Current = 50f;
        world.Entities.Set(world.Player, playerHealth);

        SpawnPickup(world, PickupKind.Health, 20f, 0.2f, 0f);
        new PickupSystem().Update(world, 1f / 60f);

        Assert.Equal(70f, world.Entities.Get<Health>(world.Player).Current, 3);
        Assert.Single(world.HealEvents);
    }

    [Fact]
    public void CreditPickup_AddsRunCredits()
    {
        var world = MakeWorld();
        SpawnPickup(world, PickupKind.Credits, 3f, 0.2f, 0f);

        new PickupSystem().Update(world, 1f / 60f);

        Assert.Equal(3, world.RunCredits);
    }

    [Fact]
    public void Magnet_PullsDistantPickupInsideRadius()
    {
        var world = MakeWorld(pickupRadius: 3f);
        var pickup = SpawnPickup(world, PickupKind.Credits, 1f, 2.5f, 0f);

        float before = 2.5f;
        new PickupSystem().Update(world, 1f / 60f);
        float after = world.Entities.Get<Transform2>(pickup).X;

        Assert.True(after < before, "pickup should move toward the player");
    }

    [Fact]
    public void PickupOutsideRadius_IsNotCollected()
    {
        var world = MakeWorld(pickupRadius: 1f);
        SpawnPickup(world, PickupKind.Credits, 5f, 40f, 0f);

        new PickupSystem().Update(world, 1f / 60f);

        Assert.Equal(0, world.RunCredits);
    }

    [Fact]
    public void RunReward_IncludesRunCredits()
    {
        var progression = new ProgressionState();
        int gain = progression.CompleteRun(waveReached: 1, kills: 0, seconds: 0, runCredits: 5);
        Assert.Equal(15, gain);
        Assert.Equal(15, progression.Data.Currency);
    }
}

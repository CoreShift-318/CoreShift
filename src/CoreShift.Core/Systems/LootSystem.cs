using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Math;

namespace CoreShift.Core.Systems;

public sealed class LootSystem : ISystem
{
    public const float HealthDropChance = 0.10f;
    public const float CreditDropChance = 0.20f;

    public void Update(World world, float dt)
    {
        for (int i = 0; i < world.Deaths.Count; i++)
        {
            var death = world.Deaths[i];

            if (world.Rng.Chance(HealthDropChance))
            {
                SpawnPickup(world, death.Position, PickupKind.Health, 15f);
            }
            if (world.Rng.Chance(CreditDropChance))
            {
                SpawnPickup(world, death.Position, PickupKind.Credits, 1 + world.Rng.NextInt(0, 3));
            }
        }
    }

    private static void SpawnPickup(World world, Vec2 position, PickupKind kind, float value)
    {
        if (!world.PickupPool.TryRent(out var instance)) return;

        var entity = world.Entities.Create();
        instance.Entity = entity;

        world.Entities.Set(entity, new PickupTag());
        world.Entities.Set(entity, new Transform2(position.X, position.Y));
        world.Entities.Set(entity, new Velocity(0f, 0f));
        world.Entities.Set(entity, new Pickup(kind, value));
        world.Entities.Set(entity, new Lifetime(20f));
        world.Entities.Set(entity, new PickupLink { Instance = instance });
    }
}

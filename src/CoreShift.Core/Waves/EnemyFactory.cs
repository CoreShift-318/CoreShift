using CoreShift.Core.Ecs;
using CoreShift.Core.Math;

namespace CoreShift.Core.Waves;

public static class EnemyFactory
{
    public static Entity Spawn(World world, EnemySpec spec, Vec2 position, int wave)
        => Spawn(world, spec, position, wave, WaveScaling.HealthMultiplier(wave), WaveScaling.SpeedMultiplier(wave));

    public static Entity Spawn(World world, EnemySpec spec, Vec2 position, int wave, float healthMultiplier, float speedMultiplier)
    {
        if (!world.EnemyPool.TryRent(out var instance)) return Entity.Null;

        var entity = world.Entities.Create();
        instance.Entity = entity;
        instance.Spec = spec;
        instance.Wave = wave;

        world.Entities.Set(entity, new EnemyTag());
        world.Entities.Set(entity, new Transform2(position.X, position.Y));
        world.Entities.Set(entity, new Velocity(0f, 0f));
        world.Entities.Set(entity, new Health(spec.MaxHealth * healthMultiplier));
        world.Entities.Set(entity, new Collider(spec.Radius));
        world.Entities.Set(entity, new EnemyAi(spec.Speed * speedMultiplier));
        var onHit = spec.OnHitStatus == Combat.StatusKind.None
            ? Combat.StatusApplication.None
            : new Combat.StatusApplication(spec.OnHitStatus, spec.OnHitMagnitude, spec.OnHitDuration, spec.OnHitChance);
        world.Entities.Set(entity, new Damage(spec.Damage) { OnHit = onHit });
        world.Entities.Set(entity, new XpReward(spec.XpReward));
        world.Entities.Set(entity, new Cooldown(0.8f) { Remaining = 0.8f });
        world.Entities.Set(entity, new EnemyLink { Instance = instance });
        return entity;
    }
}

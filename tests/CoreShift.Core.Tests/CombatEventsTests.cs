using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Math;
using CoreShift.Core.Systems;

namespace CoreShift.Core.Tests;

public class CombatEventsTests
{
    private static (World world, Entity target) Setup()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        var target = world.Entities.Create();
        world.Entities.Set(target, new EnemyTag());
        world.Entities.Set(target, new Health(100f));
        world.Entities.Set(target, new Transform2(0f, 0f));
        world.Entities.Set(target, new Velocity(0f, 0f));
        return (world, target);
    }

    [Fact]
    public void Knockback_ChangesVelocity()
    {
        var (world, target) = Setup();
        world.Hits.Add(new HitEvent(world.Player, target, 5f, false, new Vec2(3f, 0f), StatusApplication.None));

        new CombatSystem().Update(world, 1f / 60f);

        Assert.Equal(3f, world.Entities.Get<Velocity>(target).X, 3);
    }

    [Fact]
    public void DamageEvent_IsEmitted()
    {
        var (world, target) = Setup();
        world.Hits.Add(new HitEvent(world.Player, target, 12f));

        new CombatSystem().Update(world, 1f / 60f);

        Assert.Single(world.DamageEvents);
        Assert.Equal(12f, world.DamageEvents[0].Amount, 3);
        Assert.Equal(target, world.DamageEvents[0].Target);
    }

    [Fact]
    public void CriticalHit_IsFlaggedInEvent()
    {
        var (world, target) = Setup();
        world.Hits.Add(new HitEvent(world.Player, target, 30f, true, Vec2.Zero, StatusApplication.None));

        new CombatSystem().Update(world, 1f / 60f);

        Assert.Single(world.DamageEvents);
        Assert.True(world.DamageEvents[0].IsCrit);
    }

    [Fact]
    public void OnHitStatus_IsApplied()
    {
        var (world, target) = Setup();
        var application = new StatusApplication(StatusKind.Burn, 5f, 3f, 1f);
        world.Hits.Add(new HitEvent(world.Player, target, 1f, false, Vec2.Zero, application));

        new CombatSystem().Update(world, 1f / 60f);

        Assert.True(world.Entities.Has<BurnStatus>(target));
    }
}

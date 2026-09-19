using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Systems;

namespace CoreShift.Core.Tests;

public class StatusSystemTests
{
    private readonly StatusSystem _status = new();
    private readonly CombatSystem _combat = new();

    private static World MakeWorld() => new(1, new Arena(100, 100));

    private static Entity MakeTarget(World world, float health = 100f)
    {
        var entity = world.Entities.Create();
        world.Entities.Set(entity, new EnemyTag());
        world.Entities.Set(entity, new Health(health));
        world.Entities.Set(entity, new Transform2(0f, 0f));
        return entity;
    }

    [Fact]
    public void Burn_TicksDamage()
    {
        var world = MakeWorld();
        var target = MakeTarget(world);
        world.Entities.Set(target, new BurnStatus { Remaining = 10f, TickTimer = 0f, DamagePerSecond = 10f });

        _status.Update(world, 1f);
        _combat.Update(world, 1f);

        Assert.Equal(90f, world.Entities.Get<Health>(target).Current, 3);
    }

    [Fact]
    public void Burn_ExpiresAndIsRemoved()
    {
        var world = MakeWorld();
        var target = MakeTarget(world);
        world.Entities.Set(target, new BurnStatus { Remaining = 0.5f, TickTimer = 5f, DamagePerSecond = 10f });

        _status.Update(world, 1f);

        Assert.False(world.Entities.Has<BurnStatus>(target));
    }

    [Fact]
    public void Poison_StacksMultiplyDamage()
    {
        var world = MakeWorld();
        var target = MakeTarget(world);
        world.Entities.Set(target, new PoisonStatus { Remaining = 10f, TickTimer = 0f, DamagePerSecond = 3f, Stacks = 2 });

        _status.Update(world, 1f);
        _combat.Update(world, 1f);

        Assert.Equal(94f, world.Entities.Get<Health>(target).Current, 3);
    }

    [Fact]
    public void Slow_ReducesMovement()
    {
        var world = MakeWorld();
        var entity = world.Entities.Create();
        world.Entities.Set(entity, new Transform2(0f, 0f));
        world.Entities.Set(entity, new Velocity(10f, 0f));
        world.Entities.Set(entity, new SlowStatus { Remaining = 5f, Factor = 0.5f });

        new MovementSystem().Update(world, 1f);

        Assert.Equal(5f, world.Entities.Get<Transform2>(entity).X, 3);
    }

    [Fact]
    public void Stun_StopsMovement()
    {
        var world = MakeWorld();
        var entity = world.Entities.Create();
        world.Entities.Set(entity, new Transform2(0f, 0f));
        world.Entities.Set(entity, new Velocity(10f, 0f));
        world.Entities.Set(entity, new StunStatus { Remaining = 5f });

        new MovementSystem().Update(world, 1f);

        Assert.Equal(0f, world.Entities.Get<Transform2>(entity).X, 3);
    }

    [Fact]
    public void Shock_AmplifiesDamage()
    {
        var world = MakeWorld();
        var target = MakeTarget(world);
        world.Entities.Set(target, new ShockStatus { Remaining = 5f, Amplifier = 0.5f });

        world.Hits.Add(new HitEvent(Entity.Null, target, 10f));
        _combat.Update(world, 1f / 60f);

        Assert.Equal(85f, world.Entities.Get<Health>(target).Current, 3);
    }

    [Fact]
    public void Apply_SetsComponent()
    {
        var world = MakeWorld();
        var target = MakeTarget(world);

        StatusSystem.Apply(world, target, new StatusApplication(StatusKind.Burn, 7f, 4f, 1f));

        Assert.True(world.Entities.Has<BurnStatus>(target));
        var burn = world.Entities.Get<BurnStatus>(target);
        Assert.Equal(7f, burn.DamagePerSecond, 3);
        Assert.Equal(4f, burn.Remaining, 3);
    }

    [Fact]
    public void Apply_WithZeroChance_DoesNothing()
    {
        var world = MakeWorld();
        var target = MakeTarget(world);

        StatusSystem.Apply(world, target, new StatusApplication(StatusKind.Slow, 0.5f, 2f, 0f));

        Assert.False(world.Entities.Has<SlowStatus>(target));
    }
}

using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Systems;

namespace CoreShift.Core.Tests;

public class PlayerWeaponSystemTests
{
    private static World MakeWorld()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();
        return world;
    }

    private static int ProjectileCount(World world)
    {
        int count = 0;
        foreach (var _ in world.Entities.With<ProjectileTag>()) count++;
        return count;
    }

    [Fact]
    public void DoesNotFire_WhenNotFiring()
    {
        var world = MakeWorld();
        world.Input = InputState.None;

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        Assert.Equal(0, ProjectileCount(world));
    }

    [Fact]
    public void FiresAlongAim_WhenFiring()
    {
        var world = MakeWorld();
        world.Input = new InputState { AimX = 1f, AimY = 0f, Firing = true };

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        Assert.Equal(1, ProjectileCount(world));
        foreach (var projectile in world.Entities.With<ProjectileTag>())
        {
            var velocity = world.Entities.Get<Velocity>(projectile);
            Assert.True(velocity.X > 0f);
            Assert.Equal(0f, velocity.Y, 2);
        }
    }

    [Fact]
    public void DoesNotAutoTarget_WhenNotFiring()
    {
        var world = MakeWorld();
        var enemy = world.Entities.Create();
        world.Entities.Set(enemy, new EnemyTag());
        world.Entities.Set(enemy, new Transform2(1f, 0f));
        world.Entities.Set(enemy, new Health(10f));

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        Assert.Equal(0, ProjectileCount(world));
    }

    [Fact]
    public void RespectsFireRate()
    {
        var world = MakeWorld();
        world.Input = new InputState { AimX = 1f, Firing = true };

        var weapon = new PlayerWeaponSystem();
        weapon.Update(world, 1f / 60f);
        Assert.Equal(1, ProjectileCount(world));

        weapon.Update(world, 1f / 60f);
        Assert.Equal(1, ProjectileCount(world));
    }

    [Fact]
    public void CritRoll_AppliesMultiplierAndFlag()
    {
        var world = MakeWorld();
        world.Stats.CritChance = 1f;
        world.Stats.CritMultiplier = 3f;
        world.Stats.Damage = 10f;
        world.Input = new InputState { AimX = 1f, Firing = true };

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        foreach (var projectile in world.Entities.With<ProjectileTag>())
        {
            var damage = world.Entities.Get<Damage>(projectile);
            Assert.True(damage.IsCrit);
            Assert.Equal(30f, damage.Amount, 3);
        }
    }

    [Fact]
    public void NonCrit_UsesBaseDamage()
    {
        var world = MakeWorld();
        world.Stats.CritChance = 0f;
        world.Stats.Damage = 10f;
        world.Input = new InputState { AimX = 1f, Firing = true };

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        foreach (var projectile in world.Entities.With<ProjectileTag>())
        {
            var damage = world.Entities.Get<Damage>(projectile);
            Assert.False(damage.IsCrit);
            Assert.Equal(10f, damage.Amount, 3);
        }
    }

    [Fact]
    public void AimDirection_IsSetFromInput()
    {
        var world = MakeWorld();
        world.Input = new InputState { AimX = 0f, AimY = 1f };

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        var aim = world.Entities.Get<AimDirection>(world.Player);
        Assert.Equal(0f, aim.X, 3);
        Assert.Equal(1f, aim.Y, 3);
    }

    [Fact]
    public void AimDirection_RetainsLastWhenInputZero()
    {
        var world = MakeWorld();
        world.Input = new InputState { AimX = 1f, AimY = 0f };
        new PlayerWeaponSystem().Update(world, 1f / 60f);

        world.Input = InputState.None;
        new PlayerWeaponSystem().Update(world, 1f / 60f);

        var aim = world.Entities.Get<AimDirection>(world.Player);
        Assert.Equal(1f, aim.X, 3);
        Assert.Equal(0f, aim.Y, 3);
    }

    [Fact]
    public void AimDirection_DefaultsToRight()
    {
        var world = MakeWorld();
        world.Input = InputState.None;

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        var aim = world.Entities.Get<AimDirection>(world.Player);
        Assert.Equal(1f, aim.X, 3);
        Assert.Equal(0f, aim.Y, 3);
    }
}

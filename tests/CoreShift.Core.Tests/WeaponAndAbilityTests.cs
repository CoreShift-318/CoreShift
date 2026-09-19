using CoreShift.Core;
using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Systems;

namespace CoreShift.Core.Tests;

public class WeaponAndAbilityTests
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
    public void Shotgun_FiresMultiplePellets()
    {
        var world = MakeWorld();
        world.Stats.Weapon = WeaponKind.Shotgun;
        world.Input = new InputState { AimX = 1f, Firing = true };

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        Assert.Equal(5, ProjectileCount(world));
    }

    [Fact]
    public void Laser_ProjectilePierces()
    {
        var world = MakeWorld();
        world.Stats.Weapon = WeaponKind.Laser;
        world.Input = new InputState { AimX = 1f, Firing = true };

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        Assert.Equal(1, ProjectileCount(world));
        foreach (var projectile in world.Entities.With<ProjectileTag>())
        {
            Assert.True(world.Entities.Get<Damage>(projectile).Pierce);
        }
    }

    [Fact]
    public void Blaster_UsesProjectileCount()
    {
        var world = MakeWorld();
        world.Stats.ProjectileCount = 3;
        world.Input = new InputState { AimX = 1f, Firing = true };

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        Assert.Equal(3, ProjectileCount(world));
    }

    [Fact]
    public void Dash_StartsDashAndCooldown()
    {
        var world = MakeWorld();
        world.Stats.Ability = AbilityKind.Dash;
        world.Input = new InputState { MoveX = 1f, Ability = true };

        new AbilitySystem().Update(world, 1f / 60f);

        Assert.True(world.DashRemaining > 0f);
        Assert.True(world.AbilityCooldownRemaining > 0f);
    }

    [Fact]
    public void ChronoSlow_ActivatesTimeSlow()
    {
        var world = MakeWorld();
        world.Stats.Ability = AbilityKind.ChronoSlow;
        world.Input = new InputState { Ability = true };

        new AbilitySystem().Update(world, 1f / 60f);

        Assert.True(world.IsChronoActive);
    }

    [Fact]
    public void Ability_RespectsCooldown()
    {
        var world = MakeWorld();
        world.Stats.Ability = AbilityKind.Dash;
        var ability = new AbilitySystem();

        world.Input = new InputState { MoveX = 1f, Ability = true };
        ability.Update(world, 1f / 60f);
        float firstCooldown = world.AbilityCooldownRemaining;

        world.Input = new InputState { MoveX = 1f, Ability = false };
        ability.Update(world, 1f / 60f);
        world.Input = new InputState { MoveX = 1f, Ability = true };
        ability.Update(world, 1f / 60f);

        Assert.True(world.AbilityCooldownRemaining < firstCooldown);
    }

    [Fact]
    public void ChronoSlow_ScalesEnemySpeed()
    {
        var world = MakeWorld();
        var enemy = world.Entities.Create();
        world.Entities.Set(enemy, new EnemyTag());
        world.Entities.Set(enemy, new Transform2(10f, 0f));
        world.Entities.Set(enemy, new EnemyAi(4f));
        world.ChronoRemaining = 3f;

        new EnemyAiSystem().Update(world, 1f / 60f);

        var velocity = world.Entities.Get<Velocity>(enemy);
        Assert.Equal(4f * world.ChronoFactor, MathF.Abs(velocity.X), 3);
    }

    [Fact]
    public void AimAssist_PullsAimTowardNearbyEnemy()
    {
        var world = MakeWorld();
        world.Stats.AimAssist = 1f;
        var enemy = world.Entities.Create();
        world.Entities.Set(enemy, new EnemyTag());
        world.Entities.Set(enemy, new Transform2(10f, 1f));
        world.Entities.Set(enemy, new Health(100f));
        world.Input = new InputState { AimX = 1f, AimY = 0f, Firing = true };

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        bool any = false;
        foreach (var projectile in world.Entities.With<ProjectileTag>())
        {
            any = true;
            Assert.True(world.Entities.Get<Velocity>(projectile).Y > 0f);
        }
        Assert.True(any);
    }

    [Fact]
    public void NoAimAssist_KeepsRawAim()
    {
        var world = MakeWorld();
        world.Stats.AimAssist = 0f;
        var enemy = world.Entities.Create();
        world.Entities.Set(enemy, new EnemyTag());
        world.Entities.Set(enemy, new Transform2(10f, 8f));
        world.Entities.Set(enemy, new Health(100f));
        world.Input = new InputState { AimX = 1f, AimY = 0f, Firing = true };

        new PlayerWeaponSystem().Update(world, 1f / 60f);

        foreach (var projectile in world.Entities.With<ProjectileTag>())
        {
            Assert.Equal(0f, world.Entities.Get<Velocity>(projectile).Y, 2);
        }
    }
}

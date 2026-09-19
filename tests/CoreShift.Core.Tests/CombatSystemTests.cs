using CoreShift.Core.Combat;
using CoreShift.Core.Ecs;
using CoreShift.Core.Systems;

namespace CoreShift.Core.Tests;

public class CombatSystemTests
{
    [Fact]
    public void Damage_KillsEnemy_AndAwardsXp()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();

        var enemy = world.Entities.Create();
        world.Entities.Set(enemy, new EnemyTag());
        world.Entities.Set(enemy, new Health(10f));
        world.Entities.Set(enemy, new XpReward(7));

        world.Hits.Add(new HitEvent(world.Player, enemy, 10f));
        new CombatSystem().Update(world, 1f / 60f);

        Assert.False(world.Entities.IsAlive(enemy));
        Assert.Equal(7, world.Xp);
        Assert.Equal(1, world.Kills);
    }

    [Fact]
    public void PlayerDamage_TriggersGameOver()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();

        world.Hits.Add(new HitEvent(Entity.Null, world.Player, 9999f));
        new CombatSystem().Update(world, 1f / 60f);

        Assert.True(world.IsGameOver);
    }

    [Fact]
    public void KillingEnough_TriggersLevelUp()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();

        for (int i = 0; i < 10; i++)
        {
            var enemy = world.Entities.Create();
            world.Entities.Set(enemy, new EnemyTag());
            world.Entities.Set(enemy, new Health(1f));
            world.Entities.Set(enemy, new XpReward(10));
            world.Hits.Add(new HitEvent(world.Player, enemy, 1f));
        }

        new CombatSystem().Update(world, 1f / 60f);

        Assert.Equal(2, world.Level);
        Assert.Equal(1, world.PendingLevelUps);
    }

    [Fact]
    public void Projectile_ExpiresAndCleansUp()
    {
        var world = new World(1, new Arena(100, 100));
        world.CreatePlayer();

        var projectile = world.Entities.Create();
        world.Entities.Set(projectile, new ProjectileTag());
        world.Entities.Set(projectile, new Transform2(0, 0));
        world.Entities.Set(projectile, new Velocity(0, 0));
        world.Entities.Set(projectile, new Lifetime(0.01f));

        new ProjectileSystem().Update(world, 1f / 60f);
        new CleanupSystem().Update(world, 1f / 60f);

        Assert.False(world.Entities.IsAlive(projectile));
    }
}

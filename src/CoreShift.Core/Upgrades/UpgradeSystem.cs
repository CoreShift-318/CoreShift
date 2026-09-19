using CoreShift.Core.Ecs;
using CoreShift.Core.Systems;

namespace CoreShift.Core.Upgrades;

public sealed class UpgradeSystem : ISystem
{
    private readonly List<UpgradeDef> _defs;
    private readonly bool _guaranteed;
    private readonly List<UpgradeDef> _offers = new();
    private readonly List<UpgradeDef> _candidates = new();
    private readonly List<UpgradeDef> _rolling = new();
    private readonly HashSet<string> _banned = new();

    public UpgradeSystem(List<UpgradeDef> defs, bool guaranteed = false)
    {
        _defs = defs ?? new List<UpgradeDef>();
        _guaranteed = guaranteed;
    }

    public IReadOnlyList<UpgradeDef> CurrentOffers => _offers;

    public bool HasOffers => _offers.Count > 0;

    public void Update(World world, float dt)
    {
        if (world.PendingLevelUps > 0 && _offers.Count == 0)
        {
            Offer(world);
        }
    }

    public void Offer(World world)
    {
        _offers.Clear();
        _candidates.Clear();

        for (int i = 0; i < _defs.Count; i++)
        {
            var def = _defs[i];
            if (_banned.Contains(def.Id)) continue;
            int stacks = world.UpgradeStacks.GetValueOrDefault(def.Id);
            if (def.MaxStacks > 0 && stacks >= def.MaxStacks) continue;
            _candidates.Add(def);
        }

        _candidates.Sort(static (a, b) => string.CompareOrdinal(a.Id, b.Id));

        int take = System.Math.Min(3, _candidates.Count);

        if (_guaranteed)
        {
            for (int i = 0; i < take; i++) _offers.Add(_candidates[i]);
            return;
        }

        _rolling.Clear();
        _rolling.AddRange(_candidates);
        for (int n = 0; n < take; n++)
        {
            int totalWeight = 0;
            for (int i = 0; i < _rolling.Count; i++) totalWeight += _rolling[i].Weight;

            int roll = world.Rng.NextInt(0, totalWeight);
            int accumulated = 0;
            int chosen = 0;
            for (int i = 0; i < _rolling.Count; i++)
            {
                accumulated += _rolling[i].Weight;
                if (roll < accumulated)
                {
                    chosen = i;
                    break;
                }
            }

            _offers.Add(_rolling[chosen]);
            _rolling.RemoveAt(chosen);
        }
    }

    public void Choose(World world, int index)
    {
        if (index < 0 || index >= _offers.Count) return;

        var def = _offers[index];
        Apply(world, def);
        world.UpgradeStacks[def.Id] = world.UpgradeStacks.GetValueOrDefault(def.Id) + 1;
        _offers.Clear();
        world.PendingLevelUps = System.Math.Max(0, world.PendingLevelUps - 1);
    }

    /// <summary>Rerolls the current offers (free).</summary>
    public void Reroll(World world) => Offer(world);

    /// <summary>Removes an offer from the pool for the rest of the run.</summary>
    public bool Banish(World world, int index)
    {
        if (index < 0 || index >= _offers.Count) return false;
        var def = _offers[index];
        _banned.Add(def.Id);
        _offers.Clear();
        Offer(world);
        return true;
    }

    private static void Apply(World world, UpgradeDef def)
    {
        var stats = world.Stats;
        switch (def.Kind)
        {
            case UpgradeKind.MaxHealth:
                stats.MaxHealth += def.Magnitude;
                if (world.HasPlayer && world.Entities.Has<Health>(world.Player))
                {
                    ref var health = ref world.Entities.GetRef<Health>(world.Player);
                    health.Max += def.Magnitude;
                    health.Current += def.Magnitude;
                }
                break;
            case UpgradeKind.MoveSpeed:
                stats.MoveSpeed += def.Magnitude;
                break;
            case UpgradeKind.Damage:
                stats.Damage += def.Magnitude;
                break;
            case UpgradeKind.FireRate:
                stats.FireRate += def.Magnitude;
                break;
            case UpgradeKind.ProjectileCount:
                stats.ProjectileCount += (int)def.Magnitude;
                break;
            case UpgradeKind.XpGain:
                stats.XpMultiplier += def.Magnitude;
                break;
            case UpgradeKind.CritChance:
                stats.CritChance += def.Magnitude;
                break;
            case UpgradeKind.CritDamage:
                stats.CritMultiplier += def.Magnitude;
                break;
            case UpgradeKind.BurnRounds:
                stats.WeaponStatus = Combat.StatusKind.Burn;
                stats.WeaponStatusMagnitude = def.Magnitude;
                stats.WeaponStatusDuration = 3f;
                stats.WeaponStatusChance = 1f;
                break;
            case UpgradeKind.SlowRounds:
                stats.WeaponStatus = Combat.StatusKind.Slow;
                stats.WeaponStatusMagnitude = def.Magnitude;
                stats.WeaponStatusDuration = 2f;
                stats.WeaponStatusChance = 1f;
                break;
            case UpgradeKind.PickupRadius:
                stats.PickupRadius += def.Magnitude;
                break;
            case UpgradeKind.WeaponShotgun:
                stats.Weapon = WeaponKind.Shotgun;
                break;
            case UpgradeKind.WeaponLaser:
                stats.Weapon = WeaponKind.Laser;
                break;
        }
    }
}

namespace CoreShift.Core;

public enum WeaponKind
{
    Blaster,
    Shotgun,
    Laser,
}

public enum AbilityKind
{
    Dash,
    ChronoSlow,
}

public sealed class PlayerStats
{
    public float MaxHealth = 100f;
    public float MoveSpeed = 8f;
    public float Damage = 10f;
    public float FireRate = 8f;
    public int ProjectileCount = 1;
    public float XpMultiplier = 1f;
    public float WeaponRange = 8f;
    public float PickupRadius = 2f;

    public float Acceleration = 18f;
    public float AimAssist = 0.35f;

    public float CritChance = 0.05f;
    public float CritMultiplier = 2f;
    public float Knockback = 4f;

    public Combat.StatusKind WeaponStatus = Combat.StatusKind.None;
    public float WeaponStatusMagnitude = 0f;
    public float WeaponStatusDuration = 0f;
    public float WeaponStatusChance = 1f;

    public WeaponKind Weapon = WeaponKind.Blaster;
    public AbilityKind Ability = AbilityKind.Dash;
    public float AbilityCooldown = 6f;

    public PlayerStats Clone() => new()
    {
        MaxHealth = MaxHealth,
        MoveSpeed = MoveSpeed,
        Damage = Damage,
        FireRate = FireRate,
        ProjectileCount = ProjectileCount,
        XpMultiplier = XpMultiplier,
        WeaponRange = WeaponRange,
        PickupRadius = PickupRadius,
        Acceleration = Acceleration,
        AimAssist = AimAssist,
        CritChance = CritChance,
        CritMultiplier = CritMultiplier,
        Knockback = Knockback,
        WeaponStatus = WeaponStatus,
        WeaponStatusMagnitude = WeaponStatusMagnitude,
        WeaponStatusDuration = WeaponStatusDuration,
        WeaponStatusChance = WeaponStatusChance,
        Weapon = Weapon,
        Ability = Ability,
        AbilityCooldown = AbilityCooldown,
    };
}

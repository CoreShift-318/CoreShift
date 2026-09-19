namespace CoreShift.Core.Progression;

public sealed class CharacterDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public float MaxHealthBonus { get; set; }
    public float MoveSpeedBonus { get; set; }
    public float DamageBonus { get; set; }
    public float FireRateBonus { get; set; }
    public float CritChanceBonus { get; set; }
    public WeaponKind Weapon { get; set; } = WeaponKind.Blaster;
    public AbilityKind Ability { get; set; } = AbilityKind.Dash;
}

public static class CharacterCatalog
{
    public static List<CharacterDef> Definitions() => new()
    {
        new CharacterDef
        {
            Id = "scout", Name = "Scout", Description = "Balanced blaster runner",
            Weapon = WeaponKind.Blaster, Ability = AbilityKind.Dash,
        },
        new CharacterDef
        {
            Id = "bruiser", Name = "Bruiser", Description = "Tanky shotgunner, slow",
            MaxHealthBonus = 50f, MoveSpeedBonus = -1.5f, DamageBonus = 3f,
            Weapon = WeaponKind.Shotgun, Ability = AbilityKind.Dash,
        },
        new CharacterDef
        {
            Id = "techno", Name = "Techno", Description = "Piercing laser, Chrono slow",
            CritChanceBonus = 0.1f, FireRateBonus = 1f,
            Weapon = WeaponKind.Laser, Ability = AbilityKind.ChronoSlow,
        },
    };

    public static CharacterDef? Find(string? id)
    {
        var definitions = Definitions();
        for (int i = 0; i < definitions.Count; i++)
        {
            if (definitions[i].Id == id) return definitions[i];
        }
        return null;
    }

    public static void Apply(CharacterDef character, PlayerStats stats)
    {
        stats.MaxHealth += character.MaxHealthBonus;
        stats.MoveSpeed += character.MoveSpeedBonus;
        stats.Damage += character.DamageBonus;
        stats.FireRate += character.FireRateBonus;
        stats.CritChance += character.CritChanceBonus;
        stats.Weapon = character.Weapon;
        stats.Ability = character.Ability;
    }
}

namespace Content.Server._Ekrixi.ShipWeapons;

public static class ShipWeaponConstants
{
    /// <summary>
    /// Command name when radar data is updated.
    /// </summary>
    public const string CommandUpdateRadar = "EkrixiUpdateRadar";
    /// <summary>
    /// Command name when gunnery data is updated.
    /// </summary>
    public const string CommandUpdateGunnery = "EkrixiUpdateGunnery";
    /// <summary>
    /// Command name when gunnery data is updated.
    /// </summary>
    public const string TargetCoordinate = "target_coordinate";
    /// <summary>
    /// How much ammo does this turret have?
    /// </summary>
    public const string AmmoCount = "ammo_count";
    /// <summary>
    /// How much ammo can this turret hold?
    /// </summary>
    public const string MaxAmmoCount = "ammo_count_max";
}

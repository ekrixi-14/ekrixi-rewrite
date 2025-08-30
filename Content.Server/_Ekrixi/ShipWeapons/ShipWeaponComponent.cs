using Content.Shared.DeviceLinking;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._Ekrixi.ShipWeapons;

/// <summary>
/// Given to guns that are considered ship weapons.
/// </summary>
[RegisterComponent]
public sealed partial class ShipWeaponComponent : Component
{
    /// <summary>
    /// The target angle that this ship weapon will attempt to maneuver toward.
    /// </summary>
    public Angle DesiredAngle = Angle.Zero;
    /// <summary>
    /// The coordinate target that this weapon will attempt to angle toward.
    /// </summary>
    public EntityCoordinates? Target = null;
    /// <summary>
    /// Is this gun automatically firing?
    /// </summary>
    [DataField] public bool AutoFire;

    /// <summary>
    /// The tolerance of the rotation, or at what point is it considered "close enough"
    /// </summary>
    [DataField] public double AngleTolerance = Math.PI;
    /// <summary>
    /// How fast the weapon rotates
    /// </summary>
    [DataField] public double RotationSpeed = Math.PI;
    /// <summary>
    /// How often does the ship weapon fire updates to the computer?
    /// </summary>
    [DataField] public TimeSpan GunneryUpdateFrequency = TimeSpan.FromSeconds(0.5f);

    public TimeSpan NextUpdateTime;

    public TransformComponent? WeaponTransform;

    [DataField]
    public ProtoId<SinkPortPrototype> PortFire = "EkrixiFire";

    [DataField]
    public ProtoId<SinkPortPrototype> PortAim = "EkrixiAim";

    [DataField]
    public ProtoId<SinkPortPrototype> PortAutofire = "EkrixiAutofire";

    [DataField]
    public ProtoId<SourcePortPrototype> SourceData = "EkrixiDataGunnery";
}

using Content.Shared._Ekrixi.ShipWeapons;
using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Server._Ekrixi.ShipWeapons;

[RegisterComponent]
public sealed partial class GunneryComputerComponent : Component
{
    [DataField]
    public ProtoId<SinkPortPrototype> SinkGunnery = "EkrixiDataGunnery";

    [DataField]
    public ProtoId<SourcePortPrototype> SourceFire = "EkrixiFire";

    [DataField]
    public ProtoId<SourcePortPrototype> SourceAim = "EkrixiAim";

    [DataField]
    public ProtoId<SourcePortPrototype> SourceAutofire = "EkrixiAutofire";

    // We don't want this to save because it's recalculated everytime anyway
    [ViewVariables]
    public Dictionary<EntityUid, ShipWeaponData> GunneryTurretData = new ();
}

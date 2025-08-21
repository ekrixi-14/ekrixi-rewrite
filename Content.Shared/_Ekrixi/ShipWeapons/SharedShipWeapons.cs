using System.Numerics;
using Content.Shared.APC;
using Content.Shared.Shuttles.BUIStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Ekrixi.ShipWeapons;

[Serializable, NetSerializable]
public enum GunnerComputerUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class GunnerComputerBoundInterfaceState(
    NavInterfaceState state,
    List<TurretData> turrets,
    List<Vector2> bulletRadarData,
    bool onGrid)
    : BoundUserInterfaceState
{
    public readonly NavInterfaceState State = state;
    public readonly List<TurretData> Turrets = turrets;
    public readonly List<Vector2> BulletRadarData = bulletRadarData;
    public readonly bool OnGrid = onGrid;
}

[Serializable, NetSerializable]
public struct TurretData(int currentAmmo, int ammoCapacity, NetCoordinates coordinates)
{
    public int CurrentAmmo = currentAmmo;
    public int AmmoCapacity = ammoCapacity;
    public NetCoordinates Coordinates = coordinates;
}

[Serializable, NetSerializable]
public sealed class SetTurretRotationToMessage(NetCoordinates coordinates) : BoundUserInterfaceMessage
{
    public readonly NetCoordinates Coordinates = coordinates;
}

[Serializable, NetSerializable]
public sealed class SetTurretAutoFireMessage(bool autoFire) : BoundUserInterfaceMessage
{
    public readonly bool AutoFire = autoFire;
}

[Serializable, NetSerializable]
public sealed class SetTurretTargetCoordinates(NetCoordinates targetCoordinates) : BoundUserInterfaceMessage
{
    public readonly NetCoordinates TargetCoordinates = targetCoordinates;
}

[Serializable, NetSerializable]
public sealed class FireTurretMessage : BoundUserInterfaceMessage;

using System.Numerics;
using Content.Server.DeviceLinking.Components;
using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using Content.Server.DeviceNetwork;
using Content.Server.Shuttles.Systems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Ekrixi.ShipWeapons;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceNetwork;
using Content.Shared.Interaction;
using Content.Shared.Rotatable;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Server._Ekrixi.ShipWeapons;

/// <summary>
/// This handles ship weapons and device linking.
/// </summary>
public sealed class ShipWeaponsSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly DeviceLinkSystem _deviceLinkSystem = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly ShuttleConsoleSystem _console = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly RotateToFaceSystem _rotate = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ShipWeaponComponent, ComponentInit>(OnShipWeaponComponentInit);
        SubscribeLocalEvent<ShipWeaponComponent, ComponentStartup>(OnShipWeaponComponentStartup);
        SubscribeLocalEvent<ShipWeaponComponent, SignalReceivedEvent>(OnShipWeaponSignalReceived);

        SubscribeLocalEvent<GunneryComputerComponent, SignalReceivedEvent>(OnComputerSignalReceived);
        SubscribeLocalEvent<GunneryComputerComponent, ComponentStartup>(OnComputerComponentStartup);
        SubscribeLocalEvent<GunneryComputerComponent, ComponentInit>(OnComputerComponentInit);

        SubscribeLocalEvent<GunneryComputerComponent, SetTurretAutoFireMessage>(OnSetTurretAutoFireMessage);
        SubscribeLocalEvent<GunneryComputerComponent, FireTurretMessage>(OnFireTurretMessage);
        SubscribeLocalEvent<GunneryComputerComponent, SetTurretTargetCoordinatesMessage>(OnSetTurretTargetCoordinates);
    }

    private void OnSetTurretTargetCoordinates(Entity<GunneryComputerComponent> ent, ref SetTurretTargetCoordinatesMessage args)
    {
        var data = new NetworkPayload
        {
            [DeviceNetworkConstants.LogicState] = SignalState.High,
            [ShipWeaponConstants.TargetCoordinate] = EntityManager.GetCoordinates(args.TargetCoordinates),
        };
        _deviceLinkSystem.InvokePort(ent.Owner, ent.Comp.SourceAim, data);
    }

    private void OnFireTurretMessage(Entity<GunneryComputerComponent> ent, ref FireTurretMessage args)
    {
        throw new NotImplementedException();
    }

    private void OnSetTurretAutoFireMessage(Entity<GunneryComputerComponent> ent, ref SetTurretAutoFireMessage args)
    {
        var data = new NetworkPayload
        {
            [DeviceNetworkConstants.LogicState] = SignalState.High,
        };
        _deviceLinkSystem.InvokePort(ent.Owner, ent.Comp.SourceAutofire, data);
    }

    private void OnComputerComponentInit(Entity<GunneryComputerComponent> ent, ref ComponentInit args)
    {
        _deviceLinkSystem.EnsureSinkPorts(ent,
        [
            ent.Comp.SinkGunnery,
        ]);
        _deviceLinkSystem.EnsureSourcePorts(ent,
        [
            ent.Comp.SourceAutofire,
            ent.Comp.SourceFire,
            ent.Comp.SourceAim
        ]);
    }

    private void OnComputerSignalReceived(Entity<GunneryComputerComponent> ent, ref SignalReceivedEvent args)
    {
        if (args.Port != ent.Comp.SinkGunnery || !args.Trigger.HasValue || args.Data == null)
            return;
        args.Data.TryGetValue<int>(ShipWeaponConstants.AmmoCount, out var ammoCount);
        args.Data.TryGetValue<int>(ShipWeaponConstants.MaxAmmoCount, out var ammoCapacity);

        ent.Comp.GunneryTurretData[args.Trigger.Value] = new GunneryTurretData
        {
            AmmoCount = ammoCount,
            MaxAmmoCount = ammoCapacity
        };
    }

    private void OnComputerComponentStartup(Entity<GunneryComputerComponent> ent, ref ComponentStartup args)
    {
        UpdateConsoleState(ent.Owner, ent.Comp);
    }

    private void OnShipWeaponComponentStartup(Entity<ShipWeaponComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.WeaponTransform ??= Transform(ent);
    }

    public void UpdateGunneryData(Entity<ShipWeaponComponent> ent,
        GunComponent? gun = null,
        DeviceLinkSourceComponent? source = null)
    {
        if (!Resolve(ent, ref gun) || !Resolve(ent, ref source))
            return;

        var ev = new GetAmmoCountEvent();
        RaiseLocalEvent(ent, ref ev);

        var port = ent.Comp.SourceData;
        var data = new NetworkPayload
        {
            [DeviceNetworkConstants.LogicState] = SignalState.High,
            [DeviceNetworkConstants.Command] = ShipWeaponConstants.CommandUpdateGunnery,
            [ShipWeaponConstants.AmmoCount] = ev.Count,
            [ShipWeaponConstants.MaxAmmoCount] = ev.Capacity,
        };
        _deviceLinkSystem.InvokePort(ent.Owner, port, data);
    }

    public bool TryFireShipWeapon(Entity<ShipWeaponComponent> ent, GunComponent? gun = null)
    {
        if (!Resolve(ent, ref gun))
            return false;

        if (TryComp<ChamberMagazineAmmoProviderComponent>(ent, out var chamber))
        {
            var ev = new GetAmmoCountEvent();
            RaiseLocalEvent(ent, ref ev);

            if (ev.Count > 0 && chamber.BoltClosed.HasValue && !chamber.BoltClosed.Value)
                _gunSystem.UseChambered(ent, chamber);
        }

        _gunSystem.AttemptShoot(ent, EnsureComp<GunComponent>(ent));
        UpdateGunneryData(ent, gun);
        return true;
    }

    private void OnShipWeaponSignalReceived(Entity<ShipWeaponComponent> ent, ref SignalReceivedEvent args)
    {
        if (args.Port == ent.Comp.PortAutofire)
            ent.Comp.AutoFire = !ent.Comp.AutoFire;
        else if (args.Port == ent.Comp.PortFire)
            TryFireShipWeapon(ent);
        else if (args.Port == ent.Comp.PortAim)
        {
            if (args.Data != null &&
                args.Data.TryGetValue(ShipWeaponConstants.TargetCoordinate, out EntityCoordinates targetCoordinate))
                TryAimShipWeapon(ent, targetCoordinate);
        }
    }

    private void TryAimShipWeapon(Entity<ShipWeaponComponent> ent, EntityCoordinates targetCoordinate)
    {
        var xform = Transform(ent);
        var origin = _transformSystem.ToMapCoordinates(xform.Coordinates).Position;
        var target = _transformSystem.ToMapCoordinates(targetCoordinate).Position;
        var angle = (target - origin).ToWorldAngle();
        ent.Comp.DesiredAngle = angle;
        ent.Comp.Target = targetCoordinate;
    }

    private void OnShipWeaponComponentInit(Entity<ShipWeaponComponent> ent, ref ComponentInit args)
    {
        _containerSystem.EnsureContainer<ContainerSlot>(ent, "gun_magazine");
        _containerSystem.EnsureContainer<ContainerSlot>(ent, "gun_chamber");

        _deviceLinkSystem.EnsureSinkPorts(ent,
        [
            ent.Comp.PortFire,
            ent.Comp.PortAim,
            ent.Comp.PortAutofire,
        ]);
        _deviceLinkSystem.EnsureSourcePorts(ent,
        [
            ent.Comp.SourceData
        ]);
    }

    private void UpdateConsoleState(EntityUid uid, GunneryComputerComponent component)
    {
        if (!_uiSystem.HasUi(uid, GunnerComputerUiKey.Key))
            return;

        var xform = Transform(uid);
        var onGrid = xform.ParentUid == xform.GridUid;

        var docks = _console.GetAllDocks();
        var state = !onGrid ? _console.GetNavState(uid, docks) : _console.GetNavState(uid, docks, xform.Coordinates, xform.LocalRotation);

        _uiSystem.SetUiState(
            uid,
            GunnerComputerUiKey.Key,
            new GunnerComputerBoundInterfaceState(state, new (), new (), onGrid)
        );
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShipWeaponComponent, RotatableComponent>();
        while (query.MoveNext(out var entity, out var component, out _))
        {
            _rotate.TryRotateTo(entity, component.DesiredAngle, frameTime, component.AngleTolerance, component.RotationSpeed);

            if (component.AutoFire)
                TryFireShipWeapon((entity, component));
        }
    }
}

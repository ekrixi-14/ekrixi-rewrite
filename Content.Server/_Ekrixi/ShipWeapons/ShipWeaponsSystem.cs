using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Weapons.Ranged.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Interaction;
using Content.Shared.MouseRotator;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._Ekrixi.ShipWeapons;

/// <summary>
/// This handles...
/// </summary>
public sealed class ShipWeaponsSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _deviceLinkSystem = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly RotateToFaceSystem _rotate = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ShipWeaponComponent, ComponentInit>(ComponentInit);
        SubscribeLocalEvent<ShipWeaponComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<ShipWeaponComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnComponentStartup(Entity<ShipWeaponComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.WeaponTransform ??= Transform(ent);
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
        return true;
    }

    private void OnSignalReceived(Entity<ShipWeaponComponent> ent, ref SignalReceivedEvent args)
    {
        if (args.Port == ent.Comp.PortAutofire)
            ent.Comp.AutoFire = !ent.Comp.AutoFire;
        else if (args.Port == ent.Comp.PortFire)
            TryFireShipWeapon(ent);
        else if (args.Port == ent.Comp.PortAim)
        {

        }
    }

    private void ComponentInit(Entity<ShipWeaponComponent> ent, ref ComponentInit args)
    {
        _containerSystem.EnsureContainer<ContainerSlot>(ent, "gun_magazine");
        _containerSystem.EnsureContainer<ContainerSlot>(ent, "gun_chamber");

        _deviceLinkSystem.EnsureSinkPorts(ent,
        [
            ent.Comp.PortFire,
            ent.Comp.PortAim,
            ent.Comp.PortAutofire,
        ]);
        _deviceLinkSystem.EnsureSourcePorts(ent, [ent.Comp.SourceData]);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShipWeaponComponent, MouseRotatorComponent>();
        while (query.MoveNext(out var entity, out var component, out var mouseRot))
        {
            if (mouseRot.GoalRotation.HasValue)
                component.DesiredAngle = mouseRot.GoalRotation.Value;

            _rotate.TryRotateTo(entity, component.DesiredAngle, frameTime, mouseRot.AngleTolerance, mouseRot.RotationSpeed, component.WeaponTransform);

            if (component.AutoFire)
                TryFireShipWeapon((entity, component));
        }
    }
}

using Content.Shared._Ekrixi.ShipWeapons;
using JetBrains.Annotations;

namespace Content.Client._Ekrixi.ShipWeapons;

[UsedImplicitly]
public sealed class GunnerComputerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private GunnerComputerWindow? _window;
    private readonly IEntityManager _entityManager;

    public GunnerComputerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _entityManager = IoCManager.Resolve<IEntityManager>();
    }

    protected override void Open()
    {
        base.Open();
        _window = new GunnerComputerWindow();
        _window.OpenCentered();
        _window.OnClose += OnClose;
        _window.OnRadarClick += args =>
        {
            var msg = new SetTurretTargetCoordinatesMessage(_entityManager.GetNetCoordinates(args));
            SendMessage(msg);
        };
        _window.OnFireClick += () =>
        {
            var msg = new FireTurretMessage();
            SendMessage(msg);
        };
        _window.OnEjectClick += () =>
        {
            // var msg = new PerformActionWeaponSendMessage(ShipWeaponAction.Eject);
            // SendMessage(msg);
        };
        _window.OnAutofireClick += () =>
        {
            var msg = new SetTurretAutoFireMessage(true);
            SendMessage(msg);
        };
        _window.OnChamberClick += () =>
        {
            // var msg = new PerformActionWeaponSendMessage(ShipWeaponAction.Chamber);
            // SendMessage(msg);
        };
    }

    private void OnClose()
    {
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _window?.Dispose();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not GunnerComputerBoundInterfaceState cState)
            return;

        _window?.SetMatrix(_entityManager.GetCoordinates(cState.State.Coordinates), cState.State.Angle);
        _window?.UpdateState(cState);
    }
}

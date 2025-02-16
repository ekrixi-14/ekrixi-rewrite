using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Shared._Ekrixi.Splatter;

/// <summary>
/// This is used for making things start big and splat on the ground.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class SplatterComponent : Component
{
    /// <summary>
    ///     The <see cref="IGameTiming.CurTime"/> timestamp at which this entity started falling.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan? TimeToFall = TimeSpan.FromSeconds(0.35f);

    /// <summary>
    ///     The <see cref="IGameTiming.CurTime"/> timestamp at which this entity started falling.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan? StartFallTime;

    /// <summary>
    ///     The rotation of this splatter.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Angle InitialRotation;

    /// <summary>
    ///     The target rotation of this splatter.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Angle TargetRotation;

    /// <summary>
    ///     The peak height of this splatter.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float PeakHeight = 1.3f;

    /// <summary>
    ///     Compared to <see cref="IGameTiming.CurTime"/> to stop making this entity fall, if any.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan? StopFallTime;

    /// <summary>
    ///     Is this entity still falling?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool Falling;

    /// <summary>
    ///     Used to restore state after the throwing scale animation is finished.
    /// </summary>
    [DataField]
    public Vector2? OriginalScale = null;
}

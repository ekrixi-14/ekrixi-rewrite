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
    public TimeSpan? StartFallTime;

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

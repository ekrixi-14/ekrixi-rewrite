namespace Content.Shared._Ekrixi.Splatter;

/// <summary>
/// This is used for making an entity spawn with SplatterComponent.
/// </summary>
[RegisterComponent]
public sealed partial class SplatterOnInitComponent : Component
{
    /// <summary>
    /// How much time does it take for this to splat?
    /// </summary>
    public TimeSpan TimeToFall = TimeSpan.FromSeconds(1f);
}

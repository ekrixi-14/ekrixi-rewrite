using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Ekrixi.Splatter;

/// <summary>
/// This handles...
/// </summary>
public sealed class SplatterOnInitSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SplatterOnInitComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<SplatterOnInitComponent> ent, ref ComponentInit args)
    {
        var comp = EnsureComp<SplatterComponent>(ent);
        comp.StartFallTime = _timing.CurTime;
        comp.StopFallTime = _timing.CurTime + ent.Comp.TimeToFall;

        comp.InitialRotation = _random.NextAngle();
        comp.TargetRotation = comp.InitialRotation + _random.NextAngle() / 8;
        comp.PeakHeight = _random.NextFloat(1.22f, 1.47f);
    }
}

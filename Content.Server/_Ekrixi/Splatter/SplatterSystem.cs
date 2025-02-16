using System.Numerics;
using Content.Server.Body.Components;
using Content.Shared._Ekrixi.Splatter;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Ekrixi.Splatter;

/// <summary>
/// This handles splatting and splashing blood when hit.
/// </summary>
public sealed class SplatterSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly PhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SplatterableComponent, DamageChangedEvent>(OnDamage);
    }

    public Vector2 RandomDirection()
    {
        return _random.NextVector2().Normalized();
    }

    public void SplatAt(EntityUid uid)
    {
        SplatAt(uid, RandomDirection() * _random.NextFloat(4f, 12f));
    }

    public void SplatAt(EntityUid uid, Vector2 direction)
    {
        SplatAt(Transform(uid).Coordinates, direction);
    }

    public void SplatAt(EntityCoordinates coordinates, Vector2 direction)
    {
        var puddle = EntityManager.Spawn("EkrixiSplatter");
        var comp = EnsureComp<SplatterComponent>(puddle);

        comp.StartFallTime = _timing.CurTime;
        comp.StopFallTime = _timing.CurTime + comp.TimeToFall; // shouldn't be hardcoded but blehhh

        comp.InitialRotation = _random.NextAngle();
        comp.TargetRotation = comp.InitialRotation + _random.NextAngle() / 8;
        comp.PeakHeight = _random.NextFloat(1.22f, 1.47f);

        Dirty(puddle, comp);

        _transformSystem.SetCoordinates(puddle, coordinates);

        var physics = EnsureComp<PhysicsComponent>(puddle);
        _physicsSystem.ApplyLinearImpulse(puddle, direction, body: physics);
    }

    private void OnDamage(Entity<SplatterableComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta is null || !args.DamageIncreased)
        {
            return;
        }

        if (TryComp<BloodstreamComponent>(ent.Owner, out var bloodstream) &&
            _solutionContainerSystem.ResolveSolution(ent.Owner,
                bloodstream.BloodSolutionName,
                ref bloodstream.BloodSolution,
                out _))
        {
            if (bloodstream.BleedAmount <= 0)
                return;
        }
        else
        {
            return;
        }
    }
}

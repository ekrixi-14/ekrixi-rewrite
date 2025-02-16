using Content.Server.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

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

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<SplatterableComponent, DamageChangedEvent>(OnDamage);
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

        var puddle = EntityManager.Spawn("EkrixiSplatter");
        _transformSystem.SetCoordinates(puddle, Transform(ent.Owner).Coordinates);

        var physics = EnsureComp<PhysicsComponent>(puddle);

        var impulseVector = _random.NextVector2().Normalized() * _random.NextFloat(2f, 6f) * _random.NextFloat(1f, MathF.Min(1.25f, MathF.Max(bloodstream.BleedAmount, 2f)));
        _physicsSystem.ApplyLinearImpulse(puddle, impulseVector, body: physics);
    }
}

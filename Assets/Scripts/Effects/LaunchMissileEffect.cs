using UnityEngine;

public enum MissileTargetMode
{
    Target,           // Follow a target (moving or static).
    TargetOrigin,     // Go to where target was at time of launch.
    FixedPosition     // Go to a specific Vector3.
}
[CreateAssetMenu(menuName = "Effects/LaunchMissile")]
public class LaunchMissileEffect : Effect
{
    public MissileTargetMode TargetMode;
    public Effect OnImpact;
    public GameObject MissilePrefab; // assign in inspector

    public override void Apply(EffectContext context)
    {
        if (MissilePrefab == null)
        {
            Debug.LogWarning("MissilePrefab not assigned on LaunchMissileEffect.");
            return;
        }

        Vector3 startPosition = context.Source != null ? context.Source.transform.position : Vector3.zero;
        Vector3 destination;
        GameObject target = null;
        bool trackingTarget = false;

        switch (TargetMode)
        {
            case MissileTargetMode.Target:
                if (context.Target == null) return;
                destination = context.Target.transform.position;
                target = context.Target;
                trackingTarget = true;
                break;

            case MissileTargetMode.TargetOrigin:
                if (context.Target == null) return;
                destination = context.Position ?? context.Target.transform.position;
                trackingTarget = false;
                break;

            case MissileTargetMode.FixedPosition:
                if (!context.Position.HasValue) return;
                destination = context.Position.Value;
                trackingTarget = false;
                break;

            default:
                return;
        }

        GameObject missileObject = Instantiate(MissilePrefab, startPosition, Quaternion.identity);
        MissileEffect missile = missileObject.GetComponent<MissileEffect>();
        if (missile != null)
        {
            missile.Initialize(target, destination, trackingTarget, OnImpact, context);
        }
        else
        {
            Debug.LogError("MissilePrefab is missing a MissileEffect component.");
        }
    }
}

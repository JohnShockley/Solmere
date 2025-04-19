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

    public override void Apply(EffectContext context)
    {
        Vector3 destination;

        switch (TargetMode)
        {
            case MissileTargetMode.Target:
                if (context.Target == null) return;
                destination = context.Target.transform.position;
                break;

            case MissileTargetMode.TargetOrigin:
                if (context.Target == null) return;
                destination = context.Position ?? context.Target.transform.position;
                break;

            case MissileTargetMode.FixedPosition:
                if (!context.Position.HasValue) return;
                destination = context.Position.Value;
                break;

            default: return;
        }

        // spawn your missile prefab and tell it: "fly to destination, apply OnImpact when you get there"
    }
}

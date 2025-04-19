using UnityEngine;

public class EffectContext
{
    public GameObject Target;       // For target-based effects.
    public Vector3? Position;       // For position-based effects.

    public GameObject Instigator;   // (Optional) Who cast the effect.
 

    public EffectContext(GameObject target = null, Vector3? position = null, GameObject instigator = null)
    {
        Target = target;
        Position = position;
        Instigator = instigator;
        
    }
}
using UnityEngine;

public class DamageEvent
{
    public float Amount;
    public GameObject Source;
    public GameObject Target;
    public bool Cancelled;

    public DamageEvent(float amount, GameObject source, GameObject target)
    {
        Amount = amount;
        Source = source;
        Target = target;
    }
}

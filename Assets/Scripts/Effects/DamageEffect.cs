using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Damage")]
public class DamageEffect : Effect
{
    public float amount;

    public override void Apply(EffectContext context)
    {
        if (context.Target == null) {
            return;
        }


        var receiver = context.Target.GetComponent<HealthComponent>();
        if (receiver != null)
            receiver.ReceiveDamage(amount);
    }

    
}

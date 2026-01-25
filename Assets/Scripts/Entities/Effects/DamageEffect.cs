using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Damage")]
public class DamageEffect : Effect
{
    public float Amount;
    public StatType Stat;

    public DamageEffect(float amount, StatType stat)
    {
        Amount = amount;
        Stat = stat;
    }

    public override void Execute(EffectContext context)
    {
        if (context.Target.TryGetComponent<HealthComponent>(out var health))
        {
            float damage = context.StatSnapshot[Stat] + Amount;
            health.ReceiveDamage(context, damage);
            Debug.Log($"Dealt {damage} {Stat} damage to {context.Target.name}");
        }
        //damage is a terminal effect, so we don't call for each children.Execute(context);
    }
}
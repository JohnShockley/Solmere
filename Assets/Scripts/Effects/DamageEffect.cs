using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Damage")]
public class DamageEffect : Effect
{
    public DamageType damageType;
    public AttackType attackType;
    public bool flags;
    public float BaseDamage;
    public List<string> ModifyingProperties;
    public List<float> PropertyModifiers;
    public float overallModifier;

    private float damage;
    private List<Behavior> modifyingBehaviors;

    public override void Apply(EffectContext context)
    {
        if (context.Target == null)
        {
            return;
        }

        if (context.Target.TryGetComponent<EffectReceiverComponent>(out var targetEffectReceiver))
        {
            damage = BaseDamage;

            for (int i = 0; i < ModifyingProperties.Count; i++)
            {
                damage += context.StatSnapshot.Get(ModifyingProperties[i]) * PropertyModifiers[i];
            }

            // foreach (Behavior b in modifyingBehaviors) {
            //     damage *= b;
            // }

            targetEffectReceiver.ReceiveDamageEffect(this);
        }
    }

    public float getDamage()
    {
        return damage;
    }

}

public enum DamageType
{
    Physical,
    Magical,
    True // Ignores all mitigation
}

public enum AttackType
{
    Melee,
    Projectile,
    AOE,
}

[SerializeField]
public enum AttackOutcome
{
    Pending,        // Not resolved yet
    Missed,         // Attacker failed AccuracyRoll
    Dodged,         // Defender Succeeded DodgeRoll

    Parried,        // Defender Succeeded ParryRoll
    Blocked,        // Defender Succeeded BlockRoll (Physical)
    Deflected,      // Defender Succeeded BlockRoll (Magical)

    Riposted,       // Defender Critically Succeeded ParryRoll (Physical Melee)
    Reflected,      // Defender Critically Succeeded BlockRoll (Magical)

    Breakthrough,   // Defender Failed ParryRoll
    Staggered,      // Defender Failed BlockRoll (Physical)

    Absorbed,       // Defender Absorbed Attack

    Resisted,       // Defender Armor Succeeded Resist
    Pierced,        // Defender Armor Failed Resist
    Hit,            // Normal damage applied
    Fatal           // Killed target
}
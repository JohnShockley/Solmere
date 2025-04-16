using System;
using UnityEngine;

public struct DamageEvent
{
    public GameObject Source;    // Who caused the damage (attacker, trap, environment, etc)
    public GameObject Target;    // Who is being damaged


    public float AttackPower;    // Attack's strength (used for pierce, stagger, etc)
    public DamageType _DamageType;      // Physical, Magic, True
    public AttackType _AttackType;      // Melee, Projectile, AOE

    public bool IsCritical;      // Was this a critical hit?

    public AttackFlags Flags;

    public DamageEvent(GameObject source, GameObject target,  float attackPower, DamageType damageType, AttackType attackType, AttackFlags flags, bool isCritical = false)
    {
        Source = source;
        Target = target;
        AttackPower = attackPower;
        _DamageType = damageType;
        _AttackType = attackType;
        Flags = flags;
        IsCritical = isCritical;
       
    }
}
[Flags]
public enum AttackFlags
{
    None = 0,
    CannotMiss = 1 << 0,
    CannotBeDodged = 1 << 1,
    CannotBeDefended = 1 << 2,
    CannotBeResisted = 1 << 3,
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

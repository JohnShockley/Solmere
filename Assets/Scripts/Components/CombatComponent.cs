using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(PropertyRegistryComponent))]
[RequireComponent(typeof(TargetingComponent))]
public class CombatComponent : MonoBehaviour
{
    private System.Random random = new System.Random();
    [SerializeField] private Property criticalChance;
    [SerializeField] private Property criticalPower;

    [SerializeField] private Property parryChance;
    [SerializeField] private Property parryPower;

    [SerializeField] private Property blockChance;
    [SerializeField] private Property blockPower;

    [SerializeField] private Property deflectChance;
    [SerializeField] private Property deflectPower;

    [SerializeField] private Property hitChance;
    [SerializeField] private Property hitPower;
    PropertyRegistryComponent propertyRegistry;
    TargetingComponent targetingComponent;

    private List<(DamageEvent damageEvent, AttackOutcome attackOutcome)> damageReceivedLog;
    public List<(DamageEvent damageEvent, AttackOutcome attackOutcome)> damageDoneLog;

    //[SerializeField] private ResourceType resource;
    void Awake()
    {
        damageDoneLog = new List<(DamageEvent, AttackOutcome)>();
        HealthComponent hc = GetComponent<HealthComponent>();
        if (hc)
        {
            damageReceivedLog = hc.damageReceivedLog;
        }


        propertyRegistry = GetComponent<PropertyRegistryComponent>();
        targetingComponent = GetComponent<TargetingComponent>();

        propertyRegistry.Register("criticalChance", criticalChance);
        propertyRegistry.Register("criticalPower", criticalPower);

        propertyRegistry.Register("parryChance", parryChance);
        propertyRegistry.Register("parryPower", parryPower);

        propertyRegistry.Register("blockChance", blockChance);
        propertyRegistry.Register("blockPower", blockPower);

        propertyRegistry.Register("deflectChance", deflectChance);
        propertyRegistry.Register("deflectPower", deflectPower);

        propertyRegistry.Register("hitChance", hitChance);
        propertyRegistry.Register("hitPower", hitPower);
    }
    float nextDamageTime = 0f;

    void Update()
    {
        if (targetingComponent.Target && Time.time >= nextDamageTime)
        {

            GameObject target = targetingComponent.Target;
            DamageEvent damageEvent = new DamageEvent(
     source: this.gameObject,              // Who's dealing the damage (yourself)
     target: target,                       // Who's being damaged (your target)
     attackPower: 50f,                     // Attack power — adjust for your test
     damageType: DamageType.Physical,      // Physical / Magical / True
     attackType: AttackType.Melee,         // Melee / Projectile / AOE
     flags: AttackFlags.None,              // Any special flags
     isCritical: UnityEngine.Random.value < 0.25f // 25% chance to be critical for fun
 );
            DoDamage(damageEvent, target);

            nextDamageTime = Time.time + 10f; // 2 second delay
        }
    }

    public void DoDamage(DamageEvent damageEvent, GameObject target)
    {
        if (!DidNotMiss(damageEvent))
        {
            return;
        }
        target.GetComponent<HealthComponent>().ReceiveDamage(damageEvent);
    }

    private bool DidNotMiss(DamageEvent damageEvent)
    {

        float amount = (float)random.NextDouble();
        bool hit = amount < hitChance.Value;
        if (!hit)
        {
            UpdateLog(damageEvent, AttackOutcome.Missed);
        }
        return hit;
    }

    public bool DidDefend(ref DamageEvent damageEvent)
    {
        if (damageEvent.Flags.HasFlag(AttackFlags.CannotBeDefended))
        {
            return false;
        }
        if (damageEvent._DamageType == DamageType.Physical && damageEvent._AttackType == AttackType.Melee)
        {
            bool didParry = DidParry(ref damageEvent);

            if (didParry)
            {
                return true;
            }
        }
        if (damageEvent._DamageType == DamageType.Physical)
        {
            bool didBlock = DidBlock(ref damageEvent);
            if (didBlock)
            {
                return true;
            }
        }
        if (damageEvent._DamageType == DamageType.Magical && damageEvent._AttackType == AttackType.Projectile)
        {
            bool didDeflect = DidDeflect(ref damageEvent);
            if (didDeflect)
            {
                return true;
            }
        }
        return false;
    }

    private bool DidParry(ref DamageEvent damageEvent)
    {
        bool parried;

        float amount = (float)random.NextDouble();
        parried = amount < parryChance.Value;

        if (parried)
        {
            if (parryPower.Value >= damageEvent.AttackPower)
            {
                if (DidCrit(ref damageEvent))
                {
                    UpdateLog(damageEvent, AttackOutcome.Riposted);
                }
                else
                {
                    UpdateLog(damageEvent, AttackOutcome.Parried);
                }
                return true;
            }
            else
            {
                UpdateLog(damageEvent, AttackOutcome.Breakthrough);
                damageEvent.AttackPower -= parryPower.Value;
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    private bool DidBlock(ref DamageEvent damageEvent)
    {
        bool blocked;

        float amount = (float)random.NextDouble();
        blocked = amount < blockChance.Value;
        if (blocked)
        {
            if (blockPower.Value >= damageEvent.AttackPower)
            {
                if (DidCrit(ref damageEvent))
                {
                    UpdateLog(damageEvent, AttackOutcome.Riposted);
                }
                else
                {
                    UpdateLog(damageEvent, AttackOutcome.Blocked);
                }
                return true;
            }
            else
            {
                UpdateLog(damageEvent, AttackOutcome.Staggered);
                damageEvent.AttackPower -= blockPower.Value;
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    private bool DidDeflect(ref DamageEvent damageEvent)
    {
        bool deflected;

        float amount = (float)random.NextDouble();
        deflected = amount < deflectChance.Value;
        if (deflected)
        {
            if (deflectPower.Value >= damageEvent.AttackPower)
            {
                if (DidCrit(ref damageEvent))
                {
                    UpdateLog(damageEvent, AttackOutcome.Reflected);
                }
                else
                {
                    UpdateLog(damageEvent, AttackOutcome.Deflected);
                }
                return true;
            }
            else
            {
                UpdateLog(damageEvent, AttackOutcome.Staggered);
                damageEvent.AttackPower -= deflectPower.Value;
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private bool DidCrit(ref DamageEvent damageEvent)
    {
        float amount = (float)random.NextDouble();

        bool critted = amount < damageEvent.Target.GetComponent<PropertyRegistryComponent>().Get("criticalChance").Value;
        return critted;
    }

    private void UpdateLog(DamageEvent damageEvent, AttackOutcome attackOutcome)
    {
        damageDoneLog.Add((damageEvent, attackOutcome));
        damageReceivedLog?.Add((damageEvent, attackOutcome));

        Debug.Log("Your attack was " + attackOutcome);
    }
}

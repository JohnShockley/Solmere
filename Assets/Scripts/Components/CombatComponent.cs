using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.UIElements;
using static DamageEvent;


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
   
    [SerializeField]
    public List<DamageLogEntry> damageDoneLog;

    //[SerializeField] private ResourceType resource;
    void Awake()
    {
        damageDoneLog = new List<DamageLogEntry>();
     


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
     source: gameObject,              // Who's dealing the damage (yourself)
     target: target,                       // Who's being damaged (your target)
     attackPower: 50f,                     // Attack power — adjust for your test
     damageType: DamageType.Physical,      // Physical / Magical / True
     attackType: AttackType.Melee,         // Melee / Projectile / AOE
     flags: AttackFlags.None,              // Any special flags
     hitChance: hitChance.Value,
     isCritical: false
 );
            DoDamage(damageEvent, target);

            nextDamageTime = Time.time + 10f; // 2 second delay
        }
    }

    public void DoDamage(DamageEvent damageEvent, GameObject target)
    {
        if (DidCrit())
        {
            damageEvent.AttackPower *= 2;
            damageEvent.IsCritical = true;
        }
        target.GetComponent<HealthComponent>().ReceiveDamage(1);
    }

    private bool DidCrit()
    {
        float amount = (float)random.NextDouble();

        bool critted = amount < criticalChance.Value;
        return critted;
    }
}

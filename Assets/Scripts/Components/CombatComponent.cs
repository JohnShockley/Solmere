using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(PropertyRegistryComponent))]
[RequireComponent(typeof(TargetingComponent))]
public class CombatComponent : MonoBehaviour
{
    private System.Random random = new System.Random();


    [SerializeField] private Property intellect;
    // [SerializeField] private Property criticalChance;
    // [SerializeField] private Property criticalPower;

    // [SerializeField] private Property parryChance;
    // [SerializeField] private Property parryPower;

    // [SerializeField] private Property blockChance;
    // [SerializeField] private Property blockPower;

    // [SerializeField] private Property deflectChance;
    // [SerializeField] private Property deflectPower;

    // [SerializeField] private Property hitChance;
    // [SerializeField] private Property hitPower;
    PropertyRegistryComponent propertyRegistry;
    TargetingComponent targetingComponent;


    public Effect effect;

    //[SerializeField] private ResourceType resource;
    void Awake()
    {



        propertyRegistry = GetComponent<PropertyRegistryComponent>();
        targetingComponent = GetComponent<TargetingComponent>();

        propertyRegistry.Register("Intellect", intellect);
        // propertyRegistry.Register("criticalChance", criticalChance);
        // propertyRegistry.Register("criticalPower", criticalPower);

        // propertyRegistry.Register("parryChance", parryChance);
        // propertyRegistry.Register("parryPower", parryPower);

        // propertyRegistry.Register("blockChance", blockChance);
        // propertyRegistry.Register("blockPower", blockPower);

        // propertyRegistry.Register("deflectChance", deflectChance);
        // propertyRegistry.Register("deflectPower", deflectPower);

        // propertyRegistry.Register("hitChance", hitChance);
        // propertyRegistry.Register("hitPower", hitPower);
    }
    float nextDamageTime = 0f;

    void Update()
    {
        if (targetingComponent.Target && Time.time >= nextDamageTime)
        {

            GameObject target = targetingComponent.Target;

            DoDamage(target);

            nextDamageTime = Time.time + 10f; // 2 second delay
        }
    }

    public void DoDamage(GameObject target)
    {
        EffectContext ec = new EffectContext(gameObject, this.GetComponent<PropertyRegistryComponent>().SnapshotAll(), target, null);

        effect.Apply(ec);
    }

    // private bool DidCrit()
    // {
    //     float amount = (float)random.NextDouble();

    //     bool critted = amount < criticalChance.Value;
    //     return critted;
    // }
}

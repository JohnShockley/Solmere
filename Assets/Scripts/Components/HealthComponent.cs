using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(PropertyRegistryComponent))]
public class HealthComponent : MonoBehaviour
{
    private System.Random random = new System.Random();
    [SerializeField] private Resource health;
    [SerializeField] private Property healthRegen;

    [SerializeField] private Property dodgeChance;
    [SerializeField] private Property dodgePower;

    [SerializeField] private Resource armor;

    private PropertyRegistryComponent propertyRegistry;

    void Awake()
    {


        propertyRegistry = GetComponent<PropertyRegistryComponent>();

        propertyRegistry.Register("maxHealth", health.GetMax());
        propertyRegistry.Register("minHealth", health.GetMin());
        propertyRegistry.Register("healthRegen", healthRegen);

        propertyRegistry.Register("dodgeChance", dodgeChance);
        propertyRegistry.Register("dodgePower", dodgePower);

        propertyRegistry.Register("maxArmor", armor.GetMax());
        propertyRegistry.Register("minArmor", armor.GetMin());
    }
    void Update()
    {
        health.Add(healthRegen.Value * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Get the maxHealth property from the registry
            Property maxHealth = propertyRegistry.Get("maxHealth");


            // Add a modifier to maxHealth when 'E' is pressed
            // Example: Adding 10% extra health
            PropertyModifier healthModifier = new PropertyModifier(10, PropertyModType.Flat);

            maxHealth.AddModifier(healthModifier);
            Debug.Log("Max Health increased by 10");

        }
    }

    public bool ReceiveDamage(float damage)
    {
        health.Add(-damage);
        return CheckDeath();
    }

    private bool CheckDeath()
    {
        bool didDie = health.GetCurrent() == 0;
        if (didDie)
        {
            OnDeath();
        }
        return didDie;
    }
    private void OnDeath()
    {
        Debug.Log($"{health} depleted!");
    }
}

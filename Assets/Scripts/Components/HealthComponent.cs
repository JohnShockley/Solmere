using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(PropertyRegistryComponent))]
public class HealthComponent : MonoBehaviour
{
    [SerializeField] private Resource health;
    [SerializeField] private Property healthRegen;
    PropertyRegistryComponent propertyRegistry;

    //[SerializeField] private ResourceType resource;
    void Awake()
    {
        propertyRegistry = GetComponent<PropertyRegistryComponent>();
        propertyRegistry.Register("maxHealth", health.GetMax());
        propertyRegistry.Register("minHealth", health.GetMin());
        propertyRegistry.Register("healthRegen", healthRegen);
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

    public void TakeDamage(float damage)
    {
        health.Add(-damage);
        CheckDeath();
    }

    private void CheckDeath()
    {
        if (health.GetCurrent() == 0)
        {
            OnDeath();
        }
    }
    private void OnDeath()
    {
        Debug.Log($"{health} depleted!");
    }
}

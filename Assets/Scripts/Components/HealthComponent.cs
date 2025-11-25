using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(EntityComponent))]
public class HealthComponent : MonoBehaviour
{
    private StatType healthRegen = StatType.HealthRegen;
    private StatType healthMin = StatType.HealthMin;
    private StatType healthMax = StatType.HealthMax;
    private ResourceType healthResource = ResourceType.Health;
    private EntityComponent entityComponent;


    void Start()
    {
        entityComponent = GetComponent<EntityComponent>();
        entityComponent.resourceManager.SubscribeInt(healthResource, (value) =>
        {
            Debug.Log($"{healthResource} changed to: {value}");
        });
    }

    public void ReceiveDamage(float damage)
    {
        entityComponent.resourceManager.SpendUpTo(healthResource, 0, damage);
        //maybe check health? But before we 'take' the damage, need to call OnDamageTaken events

    }


    private void OnDeath()
    {
        //call a chain of OnDeath events
    }
}

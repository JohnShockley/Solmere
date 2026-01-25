using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(EntityComponent))]
public class HealthComponent : MonoBehaviour
{
    private StatType healthRegen = StatType.HealthRegen;
    private StatType healthMax = StatType.HealthMax;
    private float _healthResource;
    private ResourceType healthResource = ResourceType.Health;
    private EntityComponent entityComponent;
    void Awake()
    {
        entityComponent = GetComponent<EntityComponent>();

    }

    void Start()
    {
        _healthResource = entityComponent.resourceManager.GetValue(healthResource);
        entityComponent.resourceManager.SubscribeInt(healthResource, (value) =>
        {
            _healthResource = value;
        });
    }

    public void ReceiveDamage(EffectContext damageContext, float amount)
    {
        var e = new DamageEvent(amount, damageContext.Source, gameObject);

        // PHASE 1 — Pre-damage (can cancel)
        entityComponent.Events.RaisePreDamageMitigation(e);
        if (e.Cancelled) return;

        // PHASE 2 — Modify damage (armor, resistances, modifiers)
        entityComponent.Events.RaiseDamageMitigation(e);

        // PHASE 3 — Apply damage & notify
        ApplyDamage(amount);
        entityComponent.Events.RaisePostDamageMitigation(e);
        entityComponent.Events.RaiseDamageTaken(e);
    }

    public void ReceiveHeal(EffectContext healingContext)
    {
        //entityComponent.resourceManager.ChangeCurrentValue(healthResource, healing);
    }

    private void ApplyDamage(float damage)
    {
        entityComponent.resourceManager.ChangeCurrentValue(healthResource, -damage);



        if (_healthResource <= 0)
            entityComponent.Events.RaiseDeath();
    }
    private void ApplyHealth()
    {

    }


}

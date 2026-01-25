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
    private EntityComponent entity;
    void Awake()
    {
        entity = GetComponent<EntityComponent>();

    }

    void Start()
    {
        _healthResource = entity.resourceManager.GetValue(healthResource);
        entity.resourceManager.SubscribeInt(healthResource, (value) =>
        {
            _healthResource = value;
        });
    }

    public void ReceiveDamage(EffectContext damageContext, float amount)
    {
        var e = new DamageEvent(amount, damageContext.Source, gameObject);

        // PHASE 1 — Pre-damage (can cancel)
        entity.Events.RaisePreDamageMitigation(e);
        if (e.Cancelled) return;

        // PHASE 2 — Modify damage (armor, resistances, modifiers)
        entity.Events.RaiseDamageMitigation(e);

        // PHASE 3 — Apply damage & notify
        ApplyDamage(amount);
        entity.Events.RaisePostDamageMitigation(e);
        entity.Events.RaiseDamageTaken(e);
    }

    public void ReceiveHeal(EffectContext healingContext)
    {
        //entityComponent.resourceManager.ChangeCurrentValue(healthResource, healing);
    }

    private void ApplyDamage(float damage)
    {
        entity.resourceManager.ChangeCurrentValue(healthResource, -damage);



        if (_healthResource <= 0)
            entity.Events.RaiseDeath();
    }
    private void ApplyHealth()
    {

    }


}

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

    private CombatComponent combatComponent;
    private PropertyRegistryComponent propertyRegistry;

    public List<(DamageEvent damageEvent, AttackOutcome attackOutcome)> damageReceivedLog;
    private List<(DamageEvent damageEvent, AttackOutcome attackOutcome)> damageDoneLog;

    void Awake()
    {

        damageReceivedLog = new List<(DamageEvent, AttackOutcome)>();
        CombatComponent cc = GetComponent<CombatComponent>();
        if (cc)
        {
            damageDoneLog = cc.damageDoneLog;
        }

        combatComponent = GetComponent<CombatComponent>();
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

    public void ReceiveDamage(DamageEvent damageEvent)
    {
        if (DidDodge(ref damageEvent))
        {
            return;
        }
        if (combatComponent && combatComponent.DidDefend(ref damageEvent))
        {
            return;
        }
        if (DidResist(ref damageEvent))
        {
            return;
        }
        UpdateLog(damageEvent, AttackOutcome.Hit);

        health.Add(-damageEvent.AttackPower);
        CheckDeath(ref damageEvent);
    }

    private bool DidDodge(ref DamageEvent damageEvent)
    {
        if (damageEvent.Flags.HasFlag(AttackFlags.CannotBeDodged))
        {
            return false;
        }

        float amount = (float)random.NextDouble();

        bool dodged = amount < dodgeChance.Value;
        if (dodged)
        {
            UpdateLog(damageEvent, AttackOutcome.Dodged);
        }
        return dodged;
    }
    private bool DidResist(ref DamageEvent damageEvent)
    {
        if (damageEvent.Flags.HasFlag(AttackFlags.CannotBeResisted))
        {
            return false;
        }
        if (armor.GetCurrent() >= damageEvent.AttackPower)
        {
            UpdateLog(damageEvent, AttackOutcome.Resisted);
            armor.Add(-.25f * damageEvent.AttackPower);
            return true;
        }
        else
        {
            UpdateLog(damageEvent, AttackOutcome.Pierced);
            damageEvent.AttackPower -= armor.GetCurrent();
            armor.Add(-.5f * damageEvent.AttackPower);
            return false;
        }
    }
    private void CheckDeath(ref DamageEvent damageEvent)
    {
        if (health.GetCurrent() == 0)
        {
            UpdateLog(damageEvent, AttackOutcome.Fatal);
            OnDeath();
        }
    }
    private void OnDeath()
    {
        Debug.Log($"{health} depleted!");
    }

    private void UpdateLog(DamageEvent damageEvent, AttackOutcome attackOutcome)
    {
        damageReceivedLog.Add((damageEvent, attackOutcome));
        damageDoneLog?.Add((damageEvent, attackOutcome));


        Debug.Log("Your attack was " + attackOutcome);
    }
}

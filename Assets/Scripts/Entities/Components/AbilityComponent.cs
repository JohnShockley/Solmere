using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityComponent))]
public class AbilityComponent : MonoBehaviour
{

    public List<Ability> Abilities;
    private Dictionary<Ability, AbilityContext> contexts = new();
    private EntityComponent entity;
    void Awake()
    {
        entity = GetComponent<EntityComponent>();
        foreach (var ability in Abilities)
        {
            contexts[ability] = new AbilityContext(ability, gameObject);
        }
    }
    public void CastAbility(Ability ability)
    {

        var abilityContext = contexts[ability];
        // Set up effect context
        abilityContext.EffectContext = new EffectContext(
            abilityContext.Caster,
            entity.statManager.Snapshot(),
            entity.CurrentTarget,
            null
        );

        // Cast the ability
        ability.Cast(abilityContext);
    }
}
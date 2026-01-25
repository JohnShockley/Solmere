using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string Name = "Unnamed Spell";
    public string Description = "Empty Spell Description.";

    public AbilityType abilityType;
    public AbilityTargetType abilityTargetType;
    [Min(0)]
    public float cooldown;
    public Effect startingEffect;
    private EffectContext effectContext;
    private float lastCastTime;

    public bool CanCast()
    {
        return Time.time >= lastCastTime + cooldown;
    }

    public void Cast()
    {
        effectContext = new();
        effectContext.Source = null; //TODO: set source to caster

        lastCastTime = Time.time;
        // EffectContext ec = new EffectContext(gameObject, this.GetComponent<PropertyRegistryComponent>().SnapshotAll(), target, null); 

        if (abilityTargetType == AbilityTargetType.Targeted)
        {
            
        }
        if (abilityTargetType == AbilityTargetType.Point)
        {
            //NYI
        }


        startingEffect.Execute(effectContext);
    }





}

public enum AbilityType
{
    Casted,
    Passive //NYI
}

public enum AbilityTargetType
{
    Targeted,
    Point
}
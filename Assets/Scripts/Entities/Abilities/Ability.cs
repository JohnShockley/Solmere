using UnityEngine;
[CreateAssetMenu(menuName = "Ability")]
public class Ability : ScriptableObject
{
    public string Name = "Unnamed Spell";
    public string Description = "Empty Spell Description.";

    public AbilityType AbilityType;
    public AbilityTargetType AbilityTargetType;
    [Min(0)]
    public float Cooldown;
    public Effect StartingEffect;


    public void Cast(AbilityContext abilityContext)
    {
        if (!abilityContext.CanCast())
        {
            Debug.Log($"Ability {Name} is on cooldown.");
            return; //eventually return appropriate error code
        }
        if (AbilityTargetType == AbilityTargetType.Targeted && abilityContext.EffectContext.Target == null)
        {
            Debug.Log($"Ability {Name} requires a target.");
            return; //eventually return appropriate error code
        }
        Debug.Log($"Casting ability: {Name}");
        abilityContext.MarkCast();
        StartingEffect.Execute(abilityContext.EffectContext);
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
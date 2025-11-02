public enum StatModifierType
{
   Additive,
    Multiplicative,
    Min,
Max,
Set
}

public class StatModifier
{
    public StatType TargetStat { get; private set; }
    public StatModifierType ModifierType { get; private set; }
    public float Value { get; private set; } 
    public object Source { get; private set; }

    public StatModifier(StatType targetStat, StatModifierType type, float value, object source = null)
    {
        TargetStat = targetStat;
        ModifierType = type;
        Value = value;
        Source = source;
    }

    // <-- Add this method for polymorphism
    public virtual float GetValue(StatManager manager)
    {
        return Value;
    }
}

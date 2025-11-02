using System;

public enum StatType
{
    Strength,
    Agility,
    Stamina,
    MaxHealth,
    HealthRegen
}


public class Stat
{
    public StatType Type { get; private set; }
    public float BaseValue { get; private set; }
    public float CurrentValue { get; internal set; }
    internal bool IsDirty { get; set; } = true;

    public Stat(StatType type, float baseValue)
    {
        Type = type;
        BaseValue = baseValue;
        CurrentValue = baseValue;
    }

    public void SetBaseValue(float value)
    {
        if(value <0){
            return;
        }
        if (Math.Abs(BaseValue - value) > 0.001f)
        {
            BaseValue = value;
            IsDirty = true;
        }
    }
}

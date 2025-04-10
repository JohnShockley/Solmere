using System;
using UnityEngine;

public enum PropertyModType
{
    Flat,
    Percent,
}
[Serializable]
public class PropertyModifier
{
    public readonly float Value;
    public readonly PropertyModType Type;
    public readonly int Order;

    public PropertyModifier(float value, PropertyModType type, int order)
    {
        this.Value = value;
        this.Type = type;
        this.Order = order;
    }
    public PropertyModifier(float value, PropertyModType type) : this(value, type, (int)type) { }


}

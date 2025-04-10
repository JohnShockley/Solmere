using System;
using UnityEngine;

public enum PropertyModType
{
    Flat = 100,
    Percent = 200,
}
[Serializable]
public class PropertyModifier
{
    public readonly float Value;
    public readonly PropertyModType Type;
    public readonly int Order;
    public readonly object Source;

    public PropertyModifier(float value, PropertyModType type, int order, object source)
    {
        Value = value;
        Type = type;
        Order = order;
        Source = source;
    }
    public PropertyModifier(float value, PropertyModType type) : this(value, type, (int)type, null) { }
    public PropertyModifier(float value, PropertyModType type, int order) : this(value, type, order, null) { }
    public PropertyModifier(float value, PropertyModType type, object source) : this(value, type, (int)type, source) { }


}

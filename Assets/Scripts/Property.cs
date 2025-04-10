using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Property
{
    public float baseValue;
    public float Value
    {
        get
        {
            if (isDirty)
            {
                _value = CalculateFinalValue();
                isDirty = false;
            }
            return _value;
        }
    }
    public event Action OnValueChanged;
    private bool isDirty = true;
    [SerializeField]
    private float _value;

    private readonly List<PropertyModifier> statModifiers = new List<PropertyModifier>();

    // public Property(float baseValue)
    // {
    //     this.baseValue = baseValue;
    //     statModifiers = new List<PropertyModifier>();
    // }

    public void AddModifier(PropertyModifier mod)
    {
        statModifiers.Add(mod);
        statModifiers.Sort(CompareModifierOrder);
        MarkDirty();
    }
    private int CompareModifierOrder(PropertyModifier a, PropertyModifier b)
    {
        if (a.Order < b.Order)
        {
            return -1;
        }
        else if (a.Order > b.Order)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
    public bool RemoveModifier(PropertyModifier mod)
    {
        bool removed = statModifiers.Remove(mod);
        if (removed)
        {
            MarkDirty();
        }
        return removed;
    }
    private float CalculateFinalValue()
    {
        float finalValue = baseValue;


        for (int i = 0; i < statModifiers.Count; i++)
        {

            PropertyModifier mod = statModifiers[i];
            if (mod.Type == PropertyModType.Flat)
            {
                finalValue += mod.Value;

            }
            if (mod.Type == PropertyModType.Percent)
            {
                finalValue *= 1 + mod.Value;

            }
        }

        return (float)Math.Round(finalValue, 4);
    }

    private void MarkDirty()
    {
        isDirty = true;
        //OnValueChanged?.Invoke();
    }
}

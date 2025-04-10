using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Property
{
    public float BaseValue;
    public float Value
    {
        get
        {
            if (isDirty || BaseValue != lastBaseValue)
            {
                lastBaseValue = BaseValue;
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
    private float lastBaseValue = float.MinValue;

    private readonly List<PropertyModifier> propertyModifiers;
    public readonly ReadOnlyCollection<PropertyModifier> PropertyModifiers;

    public Property()
    {
       
        propertyModifiers = new List<PropertyModifier>();
        PropertyModifiers = propertyModifiers.AsReadOnly();
    }
    public Property(float baseValue) :this()
    {
        BaseValue = baseValue;
       
    }


    public void AddModifier(PropertyModifier mod)
    {
        propertyModifiers.Add(mod);
        propertyModifiers.Sort(CompareModifierOrder);
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
        bool removed = propertyModifiers.Remove(mod);
        if (removed)
        {
            MarkDirty();
        }
        return removed;
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        bool didRemove = false;
        for (int i = propertyModifiers.Count - 1; i >= 0; i--)
        {
            if (propertyModifiers[i].Source == source)
            {
                didRemove = true;
                isDirty = true;
                propertyModifiers.RemoveAt(i);
            }
        }

        return didRemove;
    }
    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;


        for (int i = 0; i < propertyModifiers.Count; i++)
        {

            PropertyModifier mod = propertyModifiers[i];
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

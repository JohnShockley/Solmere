using System;
using System.Collections.Generic;
using UnityEngine;
#region StatType
public enum StatType
{
    Strength,
    Agility,
    Intelligence,
    Stamina,

    MovementSpeed,
    RotationSpeed,
    JumpPower,

    HealthMin,
    HealthMax,
    HealthRegen,

    MomentumMin,
    MomentumMax,
    MomentumRegen,

    FlowMin,
    FlowMax,
    FlowRegen,


}
#endregion
public class StatManager
{
    private readonly Dictionary<StatType, Stat> stats = new();
    private readonly Dictionary<StatType, Dictionary<StatModifierType, List<StatModifier>>> modifiersByStat = new();
    private readonly Dictionary<StatType, List<StatType>> dependencyMap = new(); // Tracks dependent stats
    private readonly EntityComponent entityComponent;

    public event Action<StatType, float> OnStatChanged;
    private readonly Dictionary<StatType, Action<float>> perStatTrueListeners = new();
    private readonly Dictionary<StatType, Action<float>> perStatRoundedListeners = new();
    private readonly Dictionary<StatType, Action<int>> perStatIntListeners = new();

    #region Initialization
    public StatManager(EntityComponent entityComponent, DefaultStats defaultValueProvider)
    {
        this.entityComponent = entityComponent;
        InitializeStats(defaultValueProvider);
    }

    private void InitializeStats(DefaultStats defaults)
    {
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
            stats[type] = new Stat(type, defaults.GetValue(type));
    }

    #endregion
    #region Subscription
    public void SubscribeTrue(StatType type, Action<float> callback)
    {
        if (!perStatTrueListeners.ContainsKey(type))
            perStatTrueListeners[type] = null;
        perStatTrueListeners[type] += callback;
    }

    public void SubscribeRounded(StatType type, Action<float> callback)
    {
        if (!perStatRoundedListeners.ContainsKey(type))
            perStatRoundedListeners[type] = null;
        perStatRoundedListeners[type] += callback;
    }

    public void SubscribeInt(StatType type, Action<int> callback)
    {
        if (!perStatIntListeners.ContainsKey(type))
            perStatIntListeners[type] = null;
        perStatIntListeners[type] += callback;
    }

    public void UnsubscribeTrue(StatType type, Action<float> callback)
    {
        if (perStatTrueListeners.ContainsKey(type))
            perStatTrueListeners[type] -= callback;
    }
    public void UnsubscribeRounded(StatType type, Action<float> callback)
    {
        if (perStatRoundedListeners.ContainsKey(type))
            perStatRoundedListeners[type] -= callback;
    }
    public void UnsubscribeInt(StatType type, Action<int> callback)
    {
        if (perStatIntListeners.ContainsKey(type))
            perStatIntListeners[type] -= callback;
    }


    #endregion
    #region Snapshot
    public Dictionary<StatType, float> Snapshot()
    {
        var snapshot = new Dictionary<StatType, float>();
        foreach (var kvp in stats)
        {
            var stat = kvp.Value;
            snapshot[kvp.Key] = stat.CurrentValue;
        }
        return snapshot;
    }
    #endregion
    #region Modifiers
    public void AddModifier(StatModifier modifier)
    {
        if (!modifiersByStat.TryGetValue(modifier.TargetStat, out var typeBuckets))
        {
            typeBuckets = new Dictionary<StatModifierType, List<StatModifier>>();
            modifiersByStat[modifier.TargetStat] = typeBuckets;
        }

        if (!typeBuckets.TryGetValue(modifier.ModifierType, out var list))
        {
            list = new List<StatModifier>();
            typeBuckets[modifier.ModifierType] = list;
        }

        list.Add(modifier);

        // If it's a dependency modifier, track the source → target relation
        if (modifier is StatDependencyModifier depMod)
        {
            if (!dependencyMap.TryGetValue(depMod.SourceStat, out var dependents))
            {
                dependents = new List<StatType>();
                dependencyMap[depMod.SourceStat] = dependents;
            }

            if (!dependents.Contains(depMod.TargetStat))
                dependents.Add(depMod.TargetStat);
        }

        MarkDirty(modifier.TargetStat);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        if (modifiersByStat.TryGetValue(modifier.TargetStat, out var typeBuckets) &&
            typeBuckets.TryGetValue(modifier.ModifierType, out var list) &&
            list.Remove(modifier))
        {
            // If it was a dependency modifier, clean up dependency map
            if (modifier is StatDependencyModifier depMod &&
                dependencyMap.TryGetValue(depMod.SourceStat, out var dependents))
            {
                dependents.Remove(depMod.TargetStat);
                if (dependents.Count == 0)
                    dependencyMap.Remove(depMod.SourceStat);
            }

            MarkDirty(modifier.TargetStat);
        }
    }

    public void RemoveAllModifiersFromSource(object source)
    {
        foreach (var kvp in modifiersByStat)
        {
            bool removedAny = false;
            foreach (var bucket in kvp.Value.Values)
            {
                removedAny |= bucket.RemoveAll(m => m.Source == source) > 0;
            }

            if (removedAny)
                MarkDirty(kvp.Key);
        }

        // Clean up dependency map for any removed dependency modifiers
        var toRemove = new List<StatType>();
        foreach (var kvp in dependencyMap)
        {
            kvp.Value.RemoveAll(target =>
            {
                if (modifiersByStat.TryGetValue(target, out var buckets))
                {
                    foreach (var list in buckets.Values)
                    {
                        if (list.Exists(m => m.Source == source && m is StatDependencyModifier))
                            return true;
                    }
                }
                return false;
            });

            if (kvp.Value.Count == 0)
                toRemove.Add(kvp.Key);
        }

        foreach (var key in toRemove)
            dependencyMap.Remove(key);
    }
    #endregion
    #region Recalculation
    private void MarkDirty(StatType type)
    {
        Stat stat = stats[type];

        stat.IsDirty = true;

        // First recalc the stat itself
        Recalculate(stat);

        // Then propagate to dependents
        if (dependencyMap.TryGetValue(type, out var dependents))
        {
            foreach (var dep in dependents)
                MarkDirty(dep); // recursively recalc dependent stats
        }
    }

    private static readonly StatModifierType[] CalculationOrder =
    {
        StatModifierType.Additive,
        StatModifierType.Multiplicative,
        StatModifierType.Min,
        StatModifierType.Max,
        StatModifierType.Set
    };

    private void Recalculate(Stat stat)
    {
        float result = stat.BaseValue;

        if (modifiersByStat.TryGetValue(stat.Type, out var buckets))
        {
            foreach (var type in CalculationOrder)
            {
                if (!buckets.TryGetValue(type, out var mods)) continue;

                switch (type)
                {
                    case StatModifierType.Additive:
                        foreach (var mod in mods)
                            result += mod.GetValue(this);
                        break;

                    case StatModifierType.Multiplicative:
                        foreach (var mod in mods)
                            result *= 1f + mod.GetValue(this);
                        break;

                    case StatModifierType.Min:
                        if (mods.Count > 0)
                        {
                            float maxMin = float.MinValue;
                            foreach (var mod in mods)
                                maxMin = Math.Max(maxMin, mod.GetValue(this));
                            result = Math.Max(result, maxMin);
                        }
                        break;

                    case StatModifierType.Max:
                        if (mods.Count > 0)
                        {
                            float minMax = float.MaxValue;
                            foreach (var mod in mods)
                                minMax = Math.Min(minMax, mod.GetValue(this));
                            result = Math.Min(result, minMax);
                        }
                        break;

                    case StatModifierType.Set:
                        if (mods.Count > 0)
                            result = mods[mods.Count - 1].GetValue(this);
                        break;
                }
            }
        }

        stat.CurrentValue = (float)Math.Round(result);
        stat.IsDirty = false;

        OnStatChanged?.Invoke(stat.Type, stat.CurrentValue);
        if (perStatTrueListeners.TryGetValue(stat.Type, out var trueListeners))
        {
            trueListeners?.Invoke(stat.CurrentValue);
        }
        if (perStatRoundedListeners.TryGetValue(stat.Type, out var roundedListeners))
        {
            roundedListeners?.Invoke(stat.RoundedFloatValue);
        }
        if (perStatIntListeners.TryGetValue(stat.Type, out var intListeners))
        {
            intListeners?.Invoke(stat.IntValue);
        }
    }

    public float GetValue(StatType type)
    {
        return stats[type].CurrentValue;
    }

    #endregion

    #region BaseValue
    public void SetBaseValue(StatType type, float value)
    {
        stats[type].BaseValue = value;
        MarkDirty(type); // propagate recalculation and fire OnStatChanged
    }

    public void ChangeBaseValue(StatType type, float delta)
    {
        if (delta == 0)
        {
            return;
        }
        SetBaseValue(type, stats[type].BaseValue + delta);
    }
    #endregion
    #region Stat
    private class Stat
    {
        public StatType Type { get; private set; }
        public float BaseValue { get; set; }

        private float _currentValue;
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                IntValue = Mathf.RoundToInt(value);
                RoundedFloatValue = (float)Math.Round(value, 2, MidpointRounding.AwayFromZero);
            }
        }

        public float RoundedFloatValue { get; private set; }
        public int IntValue { get; private set; }
        public bool IsDirty { get; set; } = true;

        public Stat(StatType type, float baseValue)
        {
            Type = type;
            BaseValue = baseValue;
            CurrentValue = baseValue; // automatically sets IntValue & RoundedFloatValue
        }
    }

    #endregion

}

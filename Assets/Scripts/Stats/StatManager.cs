using System;
using System.Collections.Generic;

public class StatManager
{
    private readonly Dictionary<StatType, Stat> stats = new();
    private readonly Dictionary<StatType, Dictionary<StatModifierType, List<StatModifier>>> modifiersByStat = new();
    private readonly Dictionary<StatType, List<StatType>> dependencyMap = new(); // Tracks dependent stats
    private readonly EntityComponent entityComponent;

    public event Action<Stat> OnStatChanged;

    public StatManager(EntityComponent entityComponent)
    {
        this.entityComponent = entityComponent;
    }

    public void AddStat(Stat stat)
    {
        if (!stats.ContainsKey(stat.Type))
            stats[stat.Type] = stat;
    }

    public Dictionary<StatType, float> Snapshot()
    {
        var snapshot = new Dictionary<StatType, float>();
        foreach (var kvp in stats)
        {
            var stat = kvp.Value;
            if (!stat.IsDirty)
                snapshot[kvp.Key] = stat.CurrentValue;
        }
        return snapshot;
    }

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

private void MarkDirty(StatType type)
{
    if (!stats.TryGetValue(type, out var stat))
        return;

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



    #endregion

    #region Recalculation
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

        OnStatChanged?.Invoke(stat);
    }

   public float GetValue(StatType type)
{
    return stats.TryGetValue(type, out var stat) ? stat.CurrentValue : 0f;
}

    #endregion

    #region Base Value Management
public void SetBaseValue(StatType type, float value)
{
    if (stats.TryGetValue(type, out var stat))
    {
        stat.SetBaseValue(value);
        MarkDirty(type); // propagate recalculation and fire OnStatChanged
    }
}

public void ChangeBaseValue(StatType type, float delta)
{
    if (stats.TryGetValue(type, out var stat))
    {
        stat.SetBaseValue(stat.BaseValue + delta);
        MarkDirty(type); // propagate recalculation and fire OnStatChanged
    }
}


    #endregion
}

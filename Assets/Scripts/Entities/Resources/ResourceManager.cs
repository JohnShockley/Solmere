using System.Collections.Generic;
using UnityEngine;
using System;
#region ResourceType
public enum ResourceType
{
    Health,
    Momentum,
    Flow,
    Armor
}
#endregion


public class ResourceManager
{
    private readonly float MINVALUE = 0f;
    private readonly Dictionary<ResourceType, Resource> resources = new();
    private readonly Dictionary<ResourceType, float> regenLogAccumulator = new();

    private readonly Dictionary<ResourceType, List<RegenModifier>> regenModifiers = new();

    private readonly EntityComponent entityComponent;
    private readonly StatManager statManager;

    public event Action<ResourceType, float> OnResourceChanged;
    private readonly Dictionary<ResourceType, Action<float>> perResourceTrueListeners = new();
    private readonly Dictionary<ResourceType, Action<float>> perResourceRoundedListeners = new();
    private readonly Dictionary<ResourceType, Action<int>> perResourceIntListeners = new();
    #region Initialization

    public ResourceManager(EntityComponent entityComponent, DefaultResources defaults)
    {
        this.entityComponent = entityComponent;
        this.statManager = entityComponent.statManager;

        InitializeResources(defaults);
    }

    private void InitializeResources(DefaultResources defaults)
    {
        foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
        {
            regenLogAccumulator[resourceType] = 0f;

            var (maxStatType, regenStatType, currentValue) =
                defaults.GetValue(resourceType);

            resources[resourceType] =
                new Resource(resourceType, maxStatType, regenStatType, currentValue);


            statManager.SubscribeTrue(maxStatType, (value) =>
                OnMaxStatChanged(resourceType, maxStatType, value));

            statManager.SubscribeTrue(regenStatType, (value) =>
                OnRegenStatChanged(resourceType, regenStatType, value));
        }
    }
    #endregion
    #region Subscription
    public void SubscribeTrue(ResourceType type, Action<float> callback)
    {
        if (!perResourceTrueListeners.ContainsKey(type))
            perResourceTrueListeners[type] = null;
        perResourceTrueListeners[type] += callback;
    }

    public void SubscribeRounded(ResourceType type, Action<float> callback)
    {
        if (!perResourceRoundedListeners.ContainsKey(type))
            perResourceRoundedListeners[type] = null;
        perResourceRoundedListeners[type] += callback;
    }

    public void SubscribeInt(ResourceType type, Action<int> callback)
    {
        if (!perResourceIntListeners.ContainsKey(type))
            perResourceIntListeners[type] = null;
        perResourceIntListeners[type] += callback;
    }

    public void UnsubscribeTrue(ResourceType type, Action<float> callback)
    {
        if (perResourceTrueListeners.ContainsKey(type))
            perResourceTrueListeners[type] -= callback;
    }
    public void UnsubscribeRounded(ResourceType type, Action<float> callback)
    {
        if (perResourceRoundedListeners.ContainsKey(type))
            perResourceRoundedListeners[type] -= callback;
    }
    public void UnsubscribeInt(ResourceType type, Action<int> callback)
    {
        if (perResourceIntListeners.ContainsKey(type))
            perResourceIntListeners[type] -= callback;
    }
    #endregion
    #region Snapshot
    public Dictionary<ResourceType, float> Snapshot()
    {
        var snapshot = new Dictionary<ResourceType, float>();
        foreach (var kvp in resources)
        {
            snapshot[kvp.Key] = kvp.Value.CurrentValue;
        }
        return snapshot;
    }
    #endregion
    #region Modifiers
    public void AddRegenModifier(ResourceType type, RegenModifier modifier)
    {
        if (!regenModifiers.ContainsKey(type))
            regenModifiers[type] = new List<RegenModifier>();

        regenModifiers[type].Add(modifier);
    }
    public void RemoveRegenModifier(ResourceType type, RegenModifier modifier)
    {
        if (regenModifiers.ContainsKey(type))
        {
            regenModifiers[type].Remove(modifier);
        }
    }
    public void RemoveAllRegenModifiersFromSource(object source)
    {
        foreach (var type in regenModifiers.Keys)
        {
            regenModifiers[type].RemoveAll(mod => mod.Source == source);
        }
    }






    #endregion
    #region Recalculation
    private void OnMaxStatChanged(ResourceType resourceType, StatType statType, float value)
    {
        Resource resource = resources[resourceType];
        if (resource.CurrentValue > value) resource.CurrentValue = value;
    }
    private void OnMinStatChanged(ResourceType resourceType, StatType statType, float value)
    {
        Resource resource = resources[resourceType];
        if (resource.CurrentValue < value) resource.CurrentValue = value;
    }
    private void OnRegenStatChanged(ResourceType resourceType, StatType statType, float value)
    {
        // Handle max stat change if needed
    }
    private void ResourceChanged(ResourceType type)
    {
        var res = resources[type];

        // ---- True value changed? ----
        if (!Mathf.Approximately(res.CurrentValue, res.LastTrueValue))
        {
            res.LastTrueValue = res.CurrentValue;

            OnResourceChanged?.Invoke(type, res.CurrentValue);

            if (perResourceTrueListeners.TryGetValue(res.Type, out var trueListeners))
                trueListeners?.Invoke(res.CurrentValue);
        }

        // ---- Rounded float changed? ----
        if (!Mathf.Approximately(res.RoundedFloatValue, res.LastRoundedFloatValue))
        {
            res.LastRoundedFloatValue = res.RoundedFloatValue;

            if (perResourceRoundedListeners.TryGetValue(res.Type, out var roundedListeners))
                roundedListeners?.Invoke(res.RoundedFloatValue);
        }

        // ---- Int changed? ----
        if (res.IntValue != res.LastIntValue)
        {
            res.LastIntValue = res.IntValue;

            if (perResourceIntListeners.TryGetValue(res.Type, out var intListeners))
                intListeners?.Invoke(res.IntValue);
        }
    }

    private void SetResourceCurrent(ResourceType type, float value)
    {
        var res = resources[type];
        var newValue = value;
        var maxValue = statManager.GetValue(res.MaxStat);
        if (newValue < MINVALUE)
        {
            newValue = MINVALUE;
        }
        else if (newValue > maxValue)
        {
            newValue = maxValue;
        }
        res.CurrentValue = newValue;
        ResourceChanged(type);
    }

    public void ChangeCurrentValue(ResourceType type, float delta)
    {
        if (delta == 0)
        {
            return;
        }
        SetResourceCurrent(type, resources[type].CurrentValue + delta);
    }


    // Spend resource: returns true if successful
    public bool TrySpend(ResourceType type, float amount)
    {
        var res = resources[type];


        if (res.CurrentValue >= amount)
        {
            ChangeCurrentValue(type, -amount);
            return true;
        }
        return false;
    }

    // Use up to amount: returns actual amount spent
    public float SpendUpTo(ResourceType type, float minimumToSpend, float maximumToSpend)
    {
        var res = resources[type];
        var spent = 0f;
        if (res.CurrentValue >= minimumToSpend)
        {
            if (res.CurrentValue > maximumToSpend)
            {
                spent = maximumToSpend;
                ChangeCurrentValue(type, -spent);
            }
            else
            {
                spent = res.CurrentValue;
                ChangeCurrentValue(type, -res.CurrentValue);

            }
        }
        return spent;
    }

    public float GetValue(ResourceType type)
    {
        return resources[type].CurrentValue;
    }


    #endregion

    #region Regeneration


    // Regeneration tick (call every frame or fixed timestep)
    public void Tick(float deltaTime)
    {
        foreach (var r in resources)
        {
            var resourceType = r.Key;
            var resource = r.Value;

            var regenThisTick = statManager.GetValue(resource.RegenStat) * deltaTime;




            if (regenModifiers.TryGetValue(resourceType, out var mods))
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    var regenMod = mods[i];

                    float appliedAmount = regenMod.DrainRate * deltaTime;
                    regenThisTick += appliedAmount;

                    if (regenMod.TotalAmount != -1)
                    {
                        regenMod.TotalAmount -= Mathf.Abs(appliedAmount);
                        if (regenMod.TotalAmount <= 0)
                            mods.RemoveAt(i);
                    }
                }
            }

            ChangeCurrentValue(resourceType, regenThisTick);

        }
    }


    #endregion
    #region Resource
    private class Resource
    {
        public ResourceType Type;

        public StatType MaxStat;
        public StatType RegenStat;

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

        public float LastTrueValue { get; set; }
        public float LastRoundedFloatValue { get; set; }
        public int LastIntValue { get; set; }

        public Resource(ResourceType type, StatType maxStat, StatType regenStat, float currentValue)
        {
            Type = type;
            MaxStat = maxStat;
            RegenStat = regenStat;

            CurrentValue = currentValue;
            LastTrueValue = CurrentValue;
            LastRoundedFloatValue = RoundedFloatValue;
            LastIntValue = IntValue;
        }
    }
    #endregion
}

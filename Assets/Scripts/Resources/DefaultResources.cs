using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultResources", menuName = "Resources/Default Resources")]
public class DefaultResources : ScriptableObject
{
    [Serializable]
    public class ResourceEntry
    {
        public ResourceType Type;

        public StatType MinStat;
        public StatType MaxStat;
        public StatType RegenStat;

        public float StartingValue;
    }

    public List<ResourceEntry> Resources = new();

    public (StatType min, StatType max, StatType regen, float current) GetValue(ResourceType type)
    {
        foreach (var r in Resources)
            if (r.Type == type)
                return (r.MinStat, r.MaxStat, r.RegenStat, r.StartingValue);

        return (default, default, default, 0);
    }

#if UNITY_EDITOR
    // Auto-populate entries when enum changes
    private void OnValidate()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (!Resources.Exists(r => r.Type == type))
            {
                Resources.Add(new ResourceEntry
                {
                    Type = type
                });
            }
        }
    }
#endif
}

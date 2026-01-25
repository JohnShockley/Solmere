using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Default Stats")]
public class DefaultStats : ScriptableObject
{
    public List<StatEntry> Stats = new List<StatEntry>();

    [Serializable]
    public class StatEntry
    {
        public StatType Type;
        public float Value;
    }

    public float GetValue(StatType type)
    {
        foreach (var s in Stats)
            if (s.Type == type)
                return s.Value;

        return 0f;
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        var names = Enum.GetValues(typeof(StatType));
        foreach (StatType type in names)
        {
            if (!Stats.Exists(s => s.Type == type))
                Stats.Add(new StatEntry { Type = type, Value = 0f });
        }
    }
#endif

}

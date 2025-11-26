using System.Collections.Generic;
using UnityEngine;

public struct EffectContext
{
    public GameObject Source;
    public GameObject Target;
    public Vector3? Position;
    public Dictionary<StatType, float> StatSnapshot;

    public EffectContext(GameObject source, Dictionary<StatType, float> statSnapshot, GameObject target = null, Vector3? position = null)
    {
        Source = source;
        StatSnapshot = statSnapshot;
        Target = target;
        Position = position;
    }
}

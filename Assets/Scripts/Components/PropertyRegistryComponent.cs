using System.Collections.Generic;
using UnityEngine;
public class PropertyRegistryComponent : MonoBehaviour
{
    private Dictionary<string, Property> properties = new();

    public void Register(string key, Property property)
    {
        properties[key] = property;
    }

    public Property Get(string key)
    {
        return properties.TryGetValue(key, out var prop) ? prop : null;
    }

    public PropertySnapshot Snapshot(params string[] keys)
    {
        var snapshot = new PropertySnapshot();
        foreach (var key in keys)
        {
            var prop = Get(key);
            if (prop != null)
            {
                snapshot.Set(key, prop.Value); // Copy value at this moment
            }
        }


        return snapshot;
    }
    public PropertySnapshot SnapshotAll()
    {
        var snapshot = new PropertySnapshot();
        foreach (var kvp in properties)
        {
            snapshot.Set(kvp.Key, kvp.Value.Value);
        }
        return snapshot;
    }

    public T GetValue<T>(string key) => (T)(object)Get(key)?.Value;
}
public class PropertySnapshot
{
    private readonly Dictionary<string, float> values = new();

    public void Set(string key, float value)
    {
        values[key] = value;
    }

    public float Get(string key)
    {
        return values.TryGetValue(key, out var val) ? val : default;
    }

    public bool Has(string key) => values.ContainsKey(key);
}
using System.Collections.Generic;
using UnityEngine;
public class PropertyRegistryComponent : MonoBehaviour
{
    private Dictionary<string, Property> properties = new();

    public void Register(string key, Property property)
    {
        properties[key] = property;
        Debug.Log($"Property Registered: {key}");
    }

    public Property Get(string key)
    {
        return properties.TryGetValue(key, out var prop) ? prop : null;
    }

    public T GetValue<T>(string key) => (T)(object)Get(key)?.Value;
}

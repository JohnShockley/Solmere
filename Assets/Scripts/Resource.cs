using System;
using UnityEngine;
[Serializable]
public class Resource
{
    [SerializeField] private float Current;

    [SerializeField] private Property max;
    [SerializeField] private Property min;


    public void Add(float amount)
    {
        Set(Current + amount);
    }

    public void Set(float newValue)
    {
        Current = Mathf.Clamp(newValue, min.Value, max.Value);
    }

    public Property GetMax()
    {
        return max;
    }
    public Property GetMin()
    {
        return min;
    }

    public float GetCurrent()
    {
        return Current;
    }


    public float Normalized => (Current - min.Value) / (max.Value - min.Value);
}

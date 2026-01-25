using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : ScriptableObject
{
     // The context contains all data the effect might need
    public EffectContext Context { get; private set; }
    public List<Effect> Children = new();
    public abstract void Execute(EffectContext context);
}
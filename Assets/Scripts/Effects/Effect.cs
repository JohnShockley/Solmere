using System.Collections.Generic;

public abstract class Effect
{
     // The context contains all data the effect might need
    public EffectContext Context { get; private set; }
    public List<Effect> Children = new();
    public abstract void Execute(EffectContext context);
}
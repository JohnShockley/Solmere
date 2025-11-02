
public class StatDependencyModifier : StatModifier
{
    public StatType SourceStat { get; }
    public float Factor { get; }

    public StatDependencyModifier(StatType target, StatType source, float factor, object sourceObject = null)
        : base(target, StatModifierType.Additive, 0f, sourceObject)
    {
        SourceStat = source;
        Factor = factor;
    }

    // <-- override to calculate value based on current source stat
    public override float GetValue(StatManager manager)
    {
        float sourceVal = manager.GetValue(SourceStat);
        return sourceVal * Factor;
    }
}


public static class EffectFactory
{
    public static DamageEffect CreateDamageEffect(float amount, StatType stat)
    {
        return new DamageEffect(amount, stat);
    }
}

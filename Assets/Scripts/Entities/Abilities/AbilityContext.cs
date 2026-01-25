using System.Collections.Generic;

using UnityEngine;
public class AbilityContext
{
    public GameObject Caster;
    // public Dictionary<StatType, float> StatSnapshot;
    private float _lastCastTime;
    public Ability Ability;

    public EffectContext EffectContext;


    public AbilityContext(Ability ability, GameObject caster)
    {
        Ability = ability;
        Caster = caster;

    }
    public bool CanCast()
    {
        return Time.time >= _lastCastTime + Ability.Cooldown;
    }

    public void MarkCast()
    {
        _lastCastTime = Time.time;
    }
}

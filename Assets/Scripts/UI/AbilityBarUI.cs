using UnityEngine;

public class AbilityBarUI : MonoBehaviour
{
    public AbilityComponent abilityComponent;
    public AbilitySlotUI[] slots; // size 4

    private void Start()
{
    for (int i = 0; i < slots.Length; i++)
    {
        Ability ability = null;

        if (i < abilityComponent.Abilities.Count)
            ability = abilityComponent.Abilities[i];

        slots[i].Initialize(abilityComponent, ability);
    }
}

}

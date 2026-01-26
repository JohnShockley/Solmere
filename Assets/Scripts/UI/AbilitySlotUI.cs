using UnityEngine;
using UnityEngine.UI;

public class AbilitySlotUI : MonoBehaviour
{
    public Button button;
    public Ability ability;

    private AbilityComponent abilityComponent;

    public void Initialize(AbilityComponent comp, Ability assignedAbility)
    {
        abilityComponent = comp;
        ability = assignedAbility;

        button.onClick.RemoveAllListeners();

        if (ability == null)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        abilityComponent.CastAbility(ability);
    }
}

using UnityEngine;

[RequireComponent(typeof(EntityComponent))]
public class CombatComponent : MonoBehaviour
{
    private EntityComponent entity;
    void Awake()
    {
        entity = GetComponent<EntityComponent>();
    }
    public void SetTarget(GameObject target)
    {
        entity.CurrentTarget = target;
        Debug.Log("Target set to: " + target.name);
    }

    public void ClearTarget()
    {
        entity.CurrentTarget = null;
        Debug.Log("Target cleared.");
    }
}

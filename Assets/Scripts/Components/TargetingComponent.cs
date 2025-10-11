using UnityEngine;

public class TargetingComponent : MonoBehaviour
{
    private GameObject Target;
    public void SetTarget(GameObject target)
    {
        if (target == null || target.GetComponent<TargetableComponent>() == null)
        {
            return;
        }
        Target = target;
    }

    public bool HasTarget()
    {
        return (bool)Target;
    }

    public GameObject GetTarget()
    {
        if (HasTarget())
        {
            return Target;
        }
        return null;
    }

    public void ClearTarget()
    {
        Target = null;
    }
}

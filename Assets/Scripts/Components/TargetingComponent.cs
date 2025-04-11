using UnityEngine;

public class TargetingComponent : MonoBehaviour
{
    public GameObject Target { get; private set; }
    public void SetTarget(GameObject target)
    {
        if (target == null || target.GetComponent<TargetableComponent>() == null)
        {
            return;
        }
        Target = target;
    }

    public void ClearTarget()
    {
        Target = null;
    }
}

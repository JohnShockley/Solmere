using UnityEngine;
[RequireComponent(typeof(CombatComponent))]

public class ClickTargeting : MonoBehaviour
{
    public Camera cam;
    private CombatComponent combat;
    void Awake()
    {
        combat = GetComponent<CombatComponent>();
    }
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            var target = hit.collider.GetComponent<EntityComponent>();
            if (target != null)
                combat.SetTarget(target.gameObject);
        }
    }
}

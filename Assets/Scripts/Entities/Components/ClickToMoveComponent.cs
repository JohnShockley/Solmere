using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

[RequireComponent(typeof(PathToDirectionComponent))]
public class ClickToMoveComponent : MonoBehaviour
{
    private Camera cam;
    private PathToDirectionComponent pathFollower;
    private NavMeshPath navPath;

    private void Awake()
    {
        cam = Camera.main;
        pathFollower = GetComponent<PathToDirectionComponent>();
        navPath = new NavMeshPath();
    }

    public void OnRightClick(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit))
        {
            if (NavMesh.CalculatePath(transform.position, hit.point, NavMesh.AllAreas, navPath))
            {
                pathFollower.SetPath(navPath.corners);
            }
        }
    }
}

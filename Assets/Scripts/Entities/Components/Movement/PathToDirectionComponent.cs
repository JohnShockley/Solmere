using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MoveComponent))]
public class PathToDirectionComponent : MonoBehaviour
{
    private MoveComponent mover;
    private Vector3[] path;
    private int currentIndex = 0;

    [SerializeField]
    private float waypointTolerance = 0.1f;

    private void Awake()
    {
        mover = GetComponent<MoveComponent>();
    }

public void SetPath(Vector3[] newPath)
{
    path = newPath;
    currentIndex = 0;

    // Skip waypoints behind the character
    while (currentIndex < path.Length)
    {
        Vector3 toWaypoint = path[currentIndex] - transform.position;
        toWaypoint.y = 0;
        if (toWaypoint.magnitude > waypointTolerance)
            break;
        currentIndex++;
    }
}

    private void Update()
    {
        if (path == null || currentIndex >= path.Length)
        {
            mover.SetMoveDirection(Vector3.zero);
            return;
        }

        Vector3 target = path[currentIndex];
        Vector3 dir = target - transform.position;
        dir.y = 0;

        if (dir.magnitude <= waypointTolerance)
        {
            currentIndex++;
            return;
        }

        mover.SetMoveDirection(dir.normalized);
    }

    public bool IsPathComplete => path == null || currentIndex >= path.Length;
    private void OnDrawGizmos()
    {
        if (path == null || path.Length == 0)
            return;

        Gizmos.color = Color.green;
        for (int i = currentIndex; i < path.Length; i++)
        {
            Gizmos.DrawSphere(path[i], 0.1f);

            if (i > currentIndex)
                Gizmos.DrawLine(path[i - 1], path[i]);
        }

        // Optional: line from entity to first waypoint
        if (currentIndex < path.Length)
            Gizmos.DrawLine(transform.position, path[currentIndex]);
    }

}

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PathToDirectionComponent))]
public class PatrolAI : MonoBehaviour
{
    public Transform[] waypoints;
    public float recalcRate = 1f;

    private int currentIndex = 0;
    private PathToDirectionComponent pathFollower;
    private NavMeshPath navPath;
    private float timer;

    private void Awake()
    {
        pathFollower = GetComponent<PathToDirectionComponent>();
        navPath = new NavMeshPath();
    }

    private void Start()
    {
        if (waypoints.Length > 0)
        {
            UpdatePathToCurrentWaypoint();
        }
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        timer -= Time.deltaTime;

        // Move to next waypoint only if path is complete
        if (pathFollower.IsPathComplete)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
            UpdatePathToCurrentWaypoint();
        }
        else if (timer <= 0f)
        {
            // Recalculate path periodically to avoid getting stuck
            UpdatePathToCurrentWaypoint();
        }
    }

    private void UpdatePathToCurrentWaypoint()
    {
        Vector3 target = waypoints[currentIndex].position;
        if (NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, navPath))
        {
            pathFollower.SetPath(navPath.corners);
        }
        timer = recalcRate;
    }
}

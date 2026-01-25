using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PathToDirectionComponent))]
public class ChasePlayerAI : MonoBehaviour
{
    public Transform target; // player transform
    public float recalcRate = 0.5f; // how often to recalc path
    private float timer;

    private PathToDirectionComponent pathFollower;
    private NavMeshPath navPath;

    private void Awake()
    {
        pathFollower = GetComponent<PathToDirectionComponent>();
        navPath = new NavMeshPath();
    }

    private void Update()
    {
        if (target == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = recalcRate;
            // Calculate new NavMesh path
            if (NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, navPath))
            {
                pathFollower.SetPath(navPath.corners);
            }
        }
    }
}

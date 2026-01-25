using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PathToDirectionComponent))]
public class WanderAI : MonoBehaviour
{
    public float wanderRadius = 5f;
    public float wanderTime = 5f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float timer;

    private PathToDirectionComponent pathFollower;
    private NavMeshPath navPath;

    private void Awake()
    {
        pathFollower = GetComponent<PathToDirectionComponent>();
        navPath = new NavMeshPath();
        startPosition = transform.position;
        PickNewTarget();
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f || (targetPosition - transform.position).magnitude < 0.5f)
        {
            PickNewTarget();
        }
    }

    private void PickNewTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        targetPosition = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
        timer = wanderTime;

        if (NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, navPath))
        {
            pathFollower.SetPath(navPath.corners);
        }
    }
}

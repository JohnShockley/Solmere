using System.Collections.Generic;
using UnityEngine;

public class SpawnRegion : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField] private float radius = 10f; // Circular spawn region radius

    private float spawnArea;

    private List<GameObject> managedSpawns = new List<GameObject>();

    private float currentSpawnDensity;

    [SerializeField] private float maximumSpawnDensity;

    [SerializeField] private float spawnDelayModifier;
    private float nextSpawnTime;
    private void Start()
    {
        spawnArea = Mathf.PI * radius * radius;
        nextSpawnTime = Time.time + GetSpawnDelay();
    }
    private void Update()
    {
        CleanDeadSpawns(); // Remove destroyed enemies from tracking

        float currentDensity = managedSpawns.Count / spawnArea;

        if (currentDensity < maximumSpawnDensity && Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + GetSpawnDelay();
        }
    }
    private void SpawnEnemy()
    {
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        Vector3 spawnPos = transform.position + new Vector3(randomPoint.x, 0, randomPoint.y);

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        managedSpawns.Add(newEnemy);
    }

    private void CleanDeadSpawns()
    {
        // Remove nulls from list (i.e., enemies that were destroyed)
        managedSpawns.RemoveAll(spawn => spawn == null);
    }

    private float GetSpawnDelay()
    {
        // Example: delay based on spawn density — faster when population is low
        return spawnDelayModifier / Mathf.Max(0.1f, 1f - (managedSpawns.Count / (maximumSpawnDensity * spawnArea)));
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the spawn area in editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

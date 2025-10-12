using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int samplingAttempts, int seed)
    {
        System.Random prng = new System.Random(seed);
        float cellSize = radius / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(sampleRegionSize / 2);
        while (spawnPoints.Count > 0)
        {

            int spawnIndex = prng.Next(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[spawnIndex];

            bool candidateAccepted = false;
            for (int i = 0; i < samplingAttempts; i++)
            {

                float angle = (float)prng.NextDouble() * Mathf.PI * 2;

                Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCenter + direction * (radius + (float)prng.NextDouble() * radius);

                if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
        return points;
    }

    static bool IsValid(Vector2 candidate, Vector2 origin, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        // Translate candidate to grid coordinates
        Vector2 localCandidate = candidate - origin;

        if (!(localCandidate.x >= 0 && localCandidate.x < sampleRegionSize.x && localCandidate.y >= 0 && localCandidate.y < sampleRegionSize.y))
            return false;

        int cellX = (int)(localCandidate.x / cellSize);
        int cellY = (int)(localCandidate.y / cellSize);

        int searchStartX = Mathf.Max(0, cellX - 2);
        int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
        int searchStartY = Mathf.Max(0, cellY - 2);
        int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

        for (int x = searchStartX; x <= searchEndX; x++)
        {
            for (int y = searchStartY; y <= searchEndY; y++)
            {
                int pointIndex = grid[x, y] - 1;
                if (pointIndex != -1)
                {
                    float sqrDist = (candidate - points[pointIndex]).sqrMagnitude;
                    if (sqrDist < radius * radius)
                        return false;
                }
            }
        }
        return true;
    }

    static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        if (!(candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y))
        {
            return false;
        }
        int cellX = (int)(candidate.x / cellSize);
        int cellY = (int)(candidate.y / cellSize);
        int searchStartX = Mathf.Max(0, cellX - 2);
        int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
        int searchStartY = Mathf.Max(0, cellY - 2);
        int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

        for (int x = searchStartX; x <= searchEndX; x++)
        {
            for (int y = searchStartY; y <= searchEndY; y++)
            {
                int pointIndex = grid[x, y] - 1;
                if (pointIndex != -1)
                {
                    float squareDistance = (candidate - points[pointIndex]).sqrMagnitude;
                    if (squareDistance < radius * radius)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public static List<Vector2> GeneratePointsInPolygon(
    float radius,
    List<Vector2> polygon,
    int samplingAttempts,
    int seed)
    {
        // Compute polygon bounding box
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        foreach (var p in polygon)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        Vector2 sampleRegionSize = new Vector2(maxX - minX, maxY - minY);
        System.Random prng = new System.Random(seed);
        float cellSize = radius / Mathf.Sqrt(2);

        int gridWidth = Mathf.CeilToInt(sampleRegionSize.x / cellSize);
        int gridHeight = Mathf.CeilToInt(sampleRegionSize.y / cellSize);

        int[,] grid = new int[gridWidth, gridHeight];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        // Start from a random point *inside* polygon
        Vector2 startPoint;
        int tries = 0;
        do
        {
            startPoint = new Vector2(
                (float)prng.NextDouble() * sampleRegionSize.x + minX,
                (float)prng.NextDouble() * sampleRegionSize.y + minY
            );
            tries++;
        }
        while (!PointInPolygon(startPoint, polygon) && tries < 1000);

        if (tries >= 1000)
        {
            Debug.LogWarning("Could not find initial point inside polygon!");
            return points;
        }

        spawnPoints.Add(startPoint);

        while (spawnPoints.Count > 0)
        {
            int spawnIndex = prng.Next(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < samplingAttempts; i++)
            {
                float angle = (float)prng.NextDouble() * Mathf.PI * 2f;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCenter + dir * (radius + (float)prng.NextDouble() * radius);

                if (!PointInPolygon(candidate, polygon)) continue; // 🚫 reject if outside polygon

                if (IsValid(candidate, new Vector2(minX, minY), sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    int cellX = (int)((candidate.x - minX) / cellSize);
                    int cellY = (int)((candidate.y - minY) / cellSize);
                    grid[cellX, cellY] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }

            if (!candidateAccepted)
                spawnPoints.RemoveAt(spawnIndex);
        }

        return points;
    }
    private static bool PointInPolygon(Vector2 p, List<Vector2> poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            bool intersect = ((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                             (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) /
                              (poly[j].y - poly[i].y) + poly[i].x);
            if (intersect)
                inside = !inside;
        }
        return inside;
    }

}

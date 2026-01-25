using System.Collections.Generic;
using UnityEngine;
using System;
using VoronatorSharp;
using System.Linq;

public static class Voronoi
{
    private static List<Vector2> points = new List<Vector2>();
    private static List<Vector3> hullEdgePoints = new List<Vector3>();
    private static List<Vector3> borderEdgePoints = new List<Vector3>();
    private static List<Vector3> triangleEdgePoints = new List<Vector3>();
    private static List<Vector3> triangleCenters = new List<Vector3>();
    private static List<Vector3> voronoiEdgeVerticePoints = new List<Vector3>();
    private static List<Vector3> voronoiEdgePoints = new List<Vector3>();
    private static List<VoronoiCell> voronoiCells = new List<VoronoiCell>();

    public static Voronator v;
    public static Delaunator d;

    public enum NodeType { land, ocean };
    public class VoronoiCell
    {
        public int id;
        public Vector2 coordinate;
        public List<Vector2> vertices;
        public List<int> neighborIDs;
        public NodeType nodeType;
        public double height;
        public bool used;


    }
    [System.Serializable]
    public struct IslandData
    {
        [Range(0, .99f)]
        public double heightDecay;
        [Range(0, 0.5f)]
        public double sharpness;
    }

    public static List<VoronoiCell> CreateVoronoi(VoronoiData voronoiData, int width, int height)
    {
        int seed = voronoiData.seed;
        int relaxations = voronoiData.relaxations;
        int pointCount = voronoiData.pointCount;
        int poissonAttempts = voronoiData.poissonAttempts;
        float poissonRadius = voronoiData.poissonRadius;

        ClearData();

        List<Vector2> polygon = new List<Vector2>()
{
    new Vector2(500, 0),
    new Vector2(1000, 250),
    new Vector2(1000, 750),
    new Vector2(500, 1000),
    new Vector2(0, 750),
    new Vector2(0, 250)
};



        polygon = EnsureClockwise(polygon);
        if (voronoiData.usePoissonDiscSampling)
        {
            GeneratePoissonPoints(seed, polygon, poissonAttempts, poissonRadius);
        }
        else
        {
            GenerateRandomPoints(seed, polygon, pointCount);
        }





        List<Vector2> filteredPoints = points
     .Where(p => PointInPolygon(p, polygon))
     .ToList();

        v = new Voronator(filteredPoints, polygon);

        for (int i = 0; i < relaxations; i++)
        {
            points = v.GetClippedRelaxedPoints();
            v = new Voronator(points, polygon);
        }
        d = v.Delaunator;
        StoreData();
        for (int i = 0; i < points.Count; i++)
        {
            VoronoiCell cell = new VoronoiCell();
            cell.id = i;
            cell.coordinate = points[i];
            cell.vertices = v.GetClippedPolygon(i);
            cell.neighborIDs = v.ClippedNeighbors(i).ToList();

            voronoiCells.Add(cell);

        }
        AddIsland(voronoiData);
        return voronoiCells;
    }

    private static bool PointInPolygon(Vector2 p, List<Vector2> poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            bool intersect = ((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                             (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x);
            if (intersect)
                inside = !inside;
        }
        return inside;
    }
    private static List<Vector2> EnsureClockwise(List<Vector2> poly)
    {
        float area = 0f;
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 p1 = poly[i];
            Vector2 p2 = poly[(i + 1) % poly.Count];
            area += (p2.x - p1.x) * (p2.y + p1.y);
        }
        if (area > 0f)
        {
            poly.Reverse(); // Ensure clockwise
        }
        return poly;
    }
    public static void AddIsland(VoronoiData voronoiData)
    {

        System.Random prng = new System.Random(voronoiData.seed);
        for (int i = 0; i < voronoiData.islandCount; i++)
        {
            if (i == 0)
            {
                AddLargeIsland(prng, voronoiData);
            }
            else
            {
                AddSmallIsland(prng, voronoiData);
            }
        }
    }
    private static void AddLargeIsland(System.Random prng, VoronoiData voronoiData)
    {

        int startingCellIndex = prng.Next(voronoiCells.Count);
        double height = .9f;
        double heightDecay = .9f;
        double sharpness = voronoiData.islandData.sharpness;
        Queue<int> q = new Queue<int>();

        List<int> deuseList = new List<int>();

        voronoiCells[startingCellIndex].height = height;
        voronoiCells[startingCellIndex].used = true;

        q.Enqueue(startingCellIndex);
        deuseList.Add(startingCellIndex);

        while (q.Count() > 0 && height > 0.01)
        {
            VoronoiCell front = voronoiCells[q.Dequeue()];

            height = front.height * heightDecay;
            foreach (var neighborID in front.neighborIDs)
            {
                if (!voronoiCells[neighborID].used)
                {

                    double mod = prng.NextDouble() * sharpness + 1.1f - sharpness;

                    if (sharpness == 0)
                    {
                        mod = 1;
                    }

                    voronoiCells[neighborID].height += height * mod;
                    if (voronoiCells[neighborID].height > 1)
                    {
                        voronoiCells[neighborID].height = 1;
                    }
                    voronoiCells[neighborID].used = true;
                    q.Enqueue(neighborID);
                    deuseList.Add(neighborID);
                }
            }
        }

        foreach (var i in deuseList)
        {
            voronoiCells[i].used = false;
        }

    }
    private static void AddSmallIsland(System.Random prng, VoronoiData voronoiData)
    {


        int startingCellIndex = prng.Next(voronoiCells.Count);

        double height = prng.NextDouble();
        double heightDecay = voronoiData.islandData.heightDecay;
        Queue<int> q = new Queue<int>();

        List<int> deuseList = new List<int>();

        voronoiCells[startingCellIndex].height = height;
        voronoiCells[startingCellIndex].used = true;

        q.Enqueue(startingCellIndex);
        deuseList.Add(startingCellIndex);


        while (q.Count() > 0 && height > 0.01)
        {
            VoronoiCell front = voronoiCells[q.Dequeue()];

            height *= heightDecay;
            foreach (var neighborID in front.neighborIDs)
            {
                if (!voronoiCells[neighborID].used)
                {

                    voronoiCells[neighborID].height += height;
                    if (voronoiCells[neighborID].height > 1)
                    {
                        voronoiCells[neighborID].height = 1;
                    }
                    voronoiCells[neighborID].used = true;
                    q.Enqueue(neighborID);
                    deuseList.Add(neighborID);
                }
            }
        }

        foreach (var i in deuseList)
        {
            voronoiCells[i].used = false;
        }

    }

    public static int Find(Vector2 u, int i = 0)
    {
        return v.Find(u, i);
    }

    static void ClearData()
    {
        points.Clear();
        hullEdgePoints.Clear();
        borderEdgePoints.Clear();
        triangleEdgePoints.Clear();
        triangleCenters.Clear();
        voronoiEdgeVerticePoints.Clear();
        voronoiEdgePoints.Clear();
        voronoiCells.Clear();
    }

    static void StoreData()
    {
        StoreVoronoiVerticePoints();
        StoreVoronoiEdgePoints();
    }

    static void GenerateRandomPoints(int seed, List<Vector2> polygon, int pointCount)
    {
        System.Random prng = new System.Random(seed);

        // Compute polygon bounding box
        float minX = polygon.Min(p => p.x);
        float maxX = polygon.Max(p => p.x);
        float minY = polygon.Min(p => p.y);
        float maxY = polygon.Max(p => p.y);

        int attempts = 0;
        while (points.Count < pointCount && attempts < pointCount * 10)
        {
            float x = (float)prng.NextDouble() * (maxX - minX) + minX;
            float y = (float)prng.NextDouble() * (maxY - minY) + minY;
            Vector2 candidate = new Vector2(x, y);

            if (PointInPolygon(candidate, polygon))
            {
                points.Add(candidate);
            }
            attempts++;
        }

        if (points.Count < pointCount)
            Debug.LogWarning("Could not generate enough points inside the polygon with random sampling.");
    }

    static void GeneratePoissonPoints(int seed, List<Vector2> polygon, int poissonAttempts, float poissonRadius)
    {


        points = PoissonDiscSampling.GeneratePointsInPolygon(poissonRadius, polygon, poissonAttempts, seed);
    }
    static void StoreVoronoiVerticePoints()
    {
        if (v == null)
        {
            Debug.LogError("Voronator instance is null!");
            return;
        }
        if (voronoiEdgeVerticePoints == null) voronoiEdgeVerticePoints = new List<Vector3>();

        for (var i = 0; i < points.Count; i++)
        {
            var polygon = v.GetClippedPolygon(i);
            if (polygon == null) continue; // Skip if the result is null

            foreach (var point in polygon)
            {
                voronoiEdgeVerticePoints.Add(point);
            }
        }
    }

    static void StoreVoronoiEdgePoints()
    {
        for (var i = 0; i < points.Count; i++)
        {
            var polygonVertices = v.GetClippedPolygon(i);
            if (polygonVertices == null) continue; // Skip if the result is null

            Vector3 start = new Vector3(polygonVertices[0].x, 0, polygonVertices[0].y);
            Vector3 end = new Vector3(polygonVertices[polygonVertices.Count - 1].x, 0, polygonVertices[polygonVertices.Count - 1].y);

            for (int j = 0; j < polygonVertices.Count - 1; j++)
            {

                voronoiEdgePoints.Add(new Vector3(polygonVertices[j].x, 0, polygonVertices[j].y));
                voronoiEdgePoints.Add(new Vector3(polygonVertices[j + 1].x, 0, polygonVertices[j + 1].y));


            }

            voronoiEdgePoints.Add(start);
            voronoiEdgePoints.Add(end);
        }
    }
}

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

    static Voronator v;
    static Delaunator d;

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
    public struct Island
    {
        [Min(0)]
        public int startID;
        [Range(0, 1f)]
        public double height;
        [Range(0, .99f)]
        public double heightDecay;
        public bool isLarge;
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

        if (voronoiData.usePoissonDiscSampling)
        {
            GeneratePoissonPoints(seed, width, height, poissonAttempts, poissonRadius);
        }
        else
        {
            GenerateRandomPoints(seed, width, height, pointCount);
        }
        v = new Voronator(points, new Vector2(0, 0), new Vector2(width, height));
        for (int i = 0; i < relaxations; i++)
        {
            points = v.GetClippedRelaxedPoints();
            v = new Voronator(points, new Vector2(0, 0), new Vector2(width, height));
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
    public static void AddIsland(VoronoiData voronoiData)
    {

        foreach (Island islandData in voronoiData.islands)
        {
            if (islandData.startID > voronoiCells.Count - 1)
            {
                continue;
            }

            if (islandData.isLarge)
            {
                AddLargeIsland(islandData, voronoiData.seed);
            }
            else
            {
                AddSmallIsland(islandData);
            }
        }
    }
    private static void AddLargeIsland(Island islandData, int seed)
    {
        System.Random prng = new System.Random(seed);


        int startingCellIndex = islandData.startID;
        double height = islandData.height;
        double heightDecay = islandData.heightDecay;
        double sharpness = islandData.sharpness;
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
    private static void AddSmallIsland(Island islandData)
    {
        int startingCellIndex = islandData.startID;
        double height = islandData.height;
        double heightDecay = islandData.heightDecay;
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

    static void GenerateRandomPoints(int seed, float width, float height, int pointCount)
    {
        System.Random prng = new System.Random(seed);
        for (int i = 0; i < pointCount; i++)
        {
            float x, y;
            x = (float)prng.NextDouble() * width;
            y = (float)prng.NextDouble() * height;
            points.Add(new Vector2(x, y));
        }
    }
    static void GeneratePoissonPoints(int seed, float width, float height, int poissonAttempts, float poissonRadius)
    {
        points = PoissonDiscSampling.GeneratePoints(poissonRadius, new Vector2(width, height), poissonAttempts, seed);
    }
    static void StoreVoronoiVerticePoints()
    {
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

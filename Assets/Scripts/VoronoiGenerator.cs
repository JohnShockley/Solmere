using System.Collections.Generic;
using UnityEngine;
using System;
using VoronatorSharp;

public class VoronoiGenerator : MonoBehaviour
{
    public int seed;
    [System.Serializable]
    public struct DebugDisplay
    {
        public bool showBorder;
        public bool showTrianglePoints;
        public bool showHull;
        public bool showTriangleEdges;
        public bool showTriangleCenters;
        public bool showVoronoiVertices;
        public bool showVoronoiEdges;
    }

    public DebugDisplay debugDisplay;

    public float width;
    public float height;
    public bool autoUpdate;
    [Min(3)]
    public int pointCount = 100;
    [Range(0, .49f)]
    public float padding;

    public bool useCentroid;

    private List<Vector2> points = new List<Vector2>();
    private List<Vector3> hullEdgePoints = new List<Vector3>();
    private List<Vector3> borderEdgePoints = new List<Vector3>();
    private List<Vector3> triangleEdgePoints = new List<Vector3>();
    private List<Vector3> triangleCenters = new List<Vector3>();
    private List<Vector3> voronoiEdgeVerticePoints = new List<Vector3>();
    private List<Vector3> voronoiEdgePoints = new List<Vector3>();

    Voronator v;
    Delaunator d;

    void Start()
    {
        CreateVoronoi();
        d = v.Delaunator;
    }
    public void CreateVoronoi()
    {
        ClearData();
        GeneratePoints();
        v = new Voronator(points, new Vector2(0, 0), new Vector2(width, height));
        d = v.Delaunator;
        StoreData();
    }
    void ClearData()
    {
        points.Clear();
        hullEdgePoints.Clear();
        borderEdgePoints.Clear();
        triangleEdgePoints.Clear();
        triangleCenters.Clear();
        voronoiEdgeVerticePoints.Clear();
        voronoiEdgePoints.Clear();
    }

    void StoreData()
    {
        StoreHullEdgePoints();
        StoreBorderEdgePoints();
        StoreTriangleEdgePoints();
        StoreTriangleCenterPoints();
        StoreVoronoiVerticePoints();
        StoreVoronoiEdgePoints();
    }

    void GeneratePoints()
    {
        System.Random prng = new System.Random(seed);



        float widthPadding = padding * width; // 10% of the smaller dimension as padding
        float heightPadding = padding * height;

        for (int i = 0; i < pointCount; i++)
        {
            float x, y;


            x = UnityEngine.Random.Range(widthPadding, width - widthPadding);
            y = UnityEngine.Random.Range(heightPadding, height - heightPadding);


            points.Add(new Vector2(x, y));

        }
    }


    void StoreBorderEdgePoints()
    {
        borderEdgePoints = new List<Vector3>()
        {
            new Vector3(0, 0,0),
            new Vector3(0, 0,height),
            new Vector3(0, 0,height),
            new Vector3(width, 0,height),
            new Vector3(width, 0,height),
            new Vector3(width, 0,0),
            new Vector3(width, 0,0),
            new Vector3(0, 0,0),
        };
    }
    void StoreHullEdgePoints()
    {
        foreach (var edge in d.GetHullEdges())
        {
            hullEdgePoints.Add(new Vector3(edge.Item1.x, 0, edge.Item1.y));
            hullEdgePoints.Add(new Vector3(edge.Item2.x, 0, edge.Item2.y));
        }
    }

    void StoreTriangleEdgePoints()
    {
        foreach (var edge in d.GetEdges())
        {
            triangleEdgePoints.Add(new Vector3(edge.Item1.x, 0, edge.Item1.y));
            triangleEdgePoints.Add(new Vector3(edge.Item2.x, 0, edge.Item2.y));
        }
    }

    void StoreTriangleCenterPoints()
    {

        foreach (var t in d.GetTriangles())
        {

            if (useCentroid)
            {
                triangleCenters.Add(t.Centroid);
            }
            else
            {
                triangleCenters.Add(t.Circumcenter);
            }
        }
    }


    void StoreVoronoiVerticePoints()
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
    void StoreVoronoiEdgePoints()
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


    void OnDrawGizmos()
    {
        if (debugDisplay.showTrianglePoints)
        {

            Gizmos.color = Color.red;
            foreach (var point in points)
            {
                Gizmos.DrawSphere(new Vector3(point.x, 0, point.y), 0.3f);
            }
        }

        if (debugDisplay.showBorder)
        {
            Gizmos.color = Color.black;
            for (int i = 0; i < borderEdgePoints.Count; i += 2)
            {
                Gizmos.DrawLine(borderEdgePoints[i], borderEdgePoints[i + 1]);
            }
        }

        if (debugDisplay.showTriangleEdges)
        {

            Gizmos.color = Color.black;
            for (int i = 0; i < triangleEdgePoints.Count; i += 2)
            {
                Gizmos.DrawLine(triangleEdgePoints[i], triangleEdgePoints[i + 1]);
            }
        }

        if (debugDisplay.showHull)
        {

            Gizmos.color = Color.black;
            for (int i = 0; i < hullEdgePoints.Count; i += 2)
            {
                Gizmos.DrawLine(hullEdgePoints[i], hullEdgePoints[i + 1]);
            }
        }

        if (debugDisplay.showTriangleCenters)
        {
            Gizmos.color = Color.blue;
            foreach (var point in triangleCenters)
            {
                Gizmos.DrawSphere(new Vector3(point.x, 0, point.y), 0.3f);
            }
        }
        if (debugDisplay.showVoronoiVertices)
        {
            Gizmos.color = Color.cyan;
            foreach (var point in voronoiEdgeVerticePoints)
            {
                Gizmos.DrawSphere(new Vector3(point.x, 0, point.y), 0.3f);
            }
        }
        if (debugDisplay.showVoronoiEdges)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < voronoiEdgePoints.Count; i += 2)
            {
                Gizmos.DrawLine(voronoiEdgePoints[i], voronoiEdgePoints[i + 1]);
            }
        }

    }

}

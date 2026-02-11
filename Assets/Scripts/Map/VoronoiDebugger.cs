using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static Voronoi;

[ExecuteInEditMode]
public class VoronoiDebugger : MonoBehaviour
{
    [Header("Gizmo Settings")]
    public bool drawPoints = true;
    public bool drawEdges = true;
    public bool drawCells = true;
    public bool drawCellIDs = false;
    public bool drawUnclippedCells = true;
    public bool drawClippingBoundary = true;
    public bool drawNeighborConnections = false;
    public bool drawCircumcenters = true;

    [Header("Colors")]
    public Color pointColor = Color.yellow;
    public Color edgeColor = Color.cyan;
    public Color cellColor = new Color(0, 1, 0, 0.15f);
    public Color unclippedColor = new Color(1, 0, 1, 0.5f);
    public Color boundaryColor = Color.white;
    public Color neighborColor = new Color(1, 0.5f, 0, 0.3f);
    public Color circumcenterColor = Color.red;

    [Header("Size Settings")]
    public float pointSize = 0.2f;
    public float lineThickness = 2f;
    public float circumcenterSize = 0.15f;

    public List<List<VoronoiCell>> voronoiLayers = new List<List<VoronoiCell>>();

    private void OnDrawGizmos()
    {
        var cells = GetVoronoiCells();
        if (cells == null) return;

        // Draw unclipped cells first (if enabled)
        if (drawUnclippedCells && Voronoi.v != null)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var unclippedPoly = Voronoi.v.GetPolygon(i);
                if (unclippedPoly != null && unclippedPoly.Count > 1)
                {
                    Gizmos.color = unclippedColor;
                    for (int j = 0; j < unclippedPoly.Count; j++)
                    {
                        Vector3 from = new Vector3(unclippedPoly[j].x, 0, unclippedPoly[j].y);
                        Vector3 to = new Vector3(unclippedPoly[(j + 1) % unclippedPoly.Count].x, 0, unclippedPoly[(j + 1) % unclippedPoly.Count].y);
                        Gizmos.DrawLine(from, to);
                    }
                }
            }
        }

        // Draw clipped cells
        if (drawCells)
        {
            Gizmos.color = cellColor;
            DrawVoronoiCells(cells);
        }

        // Draw edges
        if (drawEdges)
        {
            DrawEdges();
        }

        // Draw points
        if (drawPoints)
        {
            DrawPoints(cells);
        }

        // Draw neighbor connections
        if (drawNeighborConnections)
        {
            Gizmos.color = neighborColor;
            foreach (var cell in cells)
            {
                Vector3 from = new Vector3(cell.coordinate.x, 0, cell.coordinate.y);
                foreach (var neighborId in cell.neighborIDs)
                {
                    var neighbor = cells[neighborId];
                    Vector3 to = new Vector3(neighbor.coordinate.x, 0, neighbor.coordinate.y);
                    Gizmos.DrawLine(from, to);
                }
            }
        }

        // Draw circumcenters
        if (drawCircumcenters && Voronoi.v != null)
        {
            Gizmos.color = circumcenterColor;
            var circumcenters = Voronoi.v.TriangleVertices;
            foreach (var center in circumcenters)
            {
                Gizmos.DrawSphere(new Vector3(center.x, 0, center.y), circumcenterSize);
            }
        }

        // Draw cell IDs
        if (drawCellIDs)
        {
            DrawIDs(cells);
        }

        // Draw clipping boundary
        if (drawClippingBoundary && Voronoi.v != null)
        {
            Gizmos.color = boundaryColor;
            // Draw the bounds as lines
            var bounds = GetVoronoiBounds();
            if (bounds.Count > 0)
            {
                for (int i = 0; i < bounds.Count; i++)
                {
                    Vector3 from = new Vector3(bounds[i].x, 0, bounds[i].y);
                    Vector3 to = new Vector3(bounds[(i + 1) % bounds.Count].x, 0, bounds[(i + 1) % bounds.Count].y);
                    Gizmos.DrawLine(from, to);
                }
            }
        }
    }

    private List<Voronoi.VoronoiCell> GetVoronoiCells()
    {
        // Access the static list in your Voronoi class
        var field = typeof(Voronoi)
            .GetField("voronoiCells", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        return field?.GetValue(null) as List<Voronoi.VoronoiCell>;
    }

    private void DrawPoints(List<Voronoi.VoronoiCell> cells)
    {
        Gizmos.color = pointColor;
        foreach (var cell in cells)
        {
            Gizmos.DrawSphere(new Vector3(cell.coordinate.x, 0, cell.coordinate.y), pointSize);
        }
    }

    private void DrawEdges()
    {
        var field = typeof(Voronoi)
            .GetField("voronoiEdgePoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var edges = field?.GetValue(null) as List<Vector3>;
        if (edges == null) return;

        Gizmos.color = edgeColor;
        for (int i = 0; i < edges.Count; i += 2)
        {
            if (i + 1 < edges.Count)
                Gizmos.DrawLine(edges[i], edges[i + 1]);
        }
    }

    private void DrawVoronoiCells(List<Voronoi.VoronoiCell> cells)
    {
        foreach (var cell in cells)
        {
            if (cell.vertices == null || cell.vertices.Count < 3) continue;



            Color drawColor = cellColor;
            if (cell.ControllingFactionId != -1)
            {
                var worldManager = FindFirstObjectByType<WorldManager>();
                if (worldManager != null)
                {
                    var faction = worldManager.GetFactionById(cell.ControllingFactionId);
                    if (faction != null)
                    {
                        drawColor = faction.Color;
                    }
                }
            }
            Gizmos.color = drawColor;
            // Draw cell boundaries
            for (int i = 0; i < cell.vertices.Count; i++)
            {
                var a = new Vector3(cell.vertices[i].x, 0, cell.vertices[i].y);
                var b = new Vector3(cell.vertices[(i + 1) % cell.vertices.Count].x, 0, cell.vertices[(i + 1) % cell.vertices.Count].y);
                Gizmos.DrawLine(a, b);
            }
        }
    }

    private void DrawIDs(List<Voronoi.VoronoiCell> cells)
    {
#if UNITY_EDITOR
        foreach (var cell in cells)
        {
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(new Vector3(cell.coordinate.x, 0, cell.coordinate.y), cell.id.ToString());
        }
#endif
    }

    private List<Vector2> GetVoronoiBounds()
    {
        var field = typeof(Voronoi)
            .GetField("points", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var points = field?.GetValue(null) as List<Vector2>;
        if (points == null || points.Count == 0) return new List<Vector2>();

        // Create a bounding box around all points
        var minX = points.Min(p => p.x);
        var maxX = points.Max(p => p.x);
        var minY = points.Min(p => p.y);
        var maxY = points.Max(p => p.y);

        // Add a small margin
        var margin = Mathf.Max(maxX - minX, maxY - minY) * 0.1f;
        minX -= margin;
        maxX += margin;
        minY -= margin;
        maxY += margin;

        return new List<Vector2>
        {
            new Vector2(minX, minY),
            new Vector2(maxX, minY),
            new Vector2(maxX, maxY),
            new Vector2(minX, maxY)
        };
    }
}

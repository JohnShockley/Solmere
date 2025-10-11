using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class VoronoiDebugger : MonoBehaviour
{
    [Header("Gizmo Settings")]
    public bool drawPoints = true;
    public bool drawEdges = true;
    public bool drawCells = false;
    public bool drawCellIDs = false;
    public Color pointColor = Color.yellow;
    public Color edgeColor = Color.cyan;
    public Color cellColor = new Color(0, 1, 0, 0.15f);

    private void OnDrawGizmos()
    {
        // Early-out if no data yet
        var cells = GetVoronoiCells();
        if (cells == null || cells.Count == 0)
            return;

        if (drawCells)
            DrawVoronoiCells(cells);

        if (drawPoints)
            DrawPoints(cells);

        if (drawEdges)
            DrawEdges();

        if (drawCellIDs)
            DrawIDs(cells);
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
            Gizmos.DrawSphere(new Vector3(cell.coordinate.x, 0, cell.coordinate.y), 0.2f);
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

            Gizmos.color = cellColor;

            // Fill-ish polygon by drawing lines between consecutive vertices
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
}

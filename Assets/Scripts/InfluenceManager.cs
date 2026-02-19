using System.Collections.Generic;
using UnityEngine;

using static Voronoi;
using System.Linq;

public static class InfluenceManager
{
    private static List<VoronoiCell> allCells;
    private static Dictionary<int, Faction> factionsById;

    public static void Initialize(List<VoronoiCell> cells, List<Faction> factions)
    {
        allCells = cells;
        factionsById = factions.ToDictionary(f => f.Id, f => f);
    }

    public static void RecalculateAll()
    {
        Debug.Log("Recalculating influence and control...");
        ClearInfluence();
        ApplyBuildingInfluence();
        EvaluateControl();
        UpdateFactionPower();
    }
    private static void ClearInfluence()
    {
        foreach (var cell in allCells)
        {
            cell.InfluenceByFaction.Clear();
        }
    }
    private static void ApplyBuildingInfluence()
    {
        foreach (var cell in allCells)
        {
            foreach (var building in cell.Buildings)
            {
                AddInfluence(cell, building.OwnerFactionId, building.BaseInfluence);

                if (building.InfluenceRadius > 0)
                {
                    SpreadToNeighbors(cell, building);
                }
            }
        }
    }
    private static void AddInfluence(VoronoiCell cell, int factionId, float amount)
    {
        if (!cell.InfluenceByFaction.ContainsKey(factionId))
            cell.InfluenceByFaction[factionId] = 0f;

        cell.InfluenceByFaction[factionId] += amount;
    }

    private static void SpreadToNeighbors(VoronoiCell origin, Building building)
    {
        int neighborCount = origin.Neighbors.Count;

        if (neighborCount == 0)
            return;

        float totalSpread = building.BaseInfluence * building.NeighborInfluencePercent;
        float perNeighbor = totalSpread / neighborCount;

        foreach (var neighbor in origin.Neighbors)
        {
            AddInfluence(neighbor, building.OwnerFactionId, perNeighbor);
        }
    }

    private static void EvaluateControl()
    {
        foreach (var cell in allCells)
        {
            int newController = -1;
            float highestInfluence = 0f;

            foreach (var pair in cell.InfluenceByFaction)
            {
                float percent = pair.Value / cell.RequiredInfluence;

                if (percent >= 0.7f && pair.Value > highestInfluence)
                {
                    highestInfluence = pair.Value;
                    newController = pair.Key;
                }
            }

            cell.ControllingFactionId = newController;
        }
    }

    private static void UpdateFactionPower()
    {
        foreach (int key in factionsById.Keys)
        {
            factionsById[key].ControlledCellCount = 0;
        }

        foreach (var cell in allCells)
        {
            if (cell.ControllingFactionId >= 0)
            {
                var faction = factionsById[cell.ControllingFactionId];
                faction.ControlledCellCount++;
            }
        }
    }
}

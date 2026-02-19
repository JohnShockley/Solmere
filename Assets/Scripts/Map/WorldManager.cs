using UnityEngine;
using System.Collections.Generic;
using static Voronoi;
using static InfluenceManager;
using System.Linq;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private VoronoiData data;

    public List<VoronoiCell> WorldCells { get; private set; }
    private List<Faction> factions = new List<Faction>();


    [SerializeField] private float testClaimInterval = 2f;
    private float testTimer;

   

    void Start()
    {
        GenerateWorld();
        


    }
    void GenerateWorld()
    {
        List<Vector2> square = new List<Vector2>
{
    new Vector2(0, 0),
    new Vector2(1000, 0),
    new Vector2(1000, 1000),
    new Vector2(0, 1000)
};



        WorldCells = CreateVoronoi(data, square);

        CreateFactions();
        InfluenceManager.Initialize(WorldCells, factions);
        AssignStartingCells();
        
    }

    void CreateFactions()
    {
        factions.Clear();

        factions.Add(new Faction(0, "Red Kingdom", Color.red));
        factions.Add(new Faction(1, "Blue Kingdom", Color.blue));
    }
    public Faction GetFactionById(int id)
    {
        return factions.Find(f => f.Id == id);
    }

    public void UpdateFactionPower(Faction faction)
    {
        // For MVP: 1 cell = 1 power
        faction.Power = faction.ControlledCellIds.Count;

        // Debug display
        Debug.Log($"{faction.Name} has {faction.Power} power.");
    }


    void AssignStartingCells()
    {
        foreach (var faction in factions)
        {

            AssignStartingCell(faction);
        }
    }

    void AssignStartingCell(Faction faction)
    {
        int randomIndex = UnityEngine.Random.Range(0, WorldCells.Count);

        var cell = WorldCells[randomIndex];

        // Make sure it's not already taken
        if (cell.ControllingFactionId != -1)
        {
            AssignStartingCell(faction); // retry
            return;
        }

        cell.ControllingFactionId = faction.Id;
        faction.ControlledCellIds.Add(cell.id);

        AddDebugBuilding(cell, faction.Id, 210f, 0.34f);
        Debug.Log($"{faction.Name} assigned cell {cell.id}");

    }

    public void ClaimCell(int cellId, int factionId)
    {
        var cell = WorldCells[cellId];

        if (cell.ControllingFactionId == factionId)
            return;

        if (cell.ControllingFactionId != -1)
        {
            var oldFaction = GetFactionById(cell.ControllingFactionId);
            if (oldFaction != null)
            {
                oldFaction.ControlledCellIds.Remove(cell.id);
                UpdateFactionPower(oldFaction);
            }
        }

        cell.ControllingFactionId = factionId;

        var newFaction = GetFactionById(factionId);
        if (newFaction != null)
        {
            newFaction.ControlledCellIds.Add(cell.id);
            UpdateFactionPower(newFaction);
        }

        Debug.Log($"Cell {cellId} claimed by faction {factionId}");
    }



    public List<int> GetClaimableNeighborCells(Faction faction)
    {
        HashSet<int> claimable = new HashSet<int>();

        foreach (var cellId in faction.ControlledCellIds)
        {
            var cell = WorldCells[cellId];

            foreach (var neighborId in cell.neighborIDs)
            {
                var neighbor = WorldCells[neighborId];

                // Can claim if neutral or owned by another faction
                if (neighbor.ControllingFactionId != faction.Id)
                {
                    claimable.Add(neighborId);
                }
            }
        }

        return claimable.ToList();
    }


    public static void AddDebugBuilding(VoronoiCell cell, int factionId, float baseInfluence, float spreadPercent)
    {
        var building = new Building
        {
            OwnerFactionId = factionId,
            BaseInfluence = baseInfluence,
            NeighborInfluencePercent = spreadPercent,
            InfluenceRadius = spreadPercent > 0 ? 1 : 0
        };

        cell.Buildings.Add(building);

        InfluenceManager.RecalculateAll();
    }

}

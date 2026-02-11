using UnityEngine;
using System.Collections.Generic;
using static Voronoi;
using System.Linq;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private VoronoiData data;

    public List<VoronoiCell> WorldCells { get; private set; }
    private List<Faction> factions = new List<Faction>();


    [SerializeField] private float testClaimInterval = 2f;
    private float testTimer;

    void Update()
    {
        testTimer += Time.deltaTime;

        if (testTimer >= testClaimInterval)
        {
            testTimer = 0f;

            int randomCell = UnityEngine.Random.Range(0, WorldCells.Count);
            int randomFaction = UnityEngine.Random.Range(0, factions.Count);

            ClaimCell(randomCell, factions[randomFaction].Id);
        }
    }

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
        Debug.Log($"{faction.Name} assigned cell {cell.id}");

    }

    public void ClaimCell(int cellId, int factionId)
    {
        var cell = WorldCells[cellId];

        // If already owned by this faction, nothing changes
        if (cell.ControllingFactionId == factionId)
            return;

        // Remove from previous owner
        if (cell.ControllingFactionId != -1)
        {
            var oldFaction = GetFactionById(cell.ControllingFactionId);
            if (oldFaction != null)
            {
                oldFaction.ControlledCellIds.Remove(cell.id);
            }
        }

        // Assign to new owner
        cell.ControllingFactionId = factionId;

        var newFaction = GetFactionById(factionId);
        if (newFaction != null)
        {
            newFaction.ControlledCellIds.Add(cell.id);
        }

        Debug.Log($"Cell {cellId} claimed by faction {factionId}");
    }




}

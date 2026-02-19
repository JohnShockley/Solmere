using System.Collections.Generic;
using UnityEngine;

public class Faction
{
    public int Id;
    public string Name;
    public Color Color;
    public int Power;

    public List<int> ControlledCellIds = new List<int>();
    public int ControlledCellCount;


    public Faction(int id, string name, Color color)
    {
        Id = id;
        Name = name;
        Color = color;
    }
}

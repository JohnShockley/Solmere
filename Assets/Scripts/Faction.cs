using System.Collections.Generic;
using UnityEngine;

public class Faction
{
    public int Id;
    public string Name;
    public Color Color;

    public List<int> ControlledCellIds = new List<int>();

    public Faction(int id, string name, Color color)
    {
        Id = id;
        Name = name;
        Color = color;
    }
}

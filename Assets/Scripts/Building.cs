
public class Building
{
    public int OwnerFactionId;

    public float BaseInfluence;                // Influence in own cell
    public float NeighborInfluencePercent;     // 0.3f = 30%
    public int InfluenceRadius;                // For MVP: 0 or 1
}

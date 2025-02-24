using UnityEngine;
[CreateAssetMenu()]
public class TerrainData : UpdatableData
{
    public float uniformScale = 1f;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
}

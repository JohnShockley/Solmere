using UnityEngine;
using static MapGenerator;
[CreateAssetMenu()]
public class NoiseData : UpdatableData
{


    public NoiseType noiseType;
    public Noise.NormalizeMode normalizeMode;

    public bool useFalloff;
    public float a;
    public float b;

    [Min(1)]
    public float noiseScale;
    [Range(1, 10)]
    public int octaves;
    [Range(0f, 1f)]
    public float persistence;
    [Min(1)]
    public float lacunarity;
    [Range(0f, .5f)]
    public float muteness;
    public int seed;
    public Vector2 offset;
    [Range(0.001f, 1f)]
    public float stepSize;
    [Range(0f, 1f)]
    public float verticalOffset;
    [Range(0f, 4f)]
    public float power;
}

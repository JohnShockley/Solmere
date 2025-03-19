using System;
using System.Collections.Generic;
using UnityEngine;
using static Voronoi;
[CreateAssetMenu()]
public class VoronoiData : UpdatableData
{

    public int seed;

    [Min(3)]
    public int pointCount = 100;
    [Range(0, 20)]
    public int relaxations;
    public bool usePoissonDiscSampling;
    [Min(1f)]
    public float poissonRadius;
    [Range(1, 20)]
    public int poissonAttempts;
    [SerializeField]
    public List<Island> islands;
}

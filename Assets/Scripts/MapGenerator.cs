using System.Security.Cryptography.X509Certificates;
using Unity.Mathematics;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static MeshGenerator;

public class MapGenerator : MonoBehaviour
{

    public enum NoiseType { Perlin, Simplex, PRidge, SRidge, RV, Combined }
    public enum DrawMode { NoiseMap, ColorMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public TerrainData terrainData;
    public NoiseData noiseData;
    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int editorPreviewLOD;



    public bool autoUpdate;

    public bool combineNoiseData;
    public NoiseData continentalnessNoiseData;
    public NoiseData erosionNoiseData;
    public NoiseData pvNoiseData;

    public AnimationCurve continentalnessCurve;
    public AnimationCurve erosionCurve;
    public AnimationCurve PVCurve;



    public TerrainType[] regions;

    float[,] falloffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();



    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }
    void Awake()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize,noiseData.a,noiseData.b);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindAnyObjectByType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize,noiseData.a,noiseData.b)));
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }
    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 center)
    {


        float[,] noiseMap = new float[mapChunkSize, mapChunkSize];
        if (combineNoiseData)
        {


            float[,] continentalnessNoiseMap = Noise.GenerateNoiseMap(continentalnessNoiseData, mapChunkSize);
            float[,] erosionNoiseMap = Noise.GenerateNoiseMap(erosionNoiseData, mapChunkSize);
            float[,] pvNoiseMap = Noise.GenerateNoiseMap(pvNoiseData, mapChunkSize);

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;

            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {

                    float continentalness = continentalnessNoiseMap[x, y] * -1 + 1;
                    float erosion = erosionNoiseMap[x, y];
                    float pv = pvNoiseMap[x, y];

                    float modifiedPV = PVCurve.Evaluate(pv);
                    float modifiedErosion = erosionCurve.Evaluate(erosion);
                    float finalHeight = continentalnessCurve.Evaluate(continentalness);
                    if (finalHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = finalHeight;
                    }
                    else if (finalHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = finalHeight;
                    }
                    noiseMap[x, y] = finalHeight;
                }
            }
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
            }
        }
        else
        {
            noiseMap = Noise.GenerateNoiseMap(noiseData, mapChunkSize);
        }

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (noiseData.useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                }
                float currentHeight = noiseMap[x, y];


                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    }
                    else
                    {
                        break;

                    }
                }

            }
        }




        return new MapData(noiseMap, colorMap);

    }

    void OnValidate()
    {

        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize,noiseData.a,noiseData.b);
        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;

        }
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;

        }
        if (continentalnessNoiseData != null)
        {
            continentalnessNoiseData.OnValuesUpdated -= OnValuesUpdated;
            continentalnessNoiseData.OnValuesUpdated += OnValuesUpdated;

        }
        if (erosionNoiseData != null)
        {
            erosionNoiseData.OnValuesUpdated -= OnValuesUpdated;
            erosionNoiseData.OnValuesUpdated += OnValuesUpdated;

        }
        if (pvNoiseData != null)
        {
            pvNoiseData.OnValuesUpdated -= OnValuesUpdated;
            pvNoiseData.OnValuesUpdated += OnValuesUpdated;

        }
    }



    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
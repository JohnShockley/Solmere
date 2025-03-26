using System.Security.Cryptography.X509Certificates;
using Unity.Mathematics;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static MeshGenerator;
using static Voronoi;

public class MapGenerator : MonoBehaviour
{

    public enum NoiseType { Perlin, Simplex, PRidge, SRidge, RV, Combined };
    public enum DrawMode { NoiseMap, ColorMap, Mesh, FalloffMap, Voronoi };
    public DrawMode drawMode;

    public TerrainData terrainData;
    public NoiseData noiseData;
    public VoronoiData voronoiData;
    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int editorPreviewLOD;



    public bool autoUpdate;



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
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, noiseData.a, noiseData.b);
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
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize, noiseData.a, noiseData.b)));
        }
        else if (drawMode == DrawMode.Voronoi)
        {
            display.DrawTexture(TextureGenerator.TextureFromVoronoi(CreateVoronoi(voronoiData, mapChunkSize, mapChunkSize), mapChunkSize));
        }


    }
    public static float[,] HeightFromVoronoi(List<VoronoiCell> cells, int sideLength)
    {
        float[,] heightMap = new float[sideLength, sideLength];

        // Loop through each pixel in the texture
        for (int y = 0; y < sideLength; y++)
        {
            for (int x = 0; x < sideLength; x++)
            {
                Vector2 pixelPos = new Vector2(x, y);

                int voronoiIndex = Find(pixelPos);

                float height = (float)cells[voronoiIndex].height;
                heightMap[x,y] = height;

            }

        }

    
        return heightMap;
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

        noiseMap = Noise.GenerateNoiseMap(noiseData, mapChunkSize);


        noiseMap = HeightFromVoronoi(CreateVoronoi(voronoiData, mapChunkSize, mapChunkSize), mapChunkSize);

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

        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize, noiseData.a, noiseData.b);
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
        if (voronoiData != null)
        {
            voronoiData.OnValuesUpdated -= OnValuesUpdated;
            voronoiData.OnValuesUpdated += OnValuesUpdated;
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
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    Color[] colors;
    public int seed;
    public int xSize = 20;
    public int zSize = 20;


    public float scale = 20;
    public int octaves = 4;
    public float persistence = .5f;
    public float lacunarity = 2;

    public float xOffset;
    public float zOffset;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

    }

    void Update()
    {
        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + xOffset;
            float offsetZ = prng.Next(-100000, 100000) + zOffset;
            octaveOffsets[i] = new Vector2(offsetX, offsetZ);
        }



        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        colors = new Color[vertices.Length];


        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfX = xSize / 2f;
        float halfZ = zSize / 2f;

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int o = 0; o < octaves; o++)
                {




                    float xCoord = (x - halfX) / scale * frequency + octaveOffsets[o].x * frequency;
                    float zCoord = (z - halfZ) / scale * frequency + octaveOffsets[o].y * frequency;

                    float y = Mathf.PerlinNoise(xCoord, zCoord) * 2 - 1;
                    noiseHeight += y * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                vertices[i] = new Vector3(x, noiseHeight, z);




                i++;
            }
        }


        for (int i = 0; i < vertices.Length; i++)
        {

            float lerpedNoiseHeight = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, vertices[i].y);
            colors[i] = new Color(lerpedNoiseHeight, lerpedNoiseHeight, lerpedNoiseHeight);


        }







        triangles = new int[xSize * zSize * 6];
        int vert = 0;
        int tris = 0;


        for (int z = 0; z < zSize; z++)
        {



            for (int x = 0; x < xSize; x++)
            {
                triangles[tris] = vert;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
                vert++;
                tris += 6;
            }
            vert++;
        }


    }
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();

    }



    // private void OnDrawGizmos()
    // {
    //     if (vertices == null)
    //     {
    //         return;
    //     }

    //     for (int i = 0; i < vertices.Length; i++)
    //     {
    //         Gizmos.DrawSphere(vertices[i], .1f);
    //     }
    // }
}

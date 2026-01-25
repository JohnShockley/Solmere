using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using static MapGenerator;

public static class Noise
{

    public enum NormalizeMode { local, Global }
    public static float[,] GenerateNoiseMap(NoiseData noiseData, int dimension)
    {
        return GenerateNoise(noiseData, dimension);
    }


    private static float[,] GenerateNoise(NoiseData noiseData, int dimension)
    {
        float persistence = noiseData.persistence;
        float lacunarity = noiseData.lacunarity;
        int seed = noiseData.seed;
        int octaves = noiseData.octaves;
        Vector2 offset = noiseData.offset;
        float scale = noiseData.noiseScale;
        NoiseType noiseType = noiseData.noiseType;
        NormalizeMode normalizeMode = noiseData.normalizeMode;
        float stepSize = noiseData.stepSize;
        float muteness = noiseData.muteness;
        float power = noiseData.power;






        float[,] noiseMap = new float[dimension, dimension];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfX = dimension / 2f;
        float halfY = dimension / 2f;

        for (int y = 0; y < dimension; y++)
        {
            for (int x = 0; x < dimension; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 1;
                float weight = 1;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfX + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfY + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    float simplexValue = SimplexNoise.SimplexValue(new Vector2(sampleX, sampleY));

                    switch (noiseType)
                    {
                        case NoiseType.Simplex:
                            noiseHeight += simplexValue * amplitude;
                            break;
                        case NoiseType.PRidge:
                            perlinValue = 1 - Mathf.Abs(perlinValue);
                            perlinValue *= perlinValue * weight;
                            weight = perlinValue;
                            noiseHeight += perlinValue * amplitude;
                            break;
                        case NoiseType.SRidge:
                            simplexValue = 1 - Mathf.Abs(simplexValue);
                            simplexValue *= simplexValue * weight;
                            weight = simplexValue;
                            noiseHeight += simplexValue * amplitude;
                            break;
                        case NoiseType.RV:
                            perlinValue = 1 - Mathf.Abs((3 * Mathf.Abs(perlinValue)) - 2); // Ridge effect
                            weight = Mathf.Abs(perlinValue);
                            perlinValue *= weight; // Maintain smooth blending

                            noiseHeight += perlinValue * amplitude;
                            break;
                        case NoiseType.Combined:
                            perlinValue = 1f - Mathf.Abs((3 * Mathf.Abs(perlinValue)) - 2) + perlinValue;
                            //perlinValue = perlinValue * perlinValue;
                            break;
                        default:

                            noiseHeight += perlinValue * amplitude;
                            break;
                    }
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                noiseHeight = Mathf.Round(noiseHeight / stepSize) * stepSize;

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }



                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < dimension; y++)
        {
            for (int x = 0; x < dimension; x++)
            {

                if (normalizeMode == NormalizeMode.local)
                {
                    noiseMap[x, y] = Mathf.Pow(noiseMap[x, y], power);
                    if (muteness > 0f)
                    {
                        noiseMap[x, y] = Mathf.Lerp(noiseMap[x, y], .5f, muteness);
                    }
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }

            }
        }
        return noiseMap;
    }

    public static class SimplexNoise
    {
        private static readonly Vector4 C = new Vector4(
            0.211324865405187f, // (3.0 - sqrt(3.0)) / 6.0
            0.366025403784439f, // 0.5 * (sqrt(3.0) - 1.0)
            -0.577350269189626f, // -1.0 + 2.0 * C.x
            0.024390243902439f  // 1.0 / 41.0
        );

        private static float Mod289(float x)
        {
            return x - Mathf.Floor(x * (1.0f / 289.0f)) * 289.0f;
        }

        private static Vector3 Mod289(Vector3 x)
        {
            return new Vector3(
                Mod289(x.x),
                Mod289(x.y),
                Mod289(x.z)
            );
        }

        private static Vector3 Permute(Vector3 x)
        {
            return Mod289(Vector3.Scale((x * 34.0f) + Vector3.one, x));
        }

        public static float SimplexValue(Vector2 v)
        {
            // Skew input space to get a triangular grid
            float s = (v.x + v.y) * C.y;
            Vector2 i = new Vector2(Mathf.Floor(v.x + s), Mathf.Floor(v.y + s));

            float t = (i.x + i.y) * C.x;
            Vector2 x0 = v - i + new Vector2(t, t);

            // Determine simplex corner offsets
            Vector2 i1 = (x0.x > x0.y) ? new Vector2(1.0f, 0.0f) : new Vector2(0.0f, 1.0f);
            Vector2 x1 = x0 - i1 + new Vector2(C.x, C.x);
            Vector2 x2 = x0 + new Vector2(C.z, C.z);

            // Permutation
            i = new Vector2(Mod289(i.x), Mod289(i.y));
            Vector3 p = Permute(Permute(new Vector3(i.y, i1.y + i.y, 1.0f + i.y))
                              + new Vector3(i.x, i1.x + i.x, 1.0f + i.x));

            // Compute gradients and contributions
            Vector3 m = Vector3.Max(Vector3.zero, new Vector3(
                0.5f - Vector2.Dot(x0, x0),
                0.5f - Vector2.Dot(x1, x1),
                0.5f - Vector2.Dot(x2, x2)
            ));
            m = Vector3.Scale(m, m);
            m = Vector3.Scale(m, m);

            // Gradients
            Vector3 x = 2.0f * (p * C.w - new Vector3(Mathf.Floor(p.x * C.w), Mathf.Floor(p.y * C.w), Mathf.Floor(p.z * C.w))) - Vector3.one;
            Vector3 h = new Vector3(Mathf.Abs(x.x), Mathf.Abs(x.y), Mathf.Abs(x.z)) - new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 ox = new Vector3(Mathf.Floor(x.x + 0.5f), Mathf.Floor(x.y + 0.5f), Mathf.Floor(x.z + 0.5f));
            Vector3 a0 = x - ox;

            // Normalize gradients
            m = Vector3.Scale(m, new Vector3(
                1.79284291400159f - 0.85373472095314f * (a0.x * a0.x + h.x * h.x),
                1.79284291400159f - 0.85373472095314f * (a0.y * a0.y + h.y * h.y),
                1.79284291400159f - 0.85373472095314f * (a0.z * a0.z + h.z * h.z)
            ));

            // Compute final noise value
            Vector3 g = new Vector3(
                a0.x * x0.x + h.x * x0.y,
                a0.y * x1.x + h.y * x1.y,
                a0.z * x2.x + h.z * x2.y
            );

            return 130.0f * Vector3.Dot(m, g);
        }
    }
    private static float Ridge(float h, float offset)
    {
        h = Mathf.Abs(h);      // create creases
        h = offset - h;        // invert so creases are at top
        h *= h;             // sharpen creases
        return h;
    }
    private static float RidgedMF(float noise, int octaves, float lacunarity, float gain, float offset)
    {
        float sum = 0.0f;
        float freq = 1.0f;
        float amp = 1f;
        float prev = 1.0f;

        for (int i = 0; i < octaves; i++)
        {
            //SimplexNoise.Noise(p * freq)
            float n = Ridge(noise, offset);
            sum += n * amp;
            sum += n * amp * prev;  // scale by previous octave
            prev = n;

            freq *= lacunarity;
            amp *= gain;
        }

        return sum;
    }

    static void ApplyBoxBlur(float[,] noiseMap, int width, int height, int blurLength)
    {
        float[,] tempMap = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float sum = 0f;
                int count = 0;

                // Average over a (2*blurRadius + 1) x (2*blurRadius + 1) region
                for (int dx = -blurLength; dx <= blurLength; dx++)
                {
                    for (int dy = -blurLength; dy <= blurLength; dy++)
                    {
                        int nx = Mathf.Clamp(x + dx, 0, width - 1);
                        int ny = Mathf.Clamp(y + dy, 0, height - 1);

                        sum += noiseMap[nx, ny];
                        count++;
                    }
                }

                tempMap[x, y] = sum / count; // Store averaged value
            }
        }

        // Copy blurred result back to the original map
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseMap[x, y] = tempMap[x, y];
            }
        }
    }

}

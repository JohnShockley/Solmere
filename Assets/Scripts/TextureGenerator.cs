using System.Collections.Generic;
using UnityEngine;
using VoronatorSharp;
using static Voronoi;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colorMap, int sideLength)
    {
        Texture2D texture = new Texture2D(sideLength, sideLength);
        // texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromVoronoi(List<VoronoiCell> cells, int sideLength)
    {
        Texture2D texture = new Texture2D(sideLength, sideLength);
        //texture.filterMode = FilterMode.Point;
        Color[] colorMap = new Color[sideLength * sideLength];



        // Loop through each pixel in the texture
        for (int y = 0; y < sideLength; y++)
        {
            for (int x = 0; x < sideLength; x++)
            {
                Vector2 pixelPos = new Vector2(x, y);

                // Assign color based on the closest Voronoi cell
                int index = y * sideLength + x;


                int voronoiIndex = Find(pixelPos);

                float height = (float)cells[voronoiIndex].height;
                colorMap[index] = HeightToColor(height);

            }

        }

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }
public static Color HeightToColor(float height)
{
    if (height >= 0.85f)
    {
        return Color.Lerp(
            new Color(181 / 255f, 26 / 255f, 71 / 255f),   // #b51a47
            new Color(158 / 255f, 1 / 255f, 66 / 255f),    // #9e0142
            Mathf.InverseLerp(0.85f, 1f, height)
        );
    }
    else if (height >= 0.7f)
    {
        return Color.Lerp(
            new Color(239 / 255f, 108 / 255f, 73 / 255f),  // #ef6c49
            new Color(181 / 255f, 26 / 255f, 71 / 255f),   // #b51a47
            Mathf.InverseLerp(0.7f, 0.85f, height)
        );
    }
    else if (height >= 0.55f)
    {
        return Color.Lerp(
            new Color(253 / 255f, 198 / 255f, 118 / 255f), // #fdc676
            new Color(239 / 255f, 108 / 255f, 73 / 255f),  // #ef6c49
            Mathf.InverseLerp(0.55f, 0.7f, height)
        );
    }
    else if (height >= 0.4f)
    {
        return Color.Lerp(
            new Color(254 / 255f, 222 / 255f, 142 / 255f), // #fede8e
            new Color(253 / 255f, 198 / 255f, 118 / 255f), // #fdc676
            Mathf.InverseLerp(0.4f, 0.55f, height)
        );
    }
    else if (height >= 0.3f)
    {
        return Color.Lerp(
            new Color(254 / 255f, 239 / 255f, 164 / 255f), // #feefa4
            new Color(254 / 255f, 222 / 255f, 142 / 255f), // #fede8e
            Mathf.InverseLerp(0.3f, 0.4f, height)
        );
    }
    else if (height >= 0.2f)
    {
        return Color.Lerp(
            new Color(170 / 255f, 218 / 255f, 162 / 255f), // #aadda2
            new Color(254 / 255f, 239 / 255f, 164 / 255f), // #feefa4
            Mathf.InverseLerp(0.2f, 0.3f, height)
        );
    }
    else if (height >= 0.1f)
    {
        return Color.Lerp(
            new Color(74 / 255f, 158 / 255f, 178 / 255f),  // #4a9eb2
            new Color(170 / 255f, 218 / 255f, 162 / 255f), // #aadda2
            Mathf.InverseLerp(0.1f, 0.2f, height)
        );
    }
    else if (height >= 0.05f)
    {
        return Color.Lerp(
            new Color(75 / 255f, 105 / 255f, 173 / 255f),  // #4b69ad
            new Color(74 / 255f, 158 / 255f, 178 / 255f),  // #4a9eb2
            Mathf.InverseLerp(0.05f, 0.1f, height)
        );
    }
    else
    {
        return Color.Lerp(
            new Color(94 / 255f, 79 / 255f, 162 / 255f),   // #5e4fa2
            new Color(75 / 255f, 105 / 255f, 173 / 255f),  // #4b69ad
            Mathf.InverseLerp(0f, 0.05f, height)
        );
    }
}




    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);


        Color[] colorMap = new Color[width * height];


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < height; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TextureFromColorMap(colorMap, width);
    }

}

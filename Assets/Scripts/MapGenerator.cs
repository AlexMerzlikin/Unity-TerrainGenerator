using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {
    public const int mapChunkSize = 241;
    public float noiseScale;

    public int octaves;
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    //FogDensity 0 means no fog
    [Range(0f, 0.5f)]
    public float fogDensity = 0.005f;

    public TerrainType[] regions;

    /// <summary>
    /// Generates map data that contains height map and colour map at the particular point of world space.
    /// </summary>
    /// <param name="center"> Center of the height and colour map</param>
    /// <returns> MapData that contains height and colour maps </returns>
    public MapData GenerateMapData(Vector2 center) {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, center + offset);
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) {
                    if (currentHeight >= regions[i].height) {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                    }
                    else {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colourMap);
    }
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color colour;
}

public struct MapData {
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;

    /// <summary>
    /// MapData constructor
    /// </summary>
    /// <param name="heightMap"> Generated hight map</param>
    /// <param name="colourMap"> Generated colour map</param>
    public MapData(float[,] heightMap, Color[] colourMap) {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}

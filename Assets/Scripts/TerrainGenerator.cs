using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

    const float scale = 5f;
    public Material mapMaterial;
    public static Vector2 playerPos;
    static MapGenerator mapGenerator;
    int chunkSize;
    int terrainRadius;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    /// <summary>
    /// Called at initialization. Chaches MapGenerator, size of each terrain chunk, radius around starting chunk measured in chunks 
    /// e.g. radius = 1 means 1 chunk in center and 8 chunks around it, thus radius = 2 means previous 9 chunks plus 16 chunks around them. 
    /// </summary>
    void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        terrainRadius = 2;
        GenerateChunks();
    }

    /// <summary>
    /// Creates objects of type TerrainChunk and adds them and their position to the dictionary.
    /// </summary>
    void GenerateChunks() {
        int startChunkPosX = Mathf.RoundToInt(playerPos.x / chunkSize);
        int startChunkPosY = Mathf.RoundToInt(playerPos.y / chunkSize);

        for (int yOffset = -terrainRadius; yOffset <= terrainRadius; yOffset++) {
            for (int xOffset = -terrainRadius; xOffset <= terrainRadius; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(startChunkPosX + xOffset, startChunkPosY + yOffset);
                terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
            }
        }
    }

    /// <summary>
    /// Defines TerrainChunk object
    /// </summary>
    public class TerrainChunk {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        Mesh chunkMesh;

        MapData mapData;
        bool mapDataReceived;

        /// <summary>
        /// Creates an instance of TerrainChunk
        /// </summary>
        /// <param name="coord">Position of the new chunk</param>
        /// <param name="size">Chunk size</param>
        /// <param name="parent">Parent object of chunk to place all created chunks in hierarchy under particular object. Just for not spamming the heirarchy view.</param>
        /// <param name="material">Material which is applied to the renderer of the chunk</param>
        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material) {

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;

            chunkMesh = new Mesh();
            MapData mapData = mapGenerator.GenerateMapData(position);
            this.mapData = mapData;
            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;
            CreateTerrainChunk();
        }

        /// <summary>
        /// Mesh is created from chunkMesh and mesh data generated from map data.   
        /// </summary>
        public void CreateTerrainChunk() {
            Mesh mesh = chunkMesh;
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, mapGenerator.meshHeightMultiplier, mapGenerator.meshHeightCurve);
            mesh = meshData.CreateMesh();
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
    }
}

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

    void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        terrainRadius = 2;
        GenerateChunks();
    }

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

    public class TerrainChunk {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        ChunkMesh chunkMesh;

        MapData mapData;
        bool mapDataReceived;

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

            chunkMesh = new ChunkMesh(CreateTerrainChunk);
            

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData) {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            CreateTerrainChunk();
        }

        public void CreateTerrainChunk() {
            if (mapDataReceived) {
                ChunkMesh mesh = chunkMesh;
                if (mesh.hasMesh) {
                    meshFilter.mesh = mesh.mesh;
                    meshCollider.sharedMesh = mesh.mesh;
                }
                else if (!mesh.hasRequestedMesh) {
                    mesh.RequestMesh(mapData);
                }
            }
        }


    }

    class ChunkMesh {

        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        System.Action updateCallback;

        public ChunkMesh(System.Action updateCallback) {
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData) {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

    }


}

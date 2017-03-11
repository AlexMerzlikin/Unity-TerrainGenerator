using UnityEngine;
using System.Collections;

public static class MeshGenerator {

    /// <summary>
    /// Generates object of type MeshData, which has method to create the mesh itself.
    /// </summary>
    /// <param name="heightMap"> The height map for the future mesh. Basically, in our case it is the noise map, generated in Noise class. </param>
    /// <param name="heightMultiplier"> The value is set up in the TerrainGenerator inspector. 
    /// Used to multiply values from height map to adjust height (e.g. make the mountains/hills higher or smaller) </param>
    /// <param name="_heightCurve">The value is set up in the TerrainGenerator inspector.
    /// Applied to height map values to evaluate them according to the curve set up in Unity inspector. In our case the curve is used to smooth out bottom values.
    /// It allows the "water" of our mesh to have a flat surface. Without this evaluation the generated mesh would have vertices going down instead of flat water surface. </param>
    /// <returns></returns>
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve) {
		AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);

		int width = heightMap.GetLength (0);
		int height = heightMap.GetLength (1);
		float topLeftX = (width - 1) / -2f;
		float topLeftZ = (height - 1) / 2f;


		MeshData meshData = new MeshData (width, width);
		int vertexIndex = 0;

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				meshData.vertices [vertexIndex] = new Vector3 (topLeftX + x, heightCurve.Evaluate (heightMap [x, y]) * heightMultiplier, topLeftZ - y);
				meshData.uvs [vertexIndex] = new Vector2 (x / (float)width, y / (float)height);

				if (x < width - 1 && y < height - 1) {
					meshData.AddTriangle (vertexIndex, vertexIndex + width + 1, vertexIndex + width);
					meshData.AddTriangle (vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		return meshData;

	}
}

public class MeshData {
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

    /// <summary>
    /// MeshData constructor.
    /// </summary>
    /// <param name="meshWidth"> Width of the mesh</param>
    /// <param name="meshHeight"> Height of the mesh </param>
    public MeshData(int meshWidth, int meshHeight) {
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
	}

    /// <summary>
    /// Adds triangle to triangles array between 3 vertices moving in clock-wise order
    /// <param name="a">The first vertex </param>
    /// <param name="b">The second vertex</param>
    /// <param name="c">The thrird vertex</param>
	public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}

    /// <summary>
    /// Creates mesh out of this mesh data.
    /// </summary>
    /// <returns> Mesh with this mesh data </returns>
	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals ();
		return mesh;
	}

}
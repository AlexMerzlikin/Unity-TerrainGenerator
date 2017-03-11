using UnityEngine;
using System.Collections;

public static class TextureGenerator {

    /// <summary>
    /// Generates a texture for the mesh from color map.
    /// </summary>
    /// <param name="colourMap"> Color map that contains color for each particular pixel of the texture </param>
    /// <param name="width"> Width of texture</param>
    /// <param name="height"> Height of texture </param>
    /// <returns> The texture that is added to Mesh Material, which instance is applied to the terrain chunks </returns>
	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
		Texture2D texture = new Texture2D (width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels (colourMap);
		texture.Apply ();
		return texture;
	}
}

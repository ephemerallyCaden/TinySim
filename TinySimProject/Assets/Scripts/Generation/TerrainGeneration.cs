using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap tilemap;
    public Tile[] tiles; //deep ocean, shallow ocean, sand, plains, deep plains, hills, mountains, peaks

    [Header("Perlin Noise Settings")]
    public int width = 100;
    public int height = 100;
    public float scale = 20f;
    public Vector2 offset;

    public void GenerateTerrain(int width, int height, float scale, Vector2 offset)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Calculate Perlin noise value
                float xCoord = (float)x / width * scale + offset.x;
                float yCoord = (float)y / height * scale + offset.y;
                float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

                // Determine tile type based on noise value
                Tile tile = GetTileFromNoise(noiseValue);

                // Set the tile in the tilemap
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    private Tile GetTileFromNoise(float noiseValue)
    {
        if (noiseValue < 0.1f) return tiles[0]; // Deep ocean
        if (noiseValue < 0.2f) return tiles[1]; // Shallow ocean
        if (noiseValue < 0.3f) return tiles[2]; // Sand
        if (noiseValue < 0.5f) return tiles[3]; // Plains
        if (noiseValue < 0.6f) return tiles[4]; // Deep plains
        if (noiseValue < 0.7f) return tiles[5]; // Hills
        if (noiseValue < 0.9f) return tiles[6]; // Mountains
        return tiles[7]; // Peaks
    }
}
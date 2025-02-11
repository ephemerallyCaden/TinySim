using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap tilemap;
    public Tile[] tiles; // deep ocean, shallow ocean, sand, plains, deep plains, hills, mountains, peaks



    [Header("Octave Settings")]
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    private void Start()
    {
        tilemap = FindObjectOfType<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogError("Tilemap not found");
        }
        else
        {
            Debug.Log("Tilemap found");
        }
    }

    public void GenerateTerrain(float width, float height, float scale, Vector2 offset)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Calculate Perlin noise value using octaves
                float noiseValue = GeneratePerlinNoise(x, y, width, height, scale, offset);

                // Determine tile type based on noise value
                Tile tile = GetTileFromNoise(noiseValue);

                // Set the tile in the tilemap
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    private float GeneratePerlinNoise(int x, int y, float width, float height, float scale, Vector2 offset)
    {
        float total = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float maxValue = 0f; // Used for normalization

        for (int i = 0; i < octaves; i++)
        {
            float xCoord = (x / width * scale * frequency) + offset.x;
            float yCoord = (y / height * scale * frequency) + offset.y;

            total += Mathf.PerlinNoise(xCoord, yCoord) * amplitude;
            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / maxValue; // Normalize result to 0-1
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

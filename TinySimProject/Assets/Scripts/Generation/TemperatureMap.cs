using UnityEngine;

public class TemperatureMap : MonoBehaviour
{
    private float[,] temperatureMap;

    public void GenerateTemperatureMap(int width, int height, float scale)
    {
        temperatureMap = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Use Perlin noise to generate temperature variation
                float temperature = Mathf.PerlinNoise(x * scale, y * scale) * 100f; // Adjust the scale as needed
                temperatureMap[x, y] = temperature;
            }
        }
    }

    public float GetTemperatureAt(int x, int y)
    {
        return temperatureMap[x, y];
    }
}
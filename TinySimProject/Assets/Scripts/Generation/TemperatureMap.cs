using UnityEngine;
public class TemperatureMap : MonoBehaviour
{
    private float[,] temperatureMap;

    public void GenerateTemperatureMap(int width, int height, float scale)
    {
        temperatureMap = new float[width, height];
        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Use Perlin noise to generate temperature variation
                float temperature = Mathf.PerlinNoise(x * scale + offsetX, y * scale + offsetY); // Adjust the scale as needed
                temperatureMap[x, y] = temperature + 0.5f;
            }
        }
    }

    public float GetTemperatureAt(int x, int y)
    {
        return temperatureMap[x, y];
    }
}
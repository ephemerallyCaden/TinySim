using UnityEngine;
public class TemperatureMap : MonoBehaviour
{
    private float[,] temperatureMap;
    private SpriteRenderer backgroundRenderer;

    public void GenerateTemperatureMap(int width, int height, float scale)
    {
        temperatureMap = new float[width, height];
        // Set a random seed
        float offsetX = SimRandom.Range(0f, 99999f);
        float offsetY = SimRandom.Range(0f, 99999f);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Use Perlin noise to generate temperature variation
                float raw = Mathf.PerlinNoise(x / (float)width * scale + offsetX, y / (float)height * scale + offsetY);
                // Skew toward cold: power of 1.5 makes warm areas smaller but not extreme
                float temperature = Mathf.Pow(raw, 1.5f);
                temperatureMap[x, y] = Mathf.Clamp01(temperature);
            }
        }

        // Generate a visual background from the temperature data
        GenerateBackground(width, height);
    }

    private void GenerateBackground(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float t = temperatureMap[x, y];
                // Cold = dark blue, Medium = green, Warm = yellow/orange
                Color colour;
                if (t < 0.3f)
                    colour = Color.Lerp(new Color(0.1f, 0.1f, 0.3f), new Color(0.2f, 0.4f, 0.3f), t / 0.3f);
                else if (t < 0.7f)
                    colour = Color.Lerp(new Color(0.2f, 0.4f, 0.3f), new Color(0.4f, 0.5f, 0.2f), (t - 0.3f) / 0.4f);
                else
                    colour = Color.Lerp(new Color(0.4f, 0.5f, 0.2f), new Color(0.6f, 0.5f, 0.1f), (t - 0.7f) / 0.3f);

                texture.SetPixel(x, y, colour);
            }
        }
        texture.Apply();

        // Create or reuse a sprite renderer for the background
        if (backgroundRenderer == null)
        {
            GameObject bgObj = new GameObject("TemperatureBackground");
            bgObj.transform.SetParent(transform);
            backgroundRenderer = bgObj.AddComponent<SpriteRenderer>();
            backgroundRenderer.sortingOrder = -100; // Behind everything
        }

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.zero, 1f);
        backgroundRenderer.sprite = sprite;
        backgroundRenderer.transform.position = Vector3.zero;
    }

    public float GetTemperatureAt(int x, int y)
    {
        x = Mathf.Clamp(x, 0, temperatureMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, temperatureMap.GetLength(1) - 1);
        return temperatureMap[x, y];
    }
}
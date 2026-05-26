using UnityEngine;
using UnityEngine.UI;

public class TemperatureMapPreview : MonoBehaviour
{
    public RawImage previewImage;

    private Texture2D previewTexture;
    private int previewSize = 128; // Fixed resolution for the preview

    private void Start()
    {
        if (previewImage == null)
            previewImage = GetComponent<RawImage>();
    }

    public void Regenerate(int worldSize, float scale, float coldSkewPower)
    {
        if (previewTexture == null || previewTexture.width != previewSize)
        {
            previewTexture = new Texture2D(previewSize, previewSize, TextureFormat.RGB24, false);
            previewTexture.filterMode = FilterMode.Bilinear;
            if (previewImage != null)
                previewImage.texture = previewTexture;
        }

        // Use a fixed seed for preview consistency while dragging sliders
        System.Random previewRng = new System.Random(42);
        float offsetX = (float)(previewRng.NextDouble() * 99999);
        float offsetY = (float)(previewRng.NextDouble() * 99999);

        for (int x = 0; x < previewSize; x++)
        {
            for (int y = 0; y < previewSize; y++)
            {
                float raw = Mathf.PerlinNoise(
                    x / (float)previewSize * scale + offsetX,
                    y / (float)previewSize * scale + offsetY);

                float temperature = Mathf.Pow(raw, coldSkewPower);
                temperature = Mathf.Clamp01(temperature);

                // Same colour mapping as TemperatureMap.GenerateBackground
                Color colour;
                if (temperature < 0.3f)
                    colour = Color.Lerp(new Color(0.1f, 0.1f, 0.3f), new Color(0.2f, 0.4f, 0.3f), temperature / 0.3f);
                else if (temperature < 0.7f)
                    colour = Color.Lerp(new Color(0.2f, 0.4f, 0.3f), new Color(0.4f, 0.5f, 0.2f), (temperature - 0.3f) / 0.4f);
                else
                    colour = Color.Lerp(new Color(0.4f, 0.5f, 0.2f), new Color(0.6f, 0.5f, 0.1f), (temperature - 0.7f) / 0.3f);

                previewTexture.SetPixel(x, y, colour);
            }
        }

        previewTexture.Apply();
    }
}

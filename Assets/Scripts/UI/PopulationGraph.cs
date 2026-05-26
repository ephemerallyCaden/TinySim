using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Population and avg. generation graph over time.
public class PopulationGraph : MonoBehaviour, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Graph Settings")]
    public int graphWidth = 300;
    public int graphHeight = 150;
    public float sampleInterval = 5f; // Sample every N seconds of world time
    public int maxSamples = 200; // Visible data points in window

    [Header("Display")]
    public Color populationColour = Color.cyan;
    public Color generationColour = Color.yellow;
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

    private RawImage graphImage;
    private Texture2D graphTexture;
    private List<float> populationData = new List<float>();
    private List<float> generationData = new List<float>();
    private float nextSampleTime = 0f;
    private int maxPopulationSeen = 1;
    private int maxGenerationSeen = 1;

    // Scrolling
    private int viewOffset = 0; // 0 = live, >0 = scrolled back
    private bool isHovered = false;
    private bool needsRedraw = false;

    // GUI labels
    private GUIStyle labelStyle;
    private GUIStyle legendStyle;

    public bool IsLive => viewOffset == 0;

    private void Start()
    {
        graphImage = GetComponent<RawImage>();
        if (graphImage == null)
        {
            graphImage = gameObject.AddComponent<RawImage>();
        }

        graphTexture = new Texture2D(graphWidth, graphHeight);
        graphTexture.filterMode = FilterMode.Point;
        graphImage.texture = graphTexture;

        ClearGraph();
    }

    private void Update()
    {
        float worldTime = SimulationManager.instance.worldTime;
        if (worldTime >= nextSampleTime)
        {
            nextSampleTime += sampleInterval;

            // Sample data
            int pop = AgentManager.instance.population;
            int gen = AgentManager.instance.avgGeneration;

            if (pop > maxPopulationSeen) maxPopulationSeen = pop;
            if (gen > maxGenerationSeen) maxGenerationSeen = gen;

            populationData.Add(pop);
            generationData.Add(gen);

            if (IsLive)
                needsRedraw = true;
        }

        if (needsRedraw)
        {
            needsRedraw = false;
            DrawGraph();
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;
        int scrollAmount = Mathf.Max(1, maxSamples / 10);

        if (scroll > 0)
        {
            // Scroll back
            viewOffset += scrollAmount;
        }
        else if (scroll < 0)
        {
            // Scroll forward
            viewOffset -= scrollAmount;
        }

        int maxOffset = Mathf.Max(0, populationData.Count - maxSamples);
        viewOffset = Mathf.Clamp(viewOffset, 0, maxOffset);

        needsRedraw = true;
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => isHovered = false;

    private void DrawGraph()
    {
        ClearGraph();

        int dataCount = populationData.Count;
        if (dataCount < 2) return;

        // Calculate visible window
        int endIndex = dataCount - viewOffset;
        int startIndex = Mathf.Max(0, endIndex - maxSamples);
        int visibleCount = endIndex - startIndex;
        if (visibleCount < 2) return;

        // Population line
        for (int i = 1; i < visibleCount; i++)
        {
            int dataIdx = startIndex + i;
            int x0 = (int)((float)(i - 1) / maxSamples * graphWidth);
            int x1 = (int)((float)i / maxSamples * graphWidth);
            int y0 = (int)(populationData[dataIdx - 1] / maxPopulationSeen * (graphHeight - 10));
            int y1 = (int)(populationData[dataIdx] / maxPopulationSeen * (graphHeight - 10));
            DrawLine(x0, y0, x1, y1, populationColour);
        }

        // Generation line
        for (int i = 1; i < visibleCount; i++)
        {
            int dataIdx = startIndex + i;
            int x0 = (int)((float)(i - 1) / maxSamples * graphWidth);
            int x1 = (int)((float)i / maxSamples * graphWidth);
            int y0 = (int)(generationData[dataIdx - 1] / maxGenerationSeen * (graphHeight - 10));
            int y1 = (int)(generationData[dataIdx] / maxGenerationSeen * (graphHeight - 10));
            DrawLine(x0, y0, x1, y1, generationColour);
        }

        if (!IsLive)
        {
            Color32 pauseColour = new Color(1f, 1f, 1f, 0.5f);
            // Draw a small bar at top-right corner to indicate paused
            for (int x = graphWidth - 20; x < graphWidth - 2; x++)
            {
                for (int y = graphHeight - 8; y < graphHeight - 2; y++)
                {
                    if (x >= 0 && x < graphWidth && y >= 0 && y < graphHeight)
                        graphTexture.SetPixel(x, y, pauseColour);
                }
            }
        }

        graphTexture.Apply();
    }

    private void DrawLine(int x0, int y0, int x1, int y1, Color colour)
    {
        TextureDrawUtils.DrawLine(graphTexture, x0, y0, x1, y1, colour, 2);
    }

    private void ClearGraph()
    {
        Color[] pixels = new Color[graphWidth * graphHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        graphTexture.SetPixels(pixels);
        graphTexture.Apply();
    }

    private void OnGUI()
    {
        if (graphImage == null || !graphImage.enabled) return;
        if (!graphImage.gameObject.activeInHierarchy) return;

        // Create styles once
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 10;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.UpperRight;
        }
        if (legendStyle == null)
        {
            legendStyle = new GUIStyle(GUI.skin.label);
            legendStyle.fontSize = 9;
            legendStyle.fontStyle = FontStyle.Bold;
        }

        RectTransform rt = graphImage.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Canvas canvas = graphImage.canvas;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 screenMin = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 screenMax = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

        float rectX = screenMin.x;
        float rectY = Screen.height - screenMax.y; // GUI Y is flipped
        float rectWidth = screenMax.x - screenMin.x;
        float rectHeight = screenMax.y - screenMin.y;

        // Legend
        legendStyle.normal.textColor = populationColour;
        GUI.Label(new Rect(rectX + 4, rectY + 2, 120, 14), "-- Population", legendStyle);

        legendStyle.normal.textColor = generationColour;
        GUI.Label(new Rect(rectX + 4, rectY + 14, 120, 14), "-- Generation", legendStyle);

        // Y-axis max labels
        labelStyle.normal.textColor = populationColour;
        GUI.Label(new Rect(rectX + rectWidth - 80, rectY + 2, 76, 14), $"Pop: {maxPopulationSeen}", labelStyle);

        labelStyle.normal.textColor = generationColour;
        GUI.Label(new Rect(rectX + rectWidth - 80, rectY + 14, 76, 14), $"Gen: {maxGenerationSeen}", labelStyle);
    }
}

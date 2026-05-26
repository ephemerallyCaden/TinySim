using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Renders the ancestry chain of a selected species as a horizontal progression.
/// Circles and arrows drawn to a texture, text labels via OnGUI.
public class AncestryView : MonoBehaviour
{
    [Header("References")]
    public RawImage displayImage;

    [Header("Settings")]
    public int textureWidth = 600;
    public int textureHeight = 100;
    public Color backgroundColor = new Color(0.05f, 0.05f, 0.1f, 0.9f);

    private Texture2D texture;
    private Color32[] pixels;

    // Stored node positions for OnGUI labels
    private struct NodeInfo
    {
        public float normalizedX;
        public string name;
        public string timeLabel;
        public Color colour;
        public bool isSelected;
    }
    private List<NodeInfo> nodeInfos = new List<NodeInfo>();

    private GUIStyle nameStyle;
    private GUIStyle timeStyle;

    private void Awake()
    {
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        pixels = new Color32[textureWidth * textureHeight];

        if (displayImage != null)
            displayImage.texture = texture;
    }

    public void ShowAncestry(int speciesId)
    {
        var entries = SpeciesHistoryTracker.instance.GetAllEntries();
        List<SpeciesHistoryEntry> chain = new List<SpeciesHistoryEntry>();

        int current = speciesId;
        while (current >= 0 && entries.TryGetValue(current, out var entry))
        {
            chain.Add(entry);
            current = entry.parentSpeciesId;
        }

        chain.Reverse();
        Draw(chain);
    }

    public void Clear()
    {
        nodeInfos.Clear();
        ClearPixels();
        texture.SetPixels32(pixels);
        texture.Apply();
    }

    private void Draw(List<SpeciesHistoryEntry> chain)
    {
        ClearPixels();
        nodeInfos.Clear();

        if (chain.Count == 0)
        {
            texture.SetPixels32(pixels);
            texture.Apply();
            return;
        }

        int nodeRadius = 15;
        int centerY = textureHeight / 2;
        int padding = 40;
        int spacing = (textureWidth - padding * 2) / Mathf.Max(chain.Count, 1);
        spacing = Mathf.Min(spacing, 120);

        for (int i = 0; i < chain.Count; i++)
        {
            var entry = chain[i];
            int cx = padding + i * spacing + spacing / 2;
            bool isSelected = (i == chain.Count - 1);

            // Draw connecting arrow to next node
            if (i < chain.Count - 1)
            {
                int nextCx = padding + (i + 1) * spacing + spacing / 2;
                DrawArrow(cx + nodeRadius + 2, centerY, nextCx - nodeRadius - 2, centerY);
            }

            // Draw node circle
            DrawFilledCircle(cx, centerY, nodeRadius, entry.colour);

            if (isSelected)
                DrawCircleOutline(cx, centerY, nodeRadius + 2, Color.white);

            // Store node info for OnGUI text
            nodeInfos.Add(new NodeInfo
            {
                normalizedX = (float)cx / textureWidth,
                name = entry.speciesName,
                timeLabel = $"{entry.birthTime:F0}s",
                colour = entry.colour,
                isSelected = isSelected
            });
        }

        texture.SetPixels32(pixels);
        texture.Apply();
    }

    private void OnGUI()
    {
        if (nodeInfos.Count == 0) return;
        if (displayImage == null || !displayImage.gameObject.activeInHierarchy) return;

        // Init styles
        if (nameStyle == null)
        {
            nameStyle = new GUIStyle(GUI.skin.label);
            nameStyle.fontSize = 10;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.alignment = TextAnchor.UpperCenter;
        }
        if (timeStyle == null)
        {
            timeStyle = new GUIStyle(GUI.skin.label);
            timeStyle.fontSize = 9;
            timeStyle.alignment = TextAnchor.LowerCenter;
            timeStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        }

        // Get RawImage screen rect
        RectTransform rt = displayImage.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Canvas canvas = displayImage.canvas;
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

        Vector2 screenMin = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 screenMax = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

        float rectX = screenMin.x;
        float rectY = Screen.height - screenMax.y;
        float rectWidth = screenMax.x - screenMin.x;
        float rectHeight = screenMax.y - screenMin.y;

        float labelWidth = 100f;
        float centerScreenY = rectY + rectHeight * 0.5f;

        foreach (var node in nodeInfos)
        {
            float screenX = rectX + node.normalizedX * rectWidth;

            // Name below the node
            nameStyle.normal.textColor = node.isSelected ? Color.white : node.colour;
            GUI.Label(new Rect(screenX - labelWidth / 2, centerScreenY + 18, labelWidth, 20), node.name, nameStyle);

            // Birth time above the node
            GUI.Label(new Rect(screenX - labelWidth / 2, centerScreenY - 32, labelWidth, 20), node.timeLabel, timeStyle);
        }
    }

    private void DrawArrow(int x0, int y0, int x1, int y1)
    {
        Color32 arrowColour = new Color32(180, 180, 180, 255);
        DrawThickHLine(x0, x1, y0, arrowColour, 1);

        // Arrowhead
        int headSize = 5;
        for (int i = 0; i < headSize; i++)
        {
            int px = x1 - i;
            if (px < 0 || px >= textureWidth) continue;
            for (int dy = -i; dy <= i; dy++)
            {
                int py = y0 + dy;
                if (py >= 0 && py < textureHeight)
                    pixels[py * textureWidth + px] = arrowColour;
            }
        }
    }

    private void DrawThickHLine(int x0, int x1, int y, Color32 colour, int thickness)
    {
        for (int x = Mathf.Max(0, x0); x <= Mathf.Min(textureWidth - 1, x1); x++)
        {
            for (int t = -thickness; t <= thickness; t++)
            {
                int py = y + t;
                if (py >= 0 && py < textureHeight)
                    pixels[py * textureWidth + x] = colour;
            }
        }
    }

    private void DrawFilledCircle(int cx, int cy, int radius, Color colour)
    {
        Color32 col32 = colour;
        int r2 = radius * radius;
        for (int dy = -radius; dy <= radius; dy++)
        {
            int py = cy + dy;
            if (py < 0 || py >= textureHeight) continue;
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx * dx + dy * dy <= r2)
                {
                    int px = cx + dx;
                    if (px >= 0 && px < textureWidth)
                        pixels[py * textureWidth + px] = col32;
                }
            }
        }
    }

    private void DrawCircleOutline(int cx, int cy, int radius, Color colour)
    {
        Color32 col32 = colour;
        int r2Outer = radius * radius;
        int r2Inner = (radius - 2) * (radius - 2);
        for (int dy = -radius; dy <= radius; dy++)
        {
            int py = cy + dy;
            if (py < 0 || py >= textureHeight) continue;
            for (int dx = -radius; dx <= radius; dx++)
            {
                int dist2 = dx * dx + dy * dy;
                if (dist2 <= r2Outer && dist2 >= r2Inner)
                {
                    int px = cx + dx;
                    if (px >= 0 && px < textureWidth)
                        pixels[py * textureWidth + px] = col32;
                }
            }
        }
    }

    private void ClearPixels()
    {
        Color32 bg = backgroundColor;
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = bg;
    }
}

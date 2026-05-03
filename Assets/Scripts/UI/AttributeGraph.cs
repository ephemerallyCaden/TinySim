using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Draws a graph of population-average gene attributes over time.
/// Attach to a UI GameObject with a RawImage component.
/// Supports switching between attributes and scrolling back through history.
/// </summary>
public class AttributeGraph : MonoBehaviour, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum GeneAttribute
    {
        Size,
        Speed,
        VisionDistance,
        VisionAngle,
        ReproductionRange,
        ReproductionCooldown,
        ReproductionEnergyCost,
        MutationChanceMod,
        MutationMagnitudeMod
    }

    private static readonly string[] attributeNames = new string[]
    {
        "Size",
        "Speed",
        "Vision Distance",
        "Vision Angle",
        "Reproduction Range",
        "Reproduction Cooldown",
        "Reproduction Energy Cost",
        "Mutation Chance Mod",
        "Mutation Magnitude Mod"
    };

    private static readonly Color[] attributeColours = new Color[]
    {
        new Color(0.2f, 0.8f, 0.2f),    // Size - green
        new Color(0.9f, 0.4f, 0.1f),    // Speed - orange
        new Color(0.3f, 0.6f, 1.0f),    // Vision Distance - blue
        new Color(0.7f, 0.3f, 1.0f),    // Vision Angle - purple
        new Color(1.0f, 0.8f, 0.2f),    // Reproduction Range - gold
        new Color(0.0f, 0.9f, 0.9f),    // Reproduction Cooldown - teal
        new Color(1.0f, 0.3f, 0.5f),    // Reproduction Energy Cost - pink
        new Color(0.6f, 0.9f, 0.3f),    // Mutation Chance Mod - lime
        new Color(0.9f, 0.6f, 0.8f)     // Mutation Magnitude Mod - light pink
    };

    [Header("Graph Settings")]
    public int graphWidth = 300;
    public int graphHeight = 150;
    public float sampleInterval = 5f;
    public int maxSamples = 200;

    [Header("Display")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

    private RawImage graphImage;
    private Texture2D graphTexture;
    private Color[] clearPixels; // Pre-allocated to avoid per-redraw GC allocation
    private float nextSampleTime = 0f;

    // Store history for ALL attributes simultaneously
    private int attributeCount;
    private List<float>[] attributeData;
    private float[] maxValueSeen;

    // Currently displayed attribute
    private GeneAttribute currentAttribute = GeneAttribute.Size;

    // Scrolling
    private int viewOffset = 0;
    private bool isHovered = false;
    private bool needsRedraw = false;

    // Dropdown state
    private bool showDropdown = false;

    // GUI styles
    private GUIStyle labelStyle;
    private GUIStyle legendStyle;
    private GUIStyle buttonStyle;
    private GUIStyle dropdownItemStyle;
    private GUIStyle dropdownItemHoverStyle;

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

        // Pre-allocate clear buffer
        clearPixels = new Color[graphWidth * graphHeight];
        for (int i = 0; i < clearPixels.Length; i++)
            clearPixels[i] = backgroundColor;

        // Initialise data storage for all attributes
        attributeCount = System.Enum.GetValues(typeof(GeneAttribute)).Length;
        attributeData = new List<float>[attributeCount];
        maxValueSeen = new float[attributeCount];
        for (int i = 0; i < attributeCount; i++)
        {
            attributeData[i] = new List<float>();
            maxValueSeen[i] = 0.001f; // Avoid division by zero
        }

        ClearGraph();
    }

    private bool IsVisible => graphImage != null && graphImage.gameObject.activeInHierarchy;

    private void Update()
    {
        float worldTime = SimulationManager.instance.worldTime;
        if (worldTime >= nextSampleTime)
        {
            nextSampleTime += sampleInterval;
            SampleAllAttributes();

            if (IsLive && IsVisible)
                needsRedraw = true;
        }

        // Only redraw when actually visible
        if (needsRedraw && IsVisible)
        {
            needsRedraw = false;
            DrawGraph();
        }
    }

    private void SampleAllAttributes()
    {
        var agents = AgentManager.instance.agents;
        int pop = AgentManager.instance.population;

        if (pop == 0)
        {
            // Store zeros if no population
            for (int i = 0; i < attributeCount; i++)
            {
                attributeData[i].Add(0f);
            }
            return;
        }

        // Accumulate all attributes in one pass
        float sumSize = 0, sumSpeed = 0, sumVisionDist = 0, sumVisionAngle = 0;
        float sumRepRange = 0, sumRepCooldown = 0, sumRepEnergyCost = 0;
        float sumMutChance = 0, sumMutMagnitude = 0;
        int validCount = 0;

        for (int i = 0; i < agents.Count; i++)
        {
            Agent a = agents[i];
            if (a == null) continue;
            validCount++;
            sumSize += a.size;
            sumSpeed += a.speed;
            sumVisionDist += a.visionDistance;
            sumVisionAngle += a.visionAngle;
            sumRepRange += a.reproductionRange;
            sumRepCooldown += a.maxReproductionCooldown;
            sumRepEnergyCost += a.reproductionEnergyCost;
            sumMutChance += a.mutationChanceMod;
            sumMutMagnitude += a.mutationMagnitudeMod;
        }

        if (validCount == 0) validCount = 1;

        float[] averages = new float[]
        {
            sumSize / validCount,
            sumSpeed / validCount,
            sumVisionDist / validCount,
            sumVisionAngle / validCount,
            sumRepRange / validCount,
            sumRepCooldown / validCount,
            sumRepEnergyCost / validCount,
            sumMutChance / validCount,
            sumMutMagnitude / validCount
        };

        for (int i = 0; i < attributeCount; i++)
        {
            attributeData[i].Add(averages[i]);
            if (averages[i] > maxValueSeen[i])
                maxValueSeen[i] = averages[i];
        }
    }

    /// <summary>
    /// Switch which attribute is displayed. Redraws instantly from stored data.
    /// </summary>
    public void SetAttribute(GeneAttribute attribute)
    {
        currentAttribute = attribute;
        showDropdown = false;
        needsRedraw = true;
    }

    private void OnEnable()
    {
        // Redraw when tab becomes visible
        needsRedraw = true;
    }

    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;
        int scrollAmount = Mathf.Max(1, maxSamples / 10);

        if (scroll > 0)
        {
            viewOffset += scrollAmount;
        }
        else if (scroll < 0)
        {
            viewOffset -= scrollAmount;
        }

        int dataCount = attributeData[0].Count;
        int maxOffset = Mathf.Max(0, dataCount - maxSamples);
        viewOffset = Mathf.Clamp(viewOffset, 0, maxOffset);

        needsRedraw = true;
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => isHovered = false;

    private void DrawGraph()
    {
        ClearGraph();

        int attrIdx = (int)currentAttribute;
        var data = attributeData[attrIdx];
        int dataCount = data.Count;
        if (dataCount < 2) return;

        // Calculate visible window
        int endIndex = dataCount - viewOffset;
        int startIndex = Mathf.Max(0, endIndex - maxSamples);
        int visibleCount = endIndex - startIndex;
        if (visibleCount < 2) return;

        float maxVal = maxValueSeen[attrIdx];
        if (maxVal < 0.001f) maxVal = 1f;

        Color colour = attributeColours[attrIdx];

        // Draw the line
        for (int i = 1; i < visibleCount; i++)
        {
            int dataIdx = startIndex + i;
            int x0 = (int)((float)(i - 1) / maxSamples * graphWidth);
            int x1 = (int)((float)i / maxSamples * graphWidth);
            int y0 = (int)(data[dataIdx - 1] / maxVal * (graphHeight - 10));
            int y1 = (int)(data[dataIdx] / maxVal * (graphHeight - 10));
            TextureDrawUtils.DrawLine(graphTexture, x0, y0, x1, y1, colour, 2);
        }

        // Draw scroll indicator when not live
        if (!IsLive)
        {
            Color32 pauseColour = new Color(1f, 1f, 1f, 0.5f);
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

    private void ClearGraph()
    {
        graphTexture.SetPixels(clearPixels);
    }

    private void OnGUI()
    {
        if (!IsVisible) return;

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
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 9;
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.padding = new RectOffset(4, 4, 2, 2);
        }
        if (dropdownItemStyle == null)
        {
            dropdownItemStyle = new GUIStyle(GUI.skin.button);
            dropdownItemStyle.fontSize = 9;
            dropdownItemStyle.alignment = TextAnchor.MiddleLeft;
            dropdownItemStyle.padding = new RectOffset(4, 4, 1, 1);
            dropdownItemStyle.normal.textColor = Color.white;
        }
        if (dropdownItemHoverStyle == null)
        {
            dropdownItemHoverStyle = new GUIStyle(dropdownItemStyle);
            dropdownItemHoverStyle.normal.textColor = Color.yellow;
        }

        // Get the RawImage's screen rect
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

        int attrIdx = (int)currentAttribute;

        // Draw legend (current attribute name with its colour)
        legendStyle.normal.textColor = attributeColours[attrIdx];
        GUI.Label(new Rect(rectX + 4, rectY + 2, 180, 14), $"-- {attributeNames[attrIdx]}", legendStyle);

        // Draw Y-axis max value label
        labelStyle.normal.textColor = attributeColours[attrIdx];
        GUI.Label(new Rect(rectX + rectWidth - 90, rectY + 2, 86, 14), $"Max: {maxValueSeen[attrIdx]:F2}", labelStyle);

        // Draw current value (latest sample)
        if (attributeData[attrIdx].Count > 0)
        {
            float latest = attributeData[attrIdx][attributeData[attrIdx].Count - 1];
            labelStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rectX + rectWidth - 90, rectY + 14, 86, 14), $"Avg: {latest:F2}", labelStyle);
        }

        // Dropdown button (below graph)
        float dropdownX = rectX + 4;
        float dropdownY = rectY + rectHeight + 2;
        float dropdownWidth = 180;
        float dropdownButtonHeight = 18;

        if (GUI.Button(new Rect(dropdownX, dropdownY, dropdownWidth, dropdownButtonHeight), $"Attribute: {attributeNames[attrIdx]}", buttonStyle))
        {
            showDropdown = !showDropdown;
        }

        // Draw dropdown list if open
        if (showDropdown)
        {
            float itemHeight = 16;
            float listY = dropdownY + dropdownButtonHeight;

            // Background box
            GUI.Box(new Rect(dropdownX, listY, dropdownWidth, itemHeight * attributeCount + 4), "");

            for (int i = 0; i < attributeCount; i++)
            {
                float itemY = listY + 2 + i * itemHeight;
                GUIStyle style = (i == attrIdx) ? dropdownItemHoverStyle : dropdownItemStyle;
                style.normal.textColor = attributeColours[i];

                if (GUI.Button(new Rect(dropdownX + 2, itemY, dropdownWidth - 4, itemHeight), attributeNames[i], style))
                {
                    SetAttribute((GeneAttribute)i);
                }
            }
        }
    }
}

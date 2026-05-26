using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Draws a graph of population-average gene attributes over time.
public class AttributeGraph : MonoBehaviour, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
{
    public struct AttributeEntry
    {
        public string name;
        public Color colour;
        public AttributeEntry(string name, Color colour) { this.name = name; this.colour = colour; }
    }

    // Single source of truth for tracked attributes: order, name, and colour.
    public static readonly AttributeEntry[] attributes = new AttributeEntry[]
    {
        new AttributeEntry("Size",                      new Color(0.2f, 0.8f, 0.2f)),
        new AttributeEntry("Speed",                     new Color(0.9f, 0.4f, 0.1f)),
        new AttributeEntry("Vision Distance",           new Color(0.3f, 0.6f, 1.0f)),
        new AttributeEntry("Vision Angle",              new Color(0.7f, 0.3f, 1.0f)),
        new AttributeEntry("Reproduction Cooldown",     new Color(0.0f, 0.9f, 0.9f)),
        new AttributeEntry("Reproduction Energy Cost",  new Color(1.0f, 0.3f, 0.5f)),
        new AttributeEntry("Mutation Chance Mod",       new Color(0.6f, 0.9f, 0.3f)),
        new AttributeEntry("Mutation Magnitude Mod",    new Color(0.9f, 0.6f, 0.8f)),
        new AttributeEntry("Attack Damage",             new Color(1.0f, 0.5f, 0.0f)),
        new AttributeEntry("Diet Preference",           new Color(0.8f, 0.2f, 0.2f)),
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

    private int attributeCount;
    private List<float>[] attributeData;
    private float[] maxValueSeen;

    private int currentAttribute = 0;

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
        attributeCount = attributes.Length;
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

    // Called externally by AnalyticsPanel so data is collected even when tab is inactive.
    public void TrySample()
    {
        float worldTime = SimulationManager.instance.worldTime;
        if (worldTime >= nextSampleTime)
        {
            nextSampleTime += sampleInterval;
            SampleAllAttributes();

            if (IsLive)
                needsRedraw = true;
        }
    }

    private void Update()
    {
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
            for (int i = 0; i < attributeCount; i++)
            {
                attributeData[i].Add(0f);
            }
            return;
        }

        float sumSize = 0, sumSpeed = 0, sumVisionDist = 0, sumVisionAngle = 0;
        float sumRepCooldown = 0, sumRepEnergyCost = 0;
        float sumMutChance = 0, sumMutMagnitude = 0, sumAttackDamage = 0;
        float sumDietPreference = 0;
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
            sumRepCooldown += a.maxReproductionCooldown;
            sumRepEnergyCost += a.reproductionEnergyCost;
            sumMutChance += a.mutationChanceMod;
            sumMutMagnitude += a.mutationMagnitudeMod;
            sumAttackDamage += a.attackDamage;
            sumDietPreference += a.dietPreference;
        }

        if (validCount == 0) validCount = 1;

        float[] averages = new float[]
        {
            sumSize / validCount,
            sumSpeed / validCount,
            sumVisionDist / validCount,
            sumVisionAngle / validCount,
            sumRepCooldown / validCount,
            sumRepEnergyCost / validCount,
            sumMutChance / validCount,
            sumMutMagnitude / validCount,
            sumAttackDamage / validCount,
            sumDietPreference / validCount
        };

        for (int i = 0; i < attributeCount; i++)
        {
            attributeData[i].Add(averages[i]);
            if (averages[i] > maxValueSeen[i])
                maxValueSeen[i] = averages[i];
        }
    }

    // Switch which attribute is displayed.
    public void SetAttribute(int attributeIndex)
    {
        currentAttribute = attributeIndex;
        showDropdown = false;
        needsRedraw = true;
    }

    private void OnEnable()
    {
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

        var data = attributeData[currentAttribute];
        int dataCount = data.Count;
        if (dataCount < 2) return;

        // Current visible window
        int endIndex = dataCount - viewOffset;
        int startIndex = Mathf.Max(0, endIndex - maxSamples);
        int visibleCount = endIndex - startIndex;
        if (visibleCount < 2) return;

        float maxVal = maxValueSeen[currentAttribute];
        if (maxVal < 0.001f) maxVal = 1f;

        Color colour = attributes[currentAttribute].colour;

        for (int i = 1; i < visibleCount; i++)
        {
            int dataIdx = startIndex + i;
            int x0 = (int)((float)(i - 1) / maxSamples * graphWidth);
            int x1 = (int)((float)i / maxSamples * graphWidth);
            int y0 = (int)(data[dataIdx - 1] / maxVal * (graphHeight - 10));
            int y1 = (int)(data[dataIdx] / maxVal * (graphHeight - 10));
            TextureDrawUtils.DrawLine(graphTexture, x0, y0, x1, y1, colour, 2);
        }

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

        // Draw legend
        legendStyle.normal.textColor = attributes[currentAttribute].colour;
        GUI.Label(new Rect(rectX + 4, rectY + 2, 180, 14), $"-- {attributes[currentAttribute].name}", legendStyle);

        // Draw Y-axis max value label
        labelStyle.normal.textColor = attributes[currentAttribute].colour;
        GUI.Label(new Rect(rectX + rectWidth - 90, rectY + 2, 86, 14), $"Max: {maxValueSeen[currentAttribute]:F2}", labelStyle);

        // Draw current value
        if (attributeData[currentAttribute].Count > 0)
        {
            float latest = attributeData[currentAttribute][attributeData[currentAttribute].Count - 1];
            labelStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rectX + rectWidth - 90, rectY + 14, 86, 14), $"Avg: {latest:F2}", labelStyle);
        }

        // Dropdown button
        float dropdownX = rectX + 4;
        float dropdownY = rectY + rectHeight + 2;
        float dropdownWidth = 180;
        float dropdownButtonHeight = 18;

        if (GUI.Button(new Rect(dropdownX, dropdownY, dropdownWidth, dropdownButtonHeight), $"Attribute: {attributes[currentAttribute].name}", buttonStyle))
        {
            showDropdown = !showDropdown;
        }

        if (showDropdown)
        {
            float itemHeight = 16;
            float listY = dropdownY + dropdownButtonHeight;

            // Background box
            GUI.Box(new Rect(dropdownX, listY, dropdownWidth, itemHeight * attributeCount + 4), "");

            for (int i = 0; i < attributeCount; i++)
            {
                float itemY = listY + 2 + i * itemHeight;
                GUIStyle style = (i == currentAttribute) ? dropdownItemHoverStyle : dropdownItemStyle;
                style.normal.textColor = attributes[i].colour;

                if (GUI.Button(new Rect(dropdownX + 2, itemY, dropdownWidth - 4, itemHeight), attributes[i].name, style))
                {
                    SetAttribute(i);
                }
            }
        }
    }
}

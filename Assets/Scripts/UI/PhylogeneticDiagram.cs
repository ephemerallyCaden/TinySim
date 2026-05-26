using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Renders a phylogenetic tree diagram showing species lineage over time.
// Attach to a UI GameObject with a RawImage component.
public class PhylogeneticDiagram : MonoBehaviour, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Graph Settings")]
    public int graphWidth = 400;
    public int graphHeight = 250;
    public int maxLanes = 24;
    
    [Header("Display")]
    public Color backgroundColor = new Color(0.05f, 0.05f, 0.1f, 0.9f);
    public Color branchColour = Color.white;

    private RawImage graphImage;
    private Texture2D graphTexture;
    private Color32[] pixels;

    // Lane management: maps speciesId -> Y lane index
    private Dictionary<int, int> laneAssignments = new Dictionary<int, int>();
    private bool[] lanesOccupied;

    // Tracking
    private int lastSampleCount = 0;
    private int scrollOffset = 0; // How many samples have scrolled off the left

    private bool isRedrawing = false;

    private int minLaneGap = 2;

    // User scroll control
    private int userScrollOffset = 0; // 0 = live, >0 = scrolled back N samples
    private bool isUserScrolled = false;
    private bool isHovered = false;

    private void Start()
    {
        graphImage = GetComponent<RawImage>();
        if (graphImage == null)
            graphImage = gameObject.AddComponent<RawImage>();

        graphTexture = new Texture2D(graphWidth, graphHeight, TextureFormat.RGBA32, false);
        graphTexture.filterMode = FilterMode.Bilinear;
        graphImage.texture = graphTexture;

        lanesOccupied = new bool[maxLanes];

        ClearTexture();
    }

    private void Update()
    {
        if (SpeciesHistoryTracker.instance == null) return;

        // If user is scrolled back, don't process live updates (just track sample count)
        if (isUserScrolled)
        {
            // Still consume redraw flags so they don't stack up
            if (SpeciesHistoryTracker.instance.needsRedraw)
                SpeciesHistoryTracker.instance.needsRedraw = false;
            lastSampleCount = SpeciesHistoryTracker.instance.SampleCount;
            return;
        }

        // Full recalculation if a species was wiped
        if (SpeciesHistoryTracker.instance.needsRedraw || pendingRedraw)
        {
            SpeciesHistoryTracker.instance.needsRedraw = false;
            pendingRedraw = false;
            redrawRequestedTime = Time.time;
        }

        // Only actually redraw after a delay
        if (redrawRequestedTime > 0 && Time.time - redrawRequestedTime > 2f)
        {
            redrawRequestedTime = -1f;
            RedrawAll();
            return;
        }

        int currentSampleCount = SpeciesHistoryTracker.instance.SampleCount;

        // Check for extinct species, freeing their lanes
        FreeLanesForExtinctSpecies(currentSampleCount);

        if (currentSampleCount <= lastSampleCount) return;

        // Process all new samples since last update
        int samplesToProcess = currentSampleCount - lastSampleCount;
        for (int s = 0; s < samplesToProcess; s++)
        {
            int sampleIndex = lastSampleCount + s;
            DrawNewColumn(sampleIndex);
        }

        lastSampleCount = currentSampleCount;
        graphTexture.SetPixels32(pixels);
        graphTexture.Apply();
    }

    // --- Scroll input handling ---

    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;
        int scrollAmount = Mathf.Max(1, graphWidth / 10); // Scroll ~10% of visible window

        if (scroll > 0)
        {
            // Scroll back (into history)
            userScrollOffset += scrollAmount;
        }
        else if (scroll < 0)
        {
            // Scroll forward (toward present)
            userScrollOffset -= scrollAmount;
        }

        int currentSampleCount = SpeciesHistoryTracker.instance.SampleCount;
        int maxOffset = Mathf.Max(0, currentSampleCount - graphWidth);
        userScrollOffset = Mathf.Clamp(userScrollOffset, 0, maxOffset);

        isUserScrolled = userScrollOffset > 0;

        // Redraw at the user's chosen position
        RedrawAtOffset();
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => isHovered = false;

    private void RedrawAtOffset()
    {
        isRedrawing = true;
        ClearTexture();

        // Re-run lane optimisation
        laneAssignments.Clear();
        lanesOccupied = new bool[maxLanes];
        OptimiseLaneAssignments();

        int currentSampleCount = SpeciesHistoryTracker.instance.SampleCount;
        int endSample = currentSampleCount - userScrollOffset;
        int startSample = Mathf.Max(0, endSample - graphWidth);
        scrollOffset = startSample;

        for (int s = startSample; s < endSample; s++)
        {
            DrawNewColumn(s);
        }

        isRedrawing = false;

        // Draw scroll indicator if not live
        if (isUserScrolled)
        {
            Color32 indicatorColour = new Color32(255, 255, 255, 128);
            for (int x = graphWidth - 25; x < graphWidth - 2; x++)
            {
                for (int y = graphHeight - 10; y < graphHeight - 2; y++)
                {
                    if (x >= 0 && x < graphWidth && y >= 0 && y < graphHeight)
                        pixels[y * graphWidth + x] = indicatorColour;
                }
            }
        }

        graphTexture.SetPixels32(pixels);
        graphTexture.Apply();

        // If we've returned to live, restore normal tracking
        if (!isUserScrolled)
        {
            lastSampleCount = currentSampleCount;
            scrollOffset = Mathf.Max(0, currentSampleCount - graphWidth);
        }
    }

    private void RedrawAll()
    {
        // If user is scrolled back, use the offset-aware redraw
        if (isUserScrolled)
        {
            RedrawAtOffset();
            return;
        }

        // Clear everything and redraw from history
        isRedrawing = true;
        ClearTexture();
        scrollOffset = 0;

        // Re-run DFS optimisation to cluster related species together
        laneAssignments.Clear();
        lanesOccupied = new bool[maxLanes];
        OptimiseLaneAssignments();

        int currentSampleCount = SpeciesHistoryTracker.instance.SampleCount;
        int startSample = Mathf.Max(0, currentSampleCount - graphWidth);
        scrollOffset = startSample;

        for (int s = startSample; s < currentSampleCount; s++)
        {
            DrawNewColumn(s);
        }

        isRedrawing = false;
        lastSampleCount = currentSampleCount;
        graphTexture.SetPixels32(pixels);
        graphTexture.Apply();

    }


    private void DrawNewColumn(int sampleIndex)
    {
        int xPos = sampleIndex - scrollOffset;

        // Scroll if we've reached the right edge
        if (xPos >= graphWidth)
        {
            ScrollLeft();
            scrollOffset++;
            xPos = graphWidth - 1;
        }

        if (xPos < 0 || xPos >= graphWidth)
        {
            return;
        }

        // Clear this column
        for (int y = 0; y < graphHeight; y++)
        {
            pixels[y * graphWidth + xPos] = backgroundColor;
        }

        var entries = SpeciesHistoryTracker.instance.GetAllEntries();

        foreach (var kvp in entries)
        {
            var entry = kvp.Value;
            int speciesId = entry.speciesId;

            // Skip species that haven't been born yet or are already marked extinct
            if (entry.birthSampleIndex > sampleIndex) continue;
            if (entry.extinctionSampleIndex >= 0 && entry.extinctionSampleIndex < sampleIndex) continue;

            // Draw extinction marker
            if (entry.extinctionSampleIndex == sampleIndex && laneAssignments.ContainsKey(speciesId))
            {
                int extY = LaneToY(laneAssignments[speciesId]);
                DrawExtinctionMarker(xPos, extY);
                continue;
            }

            // Ensure species has lane before drawing
            if (!laneAssignments.ContainsKey(speciesId))
            {
                AssignLane(speciesId, entry.parentSpeciesId);
            }

            if (!laneAssignments.ContainsKey(speciesId))
            {
                continue;
            }
            int lane = laneAssignments[speciesId];

            // Get member count for this sample
            int historyIndex = sampleIndex - entry.birthSampleIndex;
            int memberCount = 0;
            if (historyIndex >= 0 && historyIndex < entry.memberCountHistory.Count)
            {
                memberCount = entry.memberCountHistory[historyIndex];
            }
            else if (entry.memberCountHistory.Count > 0)
            {
                // Use last known count if index is slightly ahead
                memberCount = entry.memberCountHistory[entry.memberCountHistory.Count - 1];
            }

            // Skip extinct species
            if (entry.extinctionSampleIndex >= 0 && entry.extinctionSampleIndex <= sampleIndex) continue;

            // Draw species line — logarithmic thickness so dominant species stand out
            int thickness = Mathf.Clamp(Mathf.CeilToInt(Mathf.Log(memberCount + 1) * 1.5f), 1, 5);
            int centerY = LaneToY(lane);

            // Live average colour of the species
            Color32 col32 = GetLiveSpeciesColour(speciesId, entry.colour);


            for (int t = -thickness / 2; t <= thickness / 2; t++)
            {
                int py = centerY + t;
                if (py >= 0 && py < graphHeight)
                {
                    pixels[py * graphWidth + xPos] = col32;
                }
            }

            // Draw branch line on the sample this species was born
            if (sampleIndex == entry.birthSampleIndex && entry.parentSpeciesId >= 0)
            {
                DrawBranch(xPos, entry.parentSpeciesId, speciesId);
            }
        }

        // Free lanes for extinct species
        if (!isRedrawing)
            FreeLanesForExtinctSpecies(sampleIndex);
    }

    // Pre-assign lanes based on family tree structure so related species are adjacent.
    // Uses depth-first traversal of the lineage tree.
    private void OptimiseLaneAssignments()
    {
        var entries = SpeciesHistoryTracker.instance.GetAllEntries();
        if (entries.Count == 0) return;

        int currentSampleCount = SpeciesHistoryTracker.instance.SampleCount;
        int visibleStart = Mathf.Max(0, currentSampleCount - graphWidth);

        // Determine which species are relevant (alive or have visible history in render window)
        HashSet<int> relevantSpecies = new HashSet<int>();
        foreach (var kvp in entries)
        {
            var entry = kvp.Value;
            bool isAlive = entry.extinctionSampleIndex < 0;
            bool visibleInWindow = entry.birthSampleIndex < currentSampleCount &&
                (entry.extinctionSampleIndex < 0 || entry.extinctionSampleIndex >= visibleStart);
            if (isAlive || visibleInWindow)
                relevantSpecies.Add(entry.speciesId);
        }

        // Build parent -> children mapping (only relevant species)
        Dictionary<int, List<int>> children = new Dictionary<int, List<int>>();
        List<int> roots = new List<int>();

        foreach (int id in relevantSpecies)
        {
            var entry = entries[id];
            int parentId = entry.parentSpeciesId;

            if (parentId < 0 || !relevantSpecies.Contains(parentId))
            {
                roots.Add(id);
            }
            else
            {
                if (!children.ContainsKey(parentId))
                    children[parentId] = new List<int>();
                children[parentId].Add(id);
            }
        }

        // Assign lanes spreading from centre — children alternate above and below parents
        laneAssignments.Clear();
        lanesOccupied = new bool[maxLanes];

        // Place roots around the centre
        int centre = maxLanes / 2;
        int nextLane = centre;

        for (int r = 0; r < roots.Count; r++)
        {
            // Spread roots from centre outward alternating
            int rootLane;
            if (r == 0)
            {
                rootLane = centre;
            }
            else
            {
                int dir = (r % 2 == 0) ? 1 : -1;
                int dist = (r + 1) / 2;
                rootLane = centre + dir * dist * 2; // Space roots apart
                rootLane = Mathf.Clamp(rootLane, 0, maxLanes - 1);
            }

            // Find nearest free lane to desired root position
            for (int offset = 0; offset < maxLanes; offset++)
            {
                int above = rootLane + offset;
                if (above < maxLanes && !lanesOccupied[above])
                {
                    rootLane = above;
                    break;
                }
                int below = rootLane - offset;
                if (below >= 0 && !lanesOccupied[below])
                {
                    rootLane = below;
                    break;
                }
            }

            laneAssignments[roots[r]] = rootLane;
            lanesOccupied[rootLane] = true;
            AssignLanesDFS(roots[r], children, ref nextLane);
        }
    }

    private void AssignLanesDFS(int speciesId, Dictionary<int, List<int>> children, ref int nextLane)
    {
        // Assign this node to the next available lane (used as fallback counter)
        if (!laneAssignments.ContainsKey(speciesId))
        {
            if (nextLane >= maxLanes) return;
            laneAssignments[speciesId] = nextLane;
            lanesOccupied[nextLane] = true;
            nextLane++;
        }

        if (!children.ContainsKey(speciesId)) return;

        int parentLane = laneAssignments[speciesId];
        var childList = children[speciesId];

        // Place children alternating above and below the parent
        for (int i = 0; i < childList.Count; i++)
        {
            int childId = childList[i];
            if (laneAssignments.ContainsKey(childId)) continue;

            // Alternate: even children go below (higher lane), odd go above (lower lane)
            int direction = (i % 2 == 0) ? 1 : -1;
            int searchStart = (i / 2) + 1;

            bool placed = false;
            for (int offset = searchStart; offset < maxLanes; offset++)
            {
                int candidate = parentLane + (direction * offset);
                if (candidate >= 0 && candidate < maxLanes && !lanesOccupied[candidate])
                {
                    laneAssignments[childId] = candidate;
                    lanesOccupied[candidate] = true;
                    placed = true;
                    break;
                }
            }

            // Fallback: try the other direction
            if (!placed)
            {
                for (int offset = 1; offset < maxLanes; offset++)
                {
                    int candidate = parentLane + (-direction * offset);
                    if (candidate >= 0 && candidate < maxLanes && !lanesOccupied[candidate])
                    {
                        laneAssignments[childId] = candidate;
                        lanesOccupied[candidate] = true;
                        placed = true;
                        break;
                    }
                }
            }

            if (!placed) continue;

            // Recurse into this child's subtree
            AssignLanesDFS(childId, children, ref nextLane);
        }
    }

    private void AssignLane(int speciesId, int parentSpeciesId)
    {
        if (parentSpeciesId >= 0 && laneAssignments.ContainsKey(parentSpeciesId))
        {
            // A divergence happened — trigger a full optimised redraw only for genuinely new species
            // (not species that simply overflowed from the DFS)
            if (!isRedrawing && !pendingRedraw)
            {
                var entries = SpeciesHistoryTracker.instance.GetAllEntries();
                bool isNewSpecies = entries.TryGetValue(speciesId, out var entry)
                    && entry.birthSampleIndex >= lastSampleCount - 1;
                if (isNewSpecies)
                {
                    pendingRedraw = true;
                    redrawRequestedTime = Time.time;
                }
            }

            // Place near parent
            int parentLane = laneAssignments[parentSpeciesId];
            for (int offset = 1; offset < maxLanes; offset++)
            {
                int below = parentLane - offset;
                if (below >= 0 && !lanesOccupied[below])
                {
                    laneAssignments[speciesId] = below;
                    lanesOccupied[below] = true;
                    return;
                }
                int above = parentLane + offset;
                if (above < maxLanes && !lanesOccupied[above])
                {
                    laneAssignments[speciesId] = above;
                    lanesOccupied[above] = true;
                    return;
                }
            }
        }

        // No parent or no space near parent — assign from centre outward
        int centre = maxLanes / 2;
        for (int offset = 0; offset < maxLanes; offset++)
        {
            int above = centre + offset;
            if (above < maxLanes && !lanesOccupied[above] && HasClearance(above))
            {
                laneAssignments[speciesId] = above;
                lanesOccupied[above] = true;
                return;
            }
            int below = centre - offset - 1;
            if (below >= 0 && !lanesOccupied[below] && HasClearance(below))
            {
                laneAssignments[speciesId] = below;
                lanesOccupied[below] = true;
                return;
            }
        }

        // Fallback: ignore clearance if no space found
        for (int i = 0; i < maxLanes; i++)
        {
            if (!lanesOccupied[i])
            {
                laneAssignments[speciesId] = i;
                lanesOccupied[i] = true;
                return;
            }
        }
    }

    // Check that no other active species is within minLaneGap of this lane
    private bool HasClearance(int lane)
    {
        for (int i = 1; i < minLaneGap; i++)
        {
            if (lane + i < maxLanes && lanesOccupied[lane + i]) return false;
            if (lane - i >= 0 && lanesOccupied[lane - i]) return false;
        }
        return true;
    }

    private void FreeLanesForExtinctSpecies(int currentSample)
    {
        var entries = SpeciesHistoryTracker.instance.GetAllEntries();
        List<int> toFree = new List<int>();

        foreach (var kvp in laneAssignments)
        {
            int speciesId = kvp.Key;
            if (!entries.ContainsKey(speciesId))
            {
                // Species was wiped from history (didn't survive long enough)
                toFree.Add(speciesId);
            }
            else if (entries.TryGetValue(speciesId, out var entry))
            {
                // Free lane if extinct for more than 5 samples
                if (entry.extinctionSampleIndex >= 0 && currentSample - entry.extinctionSampleIndex > 5)
                {
                    toFree.Add(speciesId);
                }
            }
        }

        if (toFree.Count > 0)
        {
            foreach (int id in toFree)
            {
                int lane = laneAssignments[id];
                lanesOccupied[lane] = false;
                laneAssignments.Remove(id);
            }
            // Just free lanes silently — no redraw needed, old pixels remain and scroll off naturally
        }
    }

    private bool pendingRedraw = false;
    private float redrawRequestedTime = -1f;

    private void DrawBranch(int xPos, int parentSpeciesId, int childSpeciesId)
    {
        if (!laneAssignments.ContainsKey(childSpeciesId)) return;

        int childLane = laneAssignments[childSpeciesId];
        int childY = LaneToY(childLane);
        int parentY;

        if (laneAssignments.ContainsKey(parentSpeciesId))
        {
            int parentLane = laneAssignments[parentSpeciesId];
            parentY = LaneToY(parentLane);
        }
        else
        {
            return;
        }

        // Thick white branch line from parent centre to child lane
        int branchWidth = 12;
        int startX = Mathf.Max(0, xPos - branchWidth);

        Color32 white = new Color32(255, 255, 255, 230);
        DrawThickLine(startX, parentY, xPos, childY, white, 1);
    }

    private void DrawExtinctionMarker(int xPos, int yPos)
    {
        Color32 red = new Color32(255, 50, 50, 255);
        int size = 5;
        int thickness = 2;
        // Draw a thick X
        for (int i = -size; i <= size; i++)
        {
            for (int t = -thickness; t <= thickness; t++)
            {
                int px = xPos + i;
                int py1 = yPos + i + t;
                int py2 = yPos - i + t;
                if (px >= 0 && px < graphWidth)
                {
                    if (py1 >= 0 && py1 < graphHeight) pixels[py1 * graphWidth + px] = red;
                    if (py2 >= 0 && py2 < graphHeight) pixels[py2 * graphWidth + px] = red;
                }
            }
        }
    }

    private void DrawThickLine(int x0, int y0, int x1, int y1, Color32 colour, int radius)
    {
        // Bresenham but draw a filled square at each point for true thickness
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            // Draw a filled square of size radius*2 at each point
            for (int ry = -radius; ry <= radius; ry++)
            {
                for (int rx = -radius; rx <= radius; rx++)
                {
                    int px = x0 + rx;
                    int py = y0 + ry;
                    if (px >= 0 && px < graphWidth && py >= 0 && py < graphHeight)
                    {
                        pixels[py * graphWidth + px] = colour;
                    }
                }
            }

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    // Cached species colours — updated every N samples rather than every column
    private Dictionary<int, Color32> cachedSpeciesColours = new Dictionary<int, Color32>();
    private int lastColourUpdateSample = -1;
    private int colourUpdateInterval = 5; // Update colours every 5 samples

    private Color32 GetLiveSpeciesColour(int speciesId, Color fallback)
    {
        // Update cache periodically
        int currentSample = SpeciesHistoryTracker.instance != null ? SpeciesHistoryTracker.instance.SampleCount : 0;
        if (currentSample - lastColourUpdateSample >= colourUpdateInterval)
        {
            lastColourUpdateSample = currentSample;
            UpdateColourCache();
        }

        if (cachedSpeciesColours.TryGetValue(speciesId, out Color32 cached))
            return cached;
        return fallback;
    }

    private void UpdateColourCache()
    {
        if (SpeciationManager.instance == null) return;

        cachedSpeciesColours.Clear();
        foreach (var s in SpeciationManager.instance.species)
        {
            if (s.members.Count == 0) continue;

            // Sample up to 10 members for colour average (not all)
            float r = 0, g = 0, b = 0;
            int sampleCount = Mathf.Min(s.members.Count, 10);
            for (int i = 0; i < sampleCount; i++)
            {
                Agent a = s.members[i];
                if (a == null) continue;
                r += a.colour.r;
                g += a.colour.g;
                b += a.colour.b;
            }
            cachedSpeciesColours[s.id] = new Color(r / sampleCount, g / sampleCount, b / sampleCount);
        }
    }

    private void DrawLine(int x0, int y0, int x1, int y1, Color colour)
    {
        TextureDrawUtils.DrawLine(pixels, graphWidth, graphHeight, x0, y0, x1, y1, colour);
    }

    private int LaneToY(int lane)
    {
        // Fixed lane positions — species stay on the same row permanently
        int margin = 15;
        int usableHeight = graphHeight - 2 * margin;
        return margin + (lane * usableHeight / Mathf.Max(maxLanes - 1, 1));
    }

    private void ScrollLeft()
    {
        // Shift all pixels left by 1 column
        for (int y = 0; y < graphHeight; y++)
        {
            int rowStart = y * graphWidth;
            for (int x = 0; x < graphWidth - 1; x++)
            {
                pixels[rowStart + x] = pixels[rowStart + x + 1];
            }
            // Clear rightmost column
            pixels[rowStart + graphWidth - 1] = backgroundColor;
        }
    }

    private void ClearTexture()
    {
        pixels = new Color32[graphWidth * graphHeight];
        Color32 bg = backgroundColor;
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = bg;
        }
        graphTexture.SetPixels32(pixels);
        graphTexture.Apply();
    }

    public System.Action<int> OnSpeciesClicked;

    private GUIStyle labelStyle;

    private void OnGUI()
    {
        if (SpeciesHistoryTracker.instance == null) return;
        if (graphImage == null || !graphImage.enabled) return;
        if (!graphImage.gameObject.activeInHierarchy) return;

        // Create style once
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 10;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.hover.textColor = Color.white;
        }

        // Get the RawImage's screen rect
        RectTransform rt = graphImage.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        // Convert to screen space
        Canvas canvas = graphImage.canvas;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 screenMin = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 screenMax = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

        float rectX = screenMin.x;
        float rectY = Screen.height - screenMax.y; // GUI Y is flipped
        float rectWidth = screenMax.x - screenMin.x;
        float rectHeight = screenMax.y - screenMin.y;

        var entries = SpeciesHistoryTracker.instance.GetAllEntries();

        foreach (var kvp in laneAssignments)
        {
            int speciesId = kvp.Key;
            int lane = kvp.Value;

            if (!entries.TryGetValue(speciesId, out var entry)) continue;
            if (entry.extinctionTime >= 0) continue; // Don't label extinct species

            // Map lane Y to screen position within the RawImage rect
            float normalizedY = 1f - ((float)LaneToY(lane) / graphHeight); // Flip Y
            float screenY = rectY + normalizedY * rectHeight;

            labelStyle.normal.textColor = entry.colour;
            if (GUI.Button(new Rect(rectX + 4, screenY - 7, 100, 20), entry.speciesName, labelStyle))
            {
                OnSpeciesClicked?.Invoke(speciesId);
            }
        }
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Toggleable analytics panel with tabbed views.
/// Tab key toggles the panel. 1/2/3 keys switch between sub-tabs.
/// </summary>
public class AnalyticsPanel : MonoBehaviour
{
    [Header("Panel")]
    public GameObject analyticsPanel; // The panel root to show/hide
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Tab Contents")]
    public GameObject overviewTab;
    public GameObject attributesTab;

    [Header("Tab Buttons")]
    public TMP_Text[] tabLabels;

    [Header("Text Stats (Overview Tab)")]
    public TMP_Text statsText;

    [Header("Graph References")]
    public PopulationGraph populationGraph;
    public PhylogeneticDiagram phylogeneticDiagram;
    public AttributeGraph attributeGraph;

    // Tab state
    private int activeTab = 0; // 0 = overview, 1 = attributes
    private GameObject[] tabs;

    // Tracked stats
    private float totalFoodEaten = 0;
    private float totalPoisonEaten = 0;
    private float totalBirths = 0;
    private float totalDeaths = 0;
    private float simStartTime;
    private int peakPopulation = 0;
    private int peakGeneration = 0;
    private int speciesCreatedTotal = 0;
    private int speciesExtinctTotal = 0;
    private float totalLifetimeSum = 0;
    private int totalDeathsForLifetime = 0;

    private void OnEnable()
    {
        SimEvents.OnAgentBorn += OnBirth;
        SimEvents.OnAgentDied += OnDeath;
        SimEvents.OnFoodConsumed += OnFoodEaten;
        SimEvents.OnSpeciesCreated += OnSpeciesCreated;
        SimEvents.OnSpeciesExtinct += OnSpeciesExtinct;
    }

    private void OnDisable()
    {
        SimEvents.OnAgentBorn -= OnBirth;
        SimEvents.OnAgentDied -= OnDeath;
        SimEvents.OnFoodConsumed -= OnFoodEaten;
        SimEvents.OnSpeciesCreated -= OnSpeciesCreated;
        SimEvents.OnSpeciesExtinct -= OnSpeciesExtinct;
    }

    private void OnBirth(Agent a) => totalBirths++;
    private void OnDeath(Agent a)
    {
        totalDeaths++;
        totalLifetimeSum += a.age;
        totalDeathsForLifetime++;
    }
    private void OnFoodEaten(Food f)
    {
        if (f.isPoison) totalPoisonEaten++;
        else totalFoodEaten++;
    }
    private void OnSpeciesCreated(int id, int parentId) => speciesCreatedTotal++;
    private void OnSpeciesExtinct(int id) => speciesExtinctTotal++;

    private void Start()
    {
        simStartTime = Time.time;
        if (analyticsPanel != null)
            analyticsPanel.SetActive(false);

        // Build tabs array from assigned references
        tabs = new GameObject[] { overviewTab, attributesTab };
        SetActiveTab(0);
    }

    private void Update()
    {
        // Toggle panel
        if (Input.GetKeyDown(toggleKey))
        {
            if (analyticsPanel != null)
                analyticsPanel.SetActive(!analyticsPanel.activeSelf);
        }

        if (analyticsPanel == null || !analyticsPanel.activeSelf) return;

        // Tab switching with number keys (only when panel is open)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetActiveTab(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetActiveTab(1);

        // Update stats text when overview tab is visible
        if (activeTab == 0 && statsText != null)
        {
            UpdateStatsText();
        }
    }

    public void SetActiveTab(int tabIndex)
    {
        activeTab = tabIndex;

        // Show/hide tab content
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] != null)
                tabs[i].SetActive(i == activeTab);
        }

        // Highlight active tab label
        if (tabLabels != null)
        {
            for (int i = 0; i < tabLabels.Length; i++)
            {
                if (tabLabels[i] == null) continue;
                tabLabels[i].color = (i == activeTab)
                    ? Color.white
                    : new Color(0.5f, 0.5f, 0.5f, 0.7f);
            }
        }
    }

    private void UpdateStatsText()
    {
        int pop = AgentManager.instance.population;
        int avgGen = AgentManager.instance.avgGeneration;
        float worldTime = SimulationManager.instance.worldTime;

        if (pop > peakPopulation) peakPopulation = pop;
        if (avgGen > peakGeneration) peakGeneration = avgGen;

        int activeSpecies = 0;
        if (SpeciationManager.instance != null)
        {
            foreach (var s in SpeciationManager.instance.species)
            {
                if (s.members.Count > 0) activeSpecies++;
            }
        }

        // Calculate averages from current population
        float avgSize = 0, avgSpeed = 0, avgEnergy = 0, avgConns = 0;
        float avgRepRange = 0, avgVisionRange = 0;
        var agents = AgentManager.instance.agents;
        if (pop > 0)
        {
            for (int i = 0; i < agents.Count; i++)
            {
                Agent a = agents[i];
                if (a == null) continue;
                avgSize += a.size;
                avgSpeed += a.speed;
                avgEnergy += a.energy;
                avgConns += a.genome.connectionGenes.Count;
                avgRepRange += a.reproductionRange;
                avgVisionRange += a.visionDistance;
            }
            avgSize /= pop;
            avgSpeed /= pop;
            avgEnergy /= pop;
            avgConns /= pop;
            avgRepRange /= pop;
            avgVisionRange /= pop;
        }

        statsText.text =
            $"<b>SIMULATION ANALYTICS</b>\n" +
            $"World Time: {worldTime:F0}s\n" +
            $"\n<b>Population</b>\n" +
            $"Current: {pop}  |  Peak: {peakPopulation}\n" +
            $"Births: {totalBirths:F0}  |  Deaths: {totalDeaths:F0}\n" +
            $"Avg Lifetime: {(totalDeathsForLifetime > 0 ? totalLifetimeSum / totalDeathsForLifetime : 0):F1}\n" +
            $"\n<b>Evolution</b>\n" +
            $"Avg Generation: {avgGen}  |  Peak: {peakGeneration}\n" +
            $"Active Species: {activeSpecies}\n" +
            $"Species Created: {speciesCreatedTotal}  |  Extinct: {speciesExtinctTotal}\n" +
            $"\n<b>Population Averages</b>\n" +
            $"Size: {avgSize:F2}  |  Speed: {avgSpeed:F2}\n" +
            $"Vision: {avgVisionRange:F1}  |  Rep Range: {avgRepRange:F1}\n" +
            $"Energy: {avgEnergy:F1}  |  Connections: {avgConns:F1}\n" +
            $"\n<b>Resources</b>\n" +
            $"Food Eaten: {totalFoodEaten:F0}  |  Poison Eaten: {totalPoisonEaten:F0}\n" +
            $"Food in World: {FoodSpawner.instance.foodList.Count}";
    }

    private GUIStyle tabBarStyle;
    private GUIStyle activeTabStyle;
    private GUIStyle inactiveTabStyle;

    private void OnGUI()
    {
        if (analyticsPanel == null || !analyticsPanel.activeSelf) return;

        // Create styles once
        if (tabBarStyle == null)
        {
            tabBarStyle = new GUIStyle(GUI.skin.box);
            tabBarStyle.normal.background = MakeTexture(1, 1, new Color(0.1f, 0.1f, 0.15f, 0.95f));
            tabBarStyle.alignment = TextAnchor.MiddleCenter;
        }
        if (activeTabStyle == null)
        {
            activeTabStyle = new GUIStyle(GUI.skin.label);
            activeTabStyle.fontSize = 13;
            activeTabStyle.fontStyle = FontStyle.Bold;
            activeTabStyle.normal.textColor = Color.white;
            activeTabStyle.alignment = TextAnchor.MiddleCenter;
        }
        if (inactiveTabStyle == null)
        {
            inactiveTabStyle = new GUIStyle(GUI.skin.label);
            inactiveTabStyle.fontSize = 12;
            inactiveTabStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            inactiveTabStyle.alignment = TextAnchor.MiddleCenter;
        }

        // Draw tab bar at the top-center of the screen
        float barWidth = 300f;
        float barHeight = 28f;
        float barX = (Screen.width - barWidth) / 2f;
        float barY = 8f;

        GUI.Box(new Rect(barX, barY, barWidth, barHeight), "", tabBarStyle);

        float tabWidth = barWidth / 2f;

        // Tab 1: Overview
        GUIStyle style1 = activeTab == 0 ? activeTabStyle : inactiveTabStyle;
        if (GUI.Button(new Rect(barX, barY, tabWidth, barHeight), "[1] Overview", style1))
            SetActiveTab(0);

        // Tab 2: Attributes
        GUIStyle style2 = activeTab == 1 ? activeTabStyle : inactiveTabStyle;
        if (GUI.Button(new Rect(barX + tabWidth, barY, tabWidth, barHeight), "[2] Attributes", style2))
            SetActiveTab(1);
    }

    private Texture2D MakeTexture(int width, int height, Color colour)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = colour;
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Toggleable analytics panel (with tab key)
public class AnalyticsPanel : MonoBehaviour
{
    [Header("Panel")]
    public GameObject analyticsPanel;
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Tab Contents")]
    public GameObject overviewTab;
    public GameObject attributesTab;
    public GameObject speciesTab;

    [Header("Tab Buttons")]
    public TMP_Text[] tabLabels;

    [Header("Text Stats (Overview Tab)")]
    public TMP_Text statsText;

    [Header("Graph References")]
    public PopulationGraph populationGraph;
    public PhylogeneticDiagram phylogeneticDiagram;
    public AttributeGraph attributeGraph;
    public SpeciesDetailPanel speciesDetailPanel;

    // Tab state
    private int activeTab = 0; // 0 = overview, 1 = attributes
    private GameObject[] tabs;

    // Tracked stats
    private float totalFoodEaten = 0;
    private float totalMeatEaten = 0;
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
        if (f is PoisonFood) totalPoisonEaten++;
        else if (f is MeatFood) totalMeatEaten++;
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
        tabs = new GameObject[] { overviewTab, attributesTab, speciesTab };
        SetActiveTab(0);

        // Wire species click event
        if (phylogeneticDiagram != null && speciesDetailPanel != null)
            phylogeneticDiagram.OnSpeciesClicked += speciesDetailPanel.Show;
    }

    private void Update()
    {
        // Always sample attribute data regardless of panel/tab visibility
        if (attributeGraph != null)
            attributeGraph.TrySample();

        if (Input.GetKeyDown(toggleKey))
        {
            if (analyticsPanel != null)
                analyticsPanel.SetActive(!analyticsPanel.activeSelf);
        }

        if (analyticsPanel == null || !analyticsPanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetActiveTab(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetActiveTab(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetActiveTab(2);

        if (activeTab == 0 && statsText != null)
        {
            UpdateStatsText();
        }
    }

    public void SetActiveTab(int tabIndex)
    {
        activeTab = tabIndex;

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
                    : new Color(0.5f, 0.5f, 0.5f, 1.0f);
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
        float avgVisionRange = 0, avgDiet = 0;
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
                avgVisionRange += a.visionDistance;
                avgDiet += a.dietPreference;
            }
            avgSize /= pop;
            avgSpeed /= pop;
            avgEnergy /= pop;
            avgConns /= pop;
            avgVisionRange /= pop;
            avgDiet /= pop;
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
            $"Vision: {avgVisionRange:F1}\n" +
            $"Energy: {avgEnergy:F1}  |  Connections: {avgConns:F1}\n" +
            $"Diet: {avgDiet:F2} (0=herb, 1=carn)\n" +
            $"\n<b>Resources</b>\n" +
            $"Plant Eaten: {totalFoodEaten:F0}  |  Meat Eaten: {totalMeatEaten:F0}  |  Poison: {totalPoisonEaten:F0}\n" +
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

        float barWidth = 300f;
        float barHeight = 28f;
        float barX = (Screen.width - barWidth) / 2f;
        float barY = 8f;

        GUI.Box(new Rect(barX, barY, barWidth, barHeight), "", tabBarStyle);

        float tabWidth = barWidth / 3f;

        // Tab 1: Overview
        GUIStyle style1 = activeTab == 0 ? activeTabStyle : inactiveTabStyle;
        if (GUI.Button(new Rect(barX, barY, tabWidth, barHeight), "[1] Overview", style1))
            SetActiveTab(0);

        // Tab 2: Attributes
        GUIStyle style2 = activeTab == 1 ? activeTabStyle : inactiveTabStyle;
        if (GUI.Button(new Rect(barX + tabWidth, barY, tabWidth, barHeight), "[2] Attributes", style2))
            SetActiveTab(1);

        // Tab 3: Species
        GUIStyle style3 = activeTab == 2 ? activeTabStyle : inactiveTabStyle;
        if (GUI.Button(new Rect(barX + tabWidth * 2, barY, tabWidth, barHeight), "[3] Species", style3))
            SetActiveTab(2);
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

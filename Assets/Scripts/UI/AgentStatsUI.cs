using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgentStatsUI : MonoBehaviour
{
    public static AgentStatsUI instance;
    [Header("UI Elements")]
    public GameObject agentStatsPanel; // Reference to the UI panel
    public TMP_Text nameText;
    public TMP_Text energyText;
    public TMP_Text attributesText;
    public Agent selectedAgent;
    public NeuralNetworkVisualiser NNVisualiser;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            NNVisualiser = GetComponent<NeuralNetworkVisualiser>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SimEvents.OnAgentDied += HandleAgentDied;
    }

    private void OnDisable()
    {
        SimEvents.OnAgentDied -= HandleAgentDied;
    }

    private void HandleAgentDied(Agent agent)
    {
        if (selectedAgent == agent)
        {
            HideAgentStats();
        }
    }

    // Functions to control AgentStats panel visibility
    public void ShowAgentStats(Agent agent)
    {
        selectedAgent = agent;
        agentStatsPanel.SetActive(true);
        NNVisualiser.Visualise(selectedAgent.network);
        UpdateUI();
    }

    public void HideAgentStats()
    {
        selectedAgent = null;
        agentStatsPanel.SetActive(false);
    }

    private void UpdateUI()
    {
        if (selectedAgent != null)
        {
            //Update all text values
            string speciesName = GetSpeciesName(selectedAgent.speciesId);
            nameText.text = $"ID: {selectedAgent.id}    Species: {speciesName}    Gen: {selectedAgent.generation}";
            energyText.text = $"Age: {selectedAgent.age} \nEnergy: {selectedAgent.energy:F0} / {selectedAgent.maxEnergy:F0} \nHealth: {selectedAgent.health} \nMetabolism Cost: {selectedAgent.metabolismCost} \n";
            attributesText.text = $"Size: {selectedAgent.size} \nSpeed: {selectedAgent.speed} \nVision Distance: {selectedAgent.visionDistance} \nVision Angle: {selectedAgent.visionAngle} \nMutation Chance: {selectedAgent.mutationChance} \nMutation Magnitude: {selectedAgent.mutationMagnitude} \nIs Fertile: {selectedAgent.isFertile()} \nReproductive Cost: {selectedAgent.reproductionEnergyCost} \nOffspring No.: {selectedAgent.offspringCount}";

        }
    }

    private string GetSpeciesName(int speciesId)
    {
        if (SpeciationManager.instance == null) return "Unknown";
        foreach (var s in SpeciationManager.instance.species)
        {
            if (s.id == speciesId) return s.speciesName;
        }
        return "Unknown";
    }

    private void Start()
    {
        //Initially hide the panel as no agent is selected
        HideAgentStats();
    }

    private void Update()
    {
        //Constantly update UI if the panel is active
        if (selectedAgent != null)
        {
            UpdateUI();
        }
    }
}
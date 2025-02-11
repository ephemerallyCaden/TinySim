using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgentStatsUI : MonoBehaviour
{
    [NonSerialized] public static AgentStatsUI instance;
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
            nameText.text = $"ID: {selectedAgent.id}                Generation: {selectedAgent.generation}";
            energyText.text = $"Age: {selectedAgent.age} \nEnergy: {selectedAgent.energy} \nMax Energy: {selectedAgent.maxEnergy} \nHealth: {selectedAgent.health} \nMetabolism Cost: {selectedAgent.metabolismCost} \n";
            attributesText.text = $"Size: {selectedAgent.size} \nSpeed: {selectedAgent.speed} \nVision Distance: {selectedAgent.visionDistance} \nVision Angle: {selectedAgent.visionAngle} \nMutation Chance: {selectedAgent.mutationChance} \nMutation Magnitude: {selectedAgent.mutationMagnitude} \nIs Fertile: {selectedAgent.isFertile()} \nReproductive Cost: {selectedAgent.reproductionEnergyCost} \nOffspring No.: {selectedAgent.offspringCount}";

        }
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
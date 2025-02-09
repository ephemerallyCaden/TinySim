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
            nameText.text = $"ID: {selectedAgent.id}";
            energyText.text = $"Age: {selectedAgent.age} \nEnergy: {selectedAgent.energy}\nHealth: {selectedAgent.health}";
            //energyText.text = $"Energy: {selectedAgent.energy}";
            //healthText.text = $"Health: {selectedAgent.health}";

        }
    }

    private void Start()
    {
        HideAgentStats();
    }

    private void Update()
    {
        if (selectedAgent != null)
        {
            UpdateUI();
        }
    }
}
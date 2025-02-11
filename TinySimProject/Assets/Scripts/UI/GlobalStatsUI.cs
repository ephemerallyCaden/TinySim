using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalStatsUI : MonoBehaviour
{
    public TMP_Text statsText;

    public void UpdateUI()
    {
        // Update statistics text
        statsText.text = $"Time: {(int)SimulationManager.instance.worldTime} \nGeneration: {AgentManager.instance.avgGeneration} \nPopulation: {AgentManager.instance.population}";
    }
    private void FixedUpdate()
    {
        UpdateUI();
    }
}

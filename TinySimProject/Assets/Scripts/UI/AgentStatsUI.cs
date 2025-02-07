using UnityEngine;
using UnityEngine.UI;

public class AgentStatsUI : MonoBehaviour
{
    public Text agentIdText;
    public Text agentAttributesText;
    public GameObject statsPanel;

    public void ShowAgentStats(Agent agent)
    {
        // Display stats in the UI
        agentIdText.text = "ID: " + agent.id.ToString();
        agentAttributesText.text = "Energy: " + agent.energy.ToString() + "\n" +
                                   "Size: " + agent.size.ToString() + "\n" +
                                   "Speed: " + agent.speed.ToString();

        statsPanel.SetActive(true); // Show the stats panel
    }

    public void HideAgentStats()
    {
        statsPanel.SetActive(false); // Hide the stats panel
    }
}
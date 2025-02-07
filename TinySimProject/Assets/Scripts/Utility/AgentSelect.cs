using UnityEngine;

public class AgentSelect : MonoBehaviour
{
    private Camera mainCamera;
    private Agent selectedAgent; // Reference to the currently selected agent
    public AgentStatsUI agentStatsUI; // Reference to the UI manager

    private void Start()
    {
        mainCamera = Camera.main; // Cache the main camera
    }

    private void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            SelectAgent();
        }
    }

    private void SelectAgent()
    {
        // Create a ray from the mouse position
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        // Check if the ray hit an agent
        if (hit.collider != null)
        {
            Agent agent = hit.collider.GetComponent<Agent>();
            if (agent != null)
            {
                // Select the agent
                selectedAgent = agent;
                Debug.Log($"Selected Agent: {agent.id}");
                // TODO: Open the UI window and display agent statistics
                selectedAgent = agent;
                agentStatsUI.ShowAgentStats(agent); // Show the UI window
            }
        }
        else
        {
            // Deselect the agent if clicking outside
            selectedAgent = null;
            Debug.Log("Deselected Agent");
            // TODO: Close the UI window
            selectedAgent = null;
            agentStatsUI.HideAgentStats(); // Hide the UI window
        }
    }

    public Agent GetSelectedAgent()
    {
        return selectedAgent;
    }
}

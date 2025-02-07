using UnityEngine;

public class AgentClicker : MonoBehaviour
{
    public float agentRadius = 1f; // Radius for click detection
    public Agent selectedAgent;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Ensure we are working in 2D space

            RaycastHit2D hit = Physics2D.CircleCast(mousePosition, agentRadius, Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Agent"))
            {
                selectedAgent = hit.collider.GetComponent<Agent>();
                ShowAgentStats(selectedAgent);
            }
        }
    }

    void ShowAgentStats(Agent agent)
    {
        //AgentStatsUI.
    }
}

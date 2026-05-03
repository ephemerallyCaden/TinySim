using UnityEngine;
using UnityEngine.EventSystems;

public class AgentSelect : MonoBehaviour
{
    private Camera mainCamera;
    private Agent selectedAgent; // Reference to the currently selected agent
    public AgentStatsUI agentStatsUI; // Reference to the UI manager

    // Drag detection
    private Vector2 mouseDownPosition;
    private bool mouseIsDown = false;
    private const float dragThreshold = 5f; // Pixels of movement before considered a drag

    private void Start()
    {
        mainCamera = Camera.main; // Cache the main camera for quick access
    }

    private void Update()
    {
        // Track mouse down
        if (Input.GetMouseButtonDown(0))
        {
            // Don't track clicks over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            mouseDownPosition = Input.mousePosition;
            mouseIsDown = true;
        }

        // On mouse up, check if it was a click (not a drag)
        if (Input.GetMouseButtonUp(0) && mouseIsDown)
        {
            mouseIsDown = false;

            Vector2 mouseUpPosition = Input.mousePosition;
            float distance = Vector2.Distance(mouseDownPosition, mouseUpPosition);

            // Only select/deselect if the mouse didn't move significantly (not a drag)
            if (distance < dragThreshold)
            {
                SelectAgent();
            }
        }
    }

    private void SelectAgent()
    {
        // Create a ray from the mouse position
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        // Check if the ray hit an agent
        if (hit.collider == null || hit.collider.GetComponent<Agent>() == null)
        {

            // Deselect the agent if clicking outside
            selectedAgent = null;
            agentStatsUI.HideAgentStats();
        }
        else
        {
            Agent agent = hit.collider.GetComponent<Agent>();
            selectedAgent = agent;
            agentStatsUI.ShowAgentStats(agent);
        }
    }

    public Agent GetSelectedAgent()
    {
        return selectedAgent;
    }
}

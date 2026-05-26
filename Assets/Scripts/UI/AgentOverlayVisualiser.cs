using UnityEngine;

// Draws vision cone and reproduction range overlays for the selected agent.
public class AgentOverlayVisualiser : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color visionConeColour = new Color(0.2f, 0.8f, 1f, 0.15f);
    public Color visionOutlineColour = new Color(0.2f, 0.8f, 1f, 0.6f);
    public Color eatingRangeColour = new Color(0.2f, 0.8f, 0.2f, 0.08f);
    public Color eatingOutlineColour = new Color(0.2f, 0.8f, 0.2f, 0.4f);
    public Color reproductionRangeColour = new Color(1f, 0.4f, 0.7f, 0.08f);
    public Color reproductionOutlineColour = new Color(1f, 0.4f, 0.7f, 0.4f);
    public Color attackRangeColour = new Color(1f, 0.6f, 0.0f, 0.08f);
    public Color attackOutlineColour = new Color(1f, 0.6f, 0.0f, 0.4f);

    [Header("Rendering")]
    public int coneSegments = 30;
    public Material overlayMaterial; // Assign a simple unlit transparent material

    private AgentSelect agentSelect;
    private Mesh visionMesh;
    private Mesh reproductionMesh;
    private AnalyticsPanel analyticsPanel;

    private bool IsAnalyticsPanelOpen =>
        analyticsPanel != null && analyticsPanel.analyticsPanel != null && analyticsPanel.analyticsPanel.activeSelf;

    private void Start()
    {
        agentSelect = GetComponent<AgentSelect>();
        analyticsPanel = FindObjectOfType<AnalyticsPanel>();
        visionMesh = new Mesh();
        reproductionMesh = new Mesh();
    }

    private void Update()
    {
        // Label positions are updated here for OnGUI
        Agent agent = agentSelect != null ? agentSelect.GetSelectedAgent() : null;
        if (agent == null) return;

        Vector3 pos = agent.transform.position;
        float facing = agent.rotation;
        float halfAngle = agent.visionAngle;
        float visionRadius = agent.visionDistance;
        visionLabelWorldPos = pos + new Vector3(
            visionRadius * 0.6f * Mathf.Cos((facing + halfAngle * 0.5f) * Mathf.Deg2Rad),
            visionRadius * 0.6f * Mathf.Sin((facing + halfAngle * 0.5f) * Mathf.Deg2Rad), 0);

        eatingLabelWorldPos = pos + new Vector3(agent.eatingRadius * 0.7f, 0, 0);
        reproductionLabelWorldPos = pos + new Vector3(0, agent.reproductionRange * 0.7f, 0);
        attackLabelWorldPos = pos + new Vector3(-agent.attackRange * 0.7f, 0, 0);
    }

    private void DrawVisionCone(Agent agent)
    {
        float radius = agent.visionDistance;
        float halfAngle = agent.visionAngle;
        float facing = agent.rotation;
        Vector3 pos = agent.transform.position;

        DrawCone(pos, facing, halfAngle, radius, visionConeColour);
        DrawArc(pos, facing, halfAngle, radius, visionOutlineColour);
    }

    private void DrawRangeCircles(Agent agent)
    {
        Vector3 pos = agent.transform.position;
        DrawCircle(pos, agent.eatingRadius, eatingRangeColour, eatingOutlineColour);
        DrawCircle(pos, agent.reproductionRange, reproductionRangeColour, reproductionOutlineColour);
        DrawCircle(pos, agent.attackRange, attackRangeColour, attackOutlineColour);
    }

    private void DrawCone(Vector3 centre, float facingDeg, float halfAngleDeg, float radius, Color colour)
    {
        float startAngle = (facingDeg - halfAngleDeg) * Mathf.Deg2Rad;
        float endAngle = (facingDeg + halfAngleDeg) * Mathf.Deg2Rad;
        float step = (endAngle - startAngle) / coneSegments;

        if (overlayMaterial != null) overlayMaterial.SetPass(0);
        GL.Begin(GL.TRIANGLES);
        GL.Color(colour);

        for (int i = 0; i < coneSegments; i++)
        {
            float a1 = startAngle + step * i;
            float a2 = startAngle + step * (i + 1);

            GL.Vertex3(centre.x, centre.y, 0);
            GL.Vertex3(centre.x + radius * Mathf.Cos(a1), centre.y + radius * Mathf.Sin(a1), 0);
            GL.Vertex3(centre.x + radius * Mathf.Cos(a2), centre.y + radius * Mathf.Sin(a2), 0);
        }

        GL.End();
    }

    private void DrawArc(Vector3 centre, float facingDeg, float halfAngleDeg, float radius, Color colour)
    {
        float startAngle = (facingDeg - halfAngleDeg) * Mathf.Deg2Rad;
        float endAngle = (facingDeg + halfAngleDeg) * Mathf.Deg2Rad;
        float step = (endAngle - startAngle) / coneSegments;

        if (overlayMaterial != null) overlayMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(colour);

        // Arc edge
        for (int i = 0; i < coneSegments; i++)
        {
            float a1 = startAngle + step * i;
            float a2 = startAngle + step * (i + 1);
            GL.Vertex3(centre.x + radius * Mathf.Cos(a1), centre.y + radius * Mathf.Sin(a1), 0);
            GL.Vertex3(centre.x + radius * Mathf.Cos(a2), centre.y + radius * Mathf.Sin(a2), 0);
        }

        // Side lines from centre to arc edges
        GL.Vertex3(centre.x, centre.y, 0);
        GL.Vertex3(centre.x + radius * Mathf.Cos(startAngle), centre.y + radius * Mathf.Sin(startAngle), 0);
        GL.Vertex3(centre.x, centre.y, 0);
        GL.Vertex3(centre.x + radius * Mathf.Cos(endAngle), centre.y + radius * Mathf.Sin(endAngle), 0);

        GL.End();
    }

    private void DrawCircle(Vector3 centre, float radius, Color fillColour, Color outlineColour)
    {
        float step = 2f * Mathf.PI / coneSegments;

        // Fill
        if (overlayMaterial != null) overlayMaterial.SetPass(0);
        GL.Begin(GL.TRIANGLES);
        GL.Color(fillColour);

        for (int i = 0; i < coneSegments; i++)
        {
            float a1 = step * i;
            float a2 = step * (i + 1);
            GL.Vertex3(centre.x, centre.y, 0);
            GL.Vertex3(centre.x + radius * Mathf.Cos(a1), centre.y + radius * Mathf.Sin(a1), 0);
            GL.Vertex3(centre.x + radius * Mathf.Cos(a2), centre.y + radius * Mathf.Sin(a2), 0);
        }

        GL.End();

        // Outline
        GL.Begin(GL.LINES);
        GL.Color(outlineColour);

        for (int i = 0; i < coneSegments; i++)
        {
            float a1 = step * i;
            float a2 = step * (i + 1);
            GL.Vertex3(centre.x + radius * Mathf.Cos(a1), centre.y + radius * Mathf.Sin(a1), 0);
            GL.Vertex3(centre.x + radius * Mathf.Cos(a2), centre.y + radius * Mathf.Sin(a2), 0);
        }

        GL.End();
    }

    private Vector3 visionLabelWorldPos;
    private Vector3 eatingLabelWorldPos;
    private Vector3 reproductionLabelWorldPos;
    private Vector3 attackLabelWorldPos;

    private void OnGUI()
    {
        if (IsAnalyticsPanelOpen) return;

        Agent agent = agentSelect != null ? agentSelect.GetSelectedAgent() : null;
        if (agent == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Vision label
        Vector3 screenPos = cam.WorldToScreenPoint(visionLabelWorldPos);
        if (screenPos.z > 0)
        {
            GUI.color = visionOutlineColour;
            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 100, 20), "Vision");
        }

        // Eating label
        screenPos = cam.WorldToScreenPoint(eatingLabelWorldPos);
        if (screenPos.z > 0)
        {
            GUI.color = eatingOutlineColour;
            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 100, 20), "Eating");
        }

        // Reproduction label
        screenPos = cam.WorldToScreenPoint(reproductionLabelWorldPos);
        if (screenPos.z > 0)
        {
            GUI.color = reproductionOutlineColour;
            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 120, 20), "Reproduction");
        }

        // Attack label
        screenPos = cam.WorldToScreenPoint(attackLabelWorldPos);
        if (screenPos.z > 0)
        {
            GUI.color = attackOutlineColour;
            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 100, 20), "Attack");
        }
    }

    // GL drawing must happen in OnRenderObject
    private void OnRenderObject()
    {
        if (IsAnalyticsPanelOpen) return;

        Agent agent = agentSelect != null ? agentSelect.GetSelectedAgent() : null;
        if (agent == null) return;

        // Set GL matrix to camera's view so world coords render correctly
        Camera cam = Camera.main;
        if (cam == null) return;
        GL.PushMatrix();
        GL.LoadProjectionMatrix(cam.projectionMatrix);
        GL.modelview = cam.worldToCameraMatrix;

        DrawRangeCircles(agent);
        DrawVisionCone(agent);

        GL.PopMatrix();
    }
}

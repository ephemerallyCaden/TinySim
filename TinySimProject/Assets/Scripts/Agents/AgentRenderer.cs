using UnityEngine;
using System.Collections.Generic;

public class AgentRenderer : MonoBehaviour
{
    public static AgentRenderer instance;
    private Mesh circleMesh;
    public Material agentMaterial;
    private List<Matrix4x4> renderMatrices = new List<Matrix4x4>();
    private List<Vector4> renderColours = new List<Vector4>();

    private List<Agent> agentsCopy; // Reference to agents in AgentManager

    private Color eyeColor = Color.black; // Black color for the eyes

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Generate the 2D circle mesh for the agent body
        circleMesh = CircleMeshGenerator.GenerateCircleMesh(16);
    }

    public void Update()
    {
        // Get the list of agents from the AgentManager
        agentsCopy = AgentManager.instance.agents;
        renderMatrices.Clear();
        renderColours.Clear();

        // Loop through all the agents and create the transformation matrices and colors
        foreach (Agent agent in agentsCopy)
        {
            if (agent != null)
            {
                // Create transformation matrix for the body
                Matrix4x4 bodyMatrix = Matrix4x4.TRS(
                    new Vector3(agent.position.x, agent.position.y, 0),
                    Quaternion.Euler(0, 0, agent.rotation), // Apply rotation based on agent's rotation
                    Vector3.one * agent.size // Scale by size (diameter)
                );

                renderMatrices.Add(bodyMatrix);
                renderColours.Add(agent.colour); // Store the color of the agent

                for (int i = 0; i < agent.eyeNumber; i++)
                {
                    float angleOffset = (i % 2 == 0) ? 30f : -30f;
                    float angle = agent.rotation + angleOffset;

                    angle = Mathf.Repeat(angle, 360);
                    if (angle < 180 && angle > 0) continue;

                    float distanceFromCenter = agent.size;
                    float eyeX = distanceFromCenter * Mathf.Cos(Mathf.Deg2Rad * angle);
                    float eyeY = distanceFromCenter * Mathf.Sin(Mathf.Deg2Rad * angle) / (agent.size * 2);

                    Vector3 eyePosition = agent.position + new Vector3(eyeX, eyeY, 0);

                    // Create the transformation matrix for the eye
                    Matrix4x4 eyeMatrix = Matrix4x4.TRS(
                        eyePosition,
                        Quaternion.identity,
                        Vector3.one * 0.25f
                    );

                    renderMatrices.Add(eyeMatrix);
                    renderColours.Add(eyeColor); // Store the eye color

                }
            }
        }
        RenderInstancedMeshes(renderMatrices, renderColours); // Render bodies
    }

    private void RenderInstancedMeshes(List<Matrix4x4> matrices, List<Vector4> colours)
    {
        if (matrices.Count == 0) return;

        // Set up the material property block to pass per-instance properties
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetVectorArray("_Color", colours);

        // Render the meshes in batches of 1023 instances to avoid GPU limits
        for (int i = 0; i < matrices.Count; i += 1023)
        {
            int count = Mathf.Min(1023, matrices.Count - i);
            Graphics.DrawMeshInstanced(circleMesh, 0, agentMaterial, matrices.GetRange(i, count), propertyBlock);
        }
    }
}


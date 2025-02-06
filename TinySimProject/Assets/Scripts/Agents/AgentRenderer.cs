using UnityEngine;
using System.Collections.Generic;

public class AgentRenderer2D : MonoBehaviour
{
    private Mesh circleMesh;
    public Material agentMaterial;

    private List<Matrix4x4> matrices = new List<Matrix4x4>();
    private List<Vector4> colours = new List<Vector4>();

    private List<Agent> agentsCopy; // Reference to agents in agentsManager

    private void Start()
    {
        // Generate the 2D circle mesh
        circleMesh = CircleMeshGenerator.GenerateCircleMesh(16);
    }

    private void Update()
    {
        // Get the list of agents from the AgentManager
        agentsCopy = AgentManager.instance.agents;
        matrices.Clear();
        colours.Clear();

        // Loop through all the agents and create the transformation matrix and color
        foreach (Agent agent in agentsCopy)
        {
            // Create transformation matrix (position, rotation, and size)
            Matrix4x4 matrix = Matrix4x4.TRS(
                new Vector3(agent.position.x, agent.position.y, 0),
                Quaternion.identity,
                Vector3.one * agent.size * 2f // Scale by size (diameter)
            );

            matrices.Add(matrix);
            colours.Add(agent.colour); // Store the color of the agent
        }

        // Set up the material property block to pass per-instance properties
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        // Set the colors for each instance (agents)
        propertyBlock.SetVectorArray("_Color", colours);

        // Render the agents in batches of 1023 instances to avoid GPU limits
        for (int i = 0; i < matrices.Count; i += 1023)
        {
            int count = Mathf.Min(1023, matrices.Count - i);
            Graphics.DrawMeshInstanced(circleMesh, 0, agentMaterial, matrices.GetRange(i, count), propertyBlock);
        }
    }
}

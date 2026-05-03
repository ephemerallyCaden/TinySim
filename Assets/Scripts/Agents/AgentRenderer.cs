using UnityEngine;

public class AgentRenderer : InstancedRenderer
{
    public static AgentRenderer instance;
    private Color eyeColor = Color.black;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    protected override Mesh CreateMesh()
    {
        return CircleMeshGenerator.GenerateCircleMesh(16);
    }

    protected override void PopulateRenderData()
    {
        var agents = AgentManager.instance.agents;

        foreach (Agent agent in agents)
        {
            if (agent == null) continue;
            if (float.IsNaN(agent.position.x) || float.IsNaN(agent.position.y)) continue;

            // Body
            Matrix4x4 bodyMatrix = Matrix4x4.TRS(
                new Vector3(agent.position.x, agent.position.y, 0),
                Quaternion.Euler(0, 0, agent.rotation),
                Vector3.one * agent.size
            );
            AddInstance(bodyMatrix, agent.colour);

            // Eyes
            for (int i = 0; i < 2; i++)
            {
                float angleOffset = (i == 0) ? 30f : -30f;
                float angle = agent.rotation + angleOffset;
                angle = Mathf.Repeat(angle, 360);
                if (angle < 180 && angle > 0) continue;

                float distanceFromCenter = agent.size;
                float eyeX = distanceFromCenter * Mathf.Cos(Mathf.Deg2Rad * angle);
                float eyeY = distanceFromCenter * Mathf.Sin(Mathf.Deg2Rad * angle) / (agent.size * 2);

                Vector3 eyePosition = agent.position + new Vector3(eyeX, eyeY, 0);

                AddInstance(Matrix4x4.TRS(eyePosition, Quaternion.identity, Vector3.one * 0.25f), eyeColor);
            }
        }
    }
}

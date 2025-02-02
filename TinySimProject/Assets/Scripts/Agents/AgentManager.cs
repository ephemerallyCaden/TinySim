using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    new List<Agent> agents = new List<Agent>();
    // Start is called before the first frame update
    void Start()
    {

    }

    public void UpdateAgents(float deltaTime)
    {
        foreach (Agent agent in agents)
        {
            agent.UpdateAgent(deltaTime);
        }
    }
}

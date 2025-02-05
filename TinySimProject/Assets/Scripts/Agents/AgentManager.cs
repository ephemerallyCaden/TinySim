using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager instance;
    public List<Agent> agents = new List<Agent>();

    // Queues for deferred modifications
    private readonly List<Agent> agentsToAdd = new List<Agent>();
    private readonly List<Agent> agentsToRemove = new List<Agent>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void UpdateAgents(float deltaTime)
    {
        // Update all agents
        foreach (Agent agent in agents)
        {
            agent.UpdateAgent(deltaTime);
        }

        // Apply additions and removals AFTER iteration
        if (agentsToAdd.Count > 0)
        {
            agents.AddRange(agentsToAdd);
            agentsToAdd.Clear();
        }

        if (agentsToRemove.Count > 0)
        {
            foreach (Agent agent in agentsToRemove)
            {
                agents.Remove(agent);
                Debug.Log("Removing an Agent");
            }
            agentsToRemove.Clear();
        }
    }

    public void CreateAgent(Agent agent)
    {
        agentsToAdd.Add(agent); // Queue for safe addition
    }

    public void RemoveAgent(Agent agent)
    {
        agentsToRemove.Add(agent); // Queue for safe removal
    }
}
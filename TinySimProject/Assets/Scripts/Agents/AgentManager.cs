using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager instance;
    public GameObject agentPrefab;
    public List<Agent> agents = new List<Agent>();
    public int currentAgentID = 0;

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

    public void AgentListAdd(Agent agent)
    {
        agentsToAdd.Add(agent); // Queue for safe addition
    }

    public void CreateAgent(
        Vector3 position,
        float size,
        float speed,
        Vector4 colour,
        float visionDistance,
        float visionAngle,
        float health,
        float maxEnergy,
        float maxReproductionCooldown,
        float reproductionEnergyCost,
        Genome genome,
        NeuralNetwork network)
    {
        GameObject agentObject = Instantiate(agentPrefab, position, Quaternion.identity);
        Agent agent = agentObject.GetComponent<Agent>();

        // Assign attributes
        agent.id = currentAgentID++;
        agent.size = size;
        agent.speed = speed;
        agent.colour = colour;
        agent.visionDistance = visionDistance;
        agent.visionAngle = visionAngle;
        agent.health = health;
        agent.maxEnergy = maxEnergy;
        agent.energy = maxEnergy; // Start with full energy

        //Reproductive attributes
        agent.maxReproductionCooldown = maxReproductionCooldown;
        agent.reproductionEnergyCost = reproductionEnergyCost;

        // Assign genome and neural network
        agent.genome = genome;
        agent.network = network;

        // Register the agent with the list in agent manager (here!)
        instance.AgentListAdd(agent);

        Debug.Log($"Agent {agent.id} Created at " + position); //Testing testing 1 2 3
    }
    public void AgentListRemove(Agent agent)
    {
        agentsToRemove.Add(agent); // Queue for safe removal
    }
}
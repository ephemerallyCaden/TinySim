using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager instance;
    public GameObject agentPrefab;
    public List<Agent> agents = new List<Agent>();
    public int currentAgentID = 0;
    public int maxPopulation = 200;
    public int population;

    // Queues for deferred list changes
    private readonly List<Agent> agentsToAdd = new List<Agent>();
    private readonly List<Agent> agentsToRemove = new List<Agent>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void UpdateAgents(float deltaTime)
    {
        population = agents.Count;
        if (population < maxPopulation)
        {
            // Update all agents
            foreach (Agent agent in agents)
            {
                agent.UpdateAgent(deltaTime);
                agent.UpdateReproduction(deltaTime);
            }
        }
        else
        {
            // Update all agents without updating reproduction
            foreach (Agent agent in agents)
            {
                agent.UpdateAgent(deltaTime);
            }
        }

        // Apply additions and removals afrer iteration to prevent changing the iterator
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
        int generation,
        Vector3 position,
        float size,
        float speed,
        Vector4 colour,
        float visionDistance,
        float visionAngle,
        float mutationChanceMod,
        float mutationMagnitudeMod,
        float health,
        float energy,
        float maxEnergy,
        float maxReproductionCooldown,
        float reproductionEnergyCost,
        Genome genome,
        NeuralNetwork network)
    {
        GameObject agentObject = Instantiate(agentPrefab, position, Quaternion.identity);
        Agent agent = agentObject.GetComponent<Agent>();

        // Assign attributes
        agent.generation = generation;
        agent.id = currentAgentID++;
        agent.size = size;
        agent.speed = speed;
        agent.colour = colour;
        agent.visionDistance = visionDistance;
        agent.visionAngle = visionAngle;
        agent.mutationChanceMod = mutationChanceMod;
        agent.mutationMagnitudeMod = mutationMagnitudeMod;
        agent.health = health;
        agent.maxEnergy = maxEnergy;
        agent.energy = energy;


        //Reproductive attributes
        agent.maxReproductionCooldown = maxReproductionCooldown;
        agent.reproductionEnergyCost = reproductionEnergyCost;
        agent.reproductionRange = 8.0f;
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
        if (AgentStatsUI.instance.selectedAgent == agent)
        {
            AgentStatsUI.instance.HideAgentStats();
        }
    }
}
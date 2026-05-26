using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager instance;
    public GameObject agentPrefab;
    private DeferredEntityList<Agent> _agents = new DeferredEntityList<Agent>();
    public IReadOnlyList<Agent> agents => _agents.Items;
    private int currentAgentID = 0;
    public int maxPopulation = 500;
    public int population { get; private set; }

    private int generationSum;
    public int avgGeneration { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        SimEvents.OnAgentDied += HandleAgentDied;
    }

    private void OnDisable()
    {
        SimEvents.OnAgentDied -= HandleAgentDied;
    }

    private void HandleAgentDied(Agent agent)
    {
        _agents.QueueRemove(agent);
    }

    public void UpdateAgents(float deltaTime)
    {
        //Calculate current population
        population = _agents.Count;
        bool canReproduce = population < maxPopulation;

        generationSum = 0;
        for (int i = _agents.Count - 1; i >= 0; i--)
        {
            if (_agents[i] == null)
            {
                _agents.RemoveNullAt(i);
                continue;
            }

            _agents[i].UpdateAgent(deltaTime);
            generationSum += _agents[i].generation;
            _agents[i].UpdateReproduction(deltaTime, canReproduce);
        }

        //Calculate the average generation number of all agents
        if (population > 0) avgGeneration = generationSum / population;
        else avgGeneration = -1;

        _agents.ApplyChanges();
    }

    public void AgentListAdd(Agent agent)
    {
        _agents.QueueAdd(agent);
    }

    public void AgentListRemove(Agent agent)
    {
        _agents.QueueRemove(agent);
    }

    public void CreateAgent(
        int generation,
        Vector3 position,
        AgentAttributes attrs,
        float energy,
        float maxEnergy,
        Genome genome,
        NeuralNetwork network,
        int parentSpeciesId = -1)
    {
        GameObject agentObject = Instantiate(agentPrefab, position, Quaternion.identity);
        Agent agent = agentObject.GetComponent<Agent>();

        // Assign attributes
        agent.generation = generation;
        agent.id = currentAgentID++;
        agent.size = attrs.size;
        agent.speed = attrs.speed;
        agent.colour = attrs.colour;
        agent.visionDistance = attrs.visionDistance;
        agent.visionAngle = attrs.visionAngle;
        agent.mutationChanceMod = attrs.mutationChanceMod;
        agent.mutationMagnitudeMod = attrs.mutationMagnitudeMod;
        agent.maxHealth = attrs.size * SimulationManager.instance.config.healthPerSize;
        agent.health = agent.maxHealth;
        agent.maxEnergy = maxEnergy;
        agent.energy = energy;

        // Reproductive attributes
        agent.maxReproductionCooldown = attrs.maxReproductionCooldown;
        agent.reproductionEnergyCost = attrs.reproductionEnergyCost;

        // Predation
        agent.attackDamage = attrs.attackDamage;

        // Diet
        agent.dietPreference = attrs.dietPreference;

        // Assign genome and neural network
        agent.genome = genome;
        agent.network = network;
        agent.rotation = SimRandom.Range(0f, 360f);

        // Register the agent
        instance.AgentListAdd(agent);

        // Assign to a species
        if (SpeciationManager.instance != null)
        {
            SpeciationManager.instance.AssignSpecies(agent, parentSpeciesId);
        }

        SimEvents.AgentBorn(agent);
    }

}
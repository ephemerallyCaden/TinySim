using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentInitialiser : MonoBehaviour
{
    [NonSerialized] public int initialAgentCount;

    //Base Attribute Variables
    [NonSerialized] public float baseSize = 1.0f;
    [NonSerialized] public float baseSpeed = 2.0f;
    [NonSerialized] public Color baseColour = Color.grey;
    [NonSerialized] public float baseVisionDistance = 10f;
    [NonSerialized] public float baseVisionAngle = 90f;
    [NonSerialized] public float baseMutationChanceMod = 1f;
    [NonSerialized] public float baseMutationMagnitudeMod = 1f;
    [NonSerialized] public float baseMaxEnergy = 200f;
    [NonSerialized] public float baseHealth = 100f;
    [NonSerialized] public float baseMaxReproductionCooldown = 10f;
    [NonSerialized] public float baseReproductionEnergyCost = 20f;
    [NonSerialized] public float baseReproductionRange = 20f;

    //Base Genome Variables
    [NonSerialized] public int baseInputNum = 17;
    [NonSerialized] public int baseOutputNum = 3;

    //Agent Spawning Variables
    [NonSerialized] public float spawnRadius = 10f;
    [NonSerialized] public Vector3 spawnCenter = Vector3.zero;

    public enum SpawnPattern { Central, Clusters, Random }
    [NonSerialized] public SpawnPattern spawnPattern = SpawnPattern.Central;
    [NonSerialized] public bool uniformStart = false; // All agents identical — no randomness
    [NonSerialized] public int numberOfClusters = 5;

    public void InitialiseAgents()
    {
        // Initialise innovation tracker with correct node ID offset
        InnovationTracker.Initialise(baseInputNum, baseOutputNum);

        //Switch using an enumerator based on spawn pattern
        switch (spawnPattern)
        {
            case SpawnPattern.Central:
                SpawnAgents(() => GetCircularPosition(spawnCenter, spawnRadius));
                break;
            case SpawnPattern.Clusters:
                SpawnInClusters();
                break;
            case SpawnPattern.Random:
                SpawnAgents(() => GetCircularPosition(Vector3.zero, spawnRadius * 3));
                break;
        }
    }

    //Get a position around a central point
    private Vector3 GetCircularPosition(Vector3 center, float radius)
    {
        float angle = SimRandom.Range(0f, Mathf.PI * 2);
        float distance = Mathf.Sqrt(SimRandom.NextFloat()) * radius;
        return center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0);
    }

    private void SpawnInClusters()
    {
        List<Vector3> clusterCenters = new List<Vector3>();
        for (int i = 0; i < numberOfClusters; i++)
            clusterCenters.Add(GetCircularPosition(spawnCenter, spawnRadius * 2));

        SpawnAgents(() =>
        {
            Vector3 center = clusterCenters[SimRandom.NextInt(numberOfClusters)];
            return GetCircularPosition(center, spawnRadius / 2);
        });
    }

    private Genome sharedGenome; // Used for uniform start — all agents share the same genome

    private void SpawnAgents(Func<Vector3> getPosition)
    {
        // Generate one shared genome for uniform start
        if (uniformStart)
            sharedGenome = GenerateBaseGenome();

        for (int i = 0; i < initialAgentCount; i++)
            CreateBaseAgent(getPosition());

    }

    private void CreateBaseAgent(Vector3 position)
    {
        // Generate or clone genome
        Genome baseGenome = uniformStart ? CloneGenome(sharedGenome) : GenerateBaseGenome();
        NeuralNetwork baseNetwork = new NeuralNetwork(baseGenome);

        AgentAttributes attrs;

        if (uniformStart)
        {
            attrs.colour = baseColour;
            attrs.size = baseSize;
            attrs.speed = baseSpeed;
            attrs.visionDistance = baseVisionDistance;
            attrs.visionAngle = baseVisionAngle;
            attrs.mutationChanceMod = baseMutationChanceMod;
            attrs.mutationMagnitudeMod = baseMutationMagnitudeMod;
            attrs.maxReproductionCooldown = baseMaxReproductionCooldown;
            attrs.reproductionEnergyCost = baseReproductionEnergyCost;
            attrs.reproductionRange = baseReproductionRange;
        }
        else
        {
            SimulationConfig cfg = SimulationManager.instance.config;
            attrs.colour = new Color(SimRandom.NextFloat(), SimRandom.NextFloat(), SimRandom.NextFloat(), 1f);
            attrs.size = SimRandom.Range(cfg.initialSizeMin, cfg.initialSizeMax);
            attrs.speed = SimRandom.Range(cfg.initialSpeedMin, cfg.initialSpeedMax);
            attrs.visionDistance = SimRandom.Range(cfg.initialVisionDistanceMin, cfg.initialVisionDistanceMax);
            attrs.visionAngle = SimRandom.Range(cfg.initialVisionAngleMin, cfg.initialVisionAngleMax);
            attrs.mutationChanceMod = SimRandom.Range(cfg.initialMutationChanceModMin, cfg.initialMutationChanceModMax);
            attrs.mutationMagnitudeMod = SimRandom.Range(cfg.initialMutationMagnitudeModMin, cfg.initialMutationMagnitudeModMax);
            attrs.maxReproductionCooldown = SimRandom.Range(cfg.initialMaxRepCooldownMin, cfg.initialMaxRepCooldownMax);
            attrs.reproductionEnergyCost = SimRandom.Range(cfg.initialRepEnergyCostMin, cfg.initialRepEnergyCostMax);
            attrs.reproductionRange = SimRandom.Range(cfg.initialRepRangeMin, cfg.initialRepRangeMax);
        }

        AgentManager.instance.CreateAgent(
            0,
            position,
            attrs,
            baseHealth,
            baseMaxEnergy,
            baseMaxEnergy,
            baseGenome,
            baseNetwork
        );
    }

    private Genome GenerateBaseGenome()
    {
        // Creates a base genome with initial connections that give agents
        // basic movement and food-seeking ability from birth
        List<NodeGene> nodeGenes = new List<NodeGene>();
        List<ConnectionGene> connectionGenes = new List<ConnectionGene>();

        //Add default input and output nodes to nodeGenes
        for (int i = 0; i < baseInputNum; i++)
        {
            nodeGenes.Add(new NodeGene(i, NodeType.Input, 0.0, ActivationFunctions.Sigmoid));
        }

        for (int o = 0; o < baseOutputNum; o++)
        {
            nodeGenes.Add(new NodeGene(baseInputNum + o, NodeType.Output, 0.0, ActivationFunctions.Tanh));
        }

        // Output node indices
        int moveOutput = baseInputNum;      // Output 0: movement speed
        int turnOutput = baseInputNum + 1;  // Output 1: turning rate

        // Input indices of interest:
        // 0 = bias (always 1), 3 = hungriness, 10 = food proximity, 11 = food angle (signed)

        // Give a baseline forward drive: bias -> movement (so agents move by default)
        AddConnection(connectionGenes, 0, moveOutput, uniformStart ? 0.5f : SimRandom.Range(0.3f, 0.8f));

        // Food angle -> turning rate (signed input handles direction, weight just sets responsiveness)
        AddConnection(connectionGenes, 11, turnOutput, uniformStart ? 1.0f : SimRandom.Range(0.5f, 1.5f));

        // Food proximity -> movement speed (speed up when food is close)
        AddConnection(connectionGenes, 10, moveOutput, uniformStart ? 0.4f : SimRandom.Range(0.2f, 0.6f));

        // Add random connections for diversity (skip in uniform mode)
        if (!uniformStart)
        {
            for (int r = 0; r < 2; r++)
            {
                int src = SimRandom.NextInt(baseInputNum);
                int tgt = SimRandom.Range(baseInputNum, baseInputNum + baseOutputNum);
                if (!connectionGenes.Exists(c => c.linkid.source == src && c.linkid.target == tgt))
                {
                    AddConnection(connectionGenes, src, tgt, SimRandom.Range(-1f, 1f));
                }
            }
        }

        //Generate the genome
        Genome genome = new Genome(nodeGenes, connectionGenes);
        return genome;
    }

    private void AddConnection(List<ConnectionGene> connections, int source, int target, float weight)
    {
        int innovationId = InnovationTracker.GetInnovation(source, target);
        LinkID link = new LinkID(innovationId, source, target);
        connections.Add(new ConnectionGene(link, weight, true));
    }

    private Genome CloneGenome(Genome original)
    {
        return Genome.Clone(original);
    }
}


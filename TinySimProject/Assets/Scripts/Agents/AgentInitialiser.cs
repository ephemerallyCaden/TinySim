using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [NonSerialized] public float baseMaxReproductionCooldown = 20f;
    [NonSerialized] public float baseReproductionEnergyCost = 30f;

    //Base Genome Variables
    [NonSerialized] public int baseInputNum = 13;
    [NonSerialized] public int baseOutputNum = 3;

    //Agent Spawning Variables
    [NonSerialized] public float spawnRadius = 10f;
    [NonSerialized] public Vector3 spawnCenter = Vector3.zero;

    public enum SpawnPattern { Central, Clusters, Random }
    [NonSerialized] public SpawnPattern spawnPattern = SpawnPattern.Central;
    [NonSerialized] public int numberOfClusters = 5;

    public void InitialiseAgents()
    {
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
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
        float distance = Mathf.Sqrt(UnityEngine.Random.value) * radius;
        return center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0);
    }

    private void SpawnInClusters()
    {
        List<Vector3> clusterCenters = new List<Vector3>();
        for (int i = 0; i < numberOfClusters; i++)
            clusterCenters.Add(GetCircularPosition(spawnCenter, spawnRadius * 2));

        SpawnAgents(() =>
        {
            Vector3 center = clusterCenters[UnityEngine.Random.Range(0, numberOfClusters)];
            return GetCircularPosition(center, spawnRadius / 2);
        });
    }

    private void SpawnAgents(Func<Vector3> getPosition)
    {
        for (int i = 0; i < initialAgentCount; i++)
            CreateBaseAgent(getPosition());

    }

    private void CreateBaseAgent(Vector3 position)
    {
        // Generate a base genome and neural network
        Genome baseGenome = GenerateBaseGenome();
        NeuralNetwork baseNetwork = new NeuralNetwork(baseGenome);

        Color randomColour = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1f);

        // Call CreateAgent with base parameters
        AgentManager.instance.CreateAgent(
            0, //GENERATION NUMBER
            position,
            baseSize + UnityEngine.Random.Range(-0.5f, 0.5f), //Add a slight mutation on base size and speed for randomness
            baseSpeed + UnityEngine.Random.Range(-0.5f, 0.5f),
            randomColour,
            baseVisionDistance,
            baseVisionAngle,
            baseMutationChanceMod,
            baseMutationMagnitudeMod,
            baseHealth,
            baseMaxEnergy, //Starts at full energy initially to boost survival chance
            baseMaxEnergy,
            baseMaxReproductionCooldown,
            baseReproductionEnergyCost,
            baseGenome,
            baseNetwork
        );
    }

    private Genome GenerateBaseGenome()
    {
        // Creates a base genome with everything empty aside from 
        List<NodeGene> nodeGenes = new List<NodeGene>();
        List<ConnectionGene> connectionGenes = new List<ConnectionGene>();

        //Add default input and outputs nodes to nodeGenes
        for (int i = 0; i < baseInputNum; i++)
        {
            nodeGenes.Add(new NodeGene(i, NodeType.Input, 0.0, ActivationFunctions.Sigmoid));
        }

        for (int o = 0; o < baseOutputNum; o++)
        {
            nodeGenes.Add(new NodeGene(baseInputNum + o, NodeType.Output, 0.0, ActivationFunctions.Tanh));
        }

        //Create a random initial connection in the base genome
        int randomSource = UnityEngine.Random.Range(0, baseInputNum);
        int randomTarget = UnityEngine.Random.Range(baseInputNum, baseInputNum + baseOutputNum - 1);

        LinkID temp = new LinkID(InnovationTracker.GetInnovation(randomSource, randomTarget), randomSource, randomTarget);
        connectionGenes.Add(new ConnectionGene(temp, 1.0, true));

        //Generate the genome
        Genome genome = new Genome(nodeGenes, connectionGenes);
        return genome;
    }
}


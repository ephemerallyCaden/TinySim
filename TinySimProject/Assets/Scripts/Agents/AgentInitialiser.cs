using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AgentInitialiser : MonoBehaviour
{
    public int initialAgentCount = 50;

    [Header("Base Attribute Variables")]
    public float baseSize = 1.0f;
    public float baseSpeed = 2.0f;
    public Color baseColour = Color.grey;
    public float baseVisionDistance = 10f;
    public float baseVisionAngle = 90f;
    public float baseMutationChanceMod = 1f;
    public float baseMutationMagnitudeMod = 1.0f;
    public float baseMaxEnergy = 5000f;
    public float baseHealth = 100f;
    public float baseMaxReproductionCooldown = 10f;
    public float baseReproductionEnergyCost = 30f;

    [Header("Genome Variables")]
    public int baseInputNum = 13;
    public int baseOutputNum = 3;


    public float spawnRadius = 10f;
    public Vector3 spawnCenter = Vector3.zero;

    public enum SpawnPattern { Central, Clusters, Random }
    public SpawnPattern spawnPattern = SpawnPattern.Central;
    public int numberOfClusters = 5;

    public void InitialiseAgents()
    {
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

    private Vector3 GetCircularPosition(Vector3 center, float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float distance = Mathf.Sqrt(Random.value) * radius;
        return center + new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0);
    }

    private void SpawnInClusters()
    {
        List<Vector3> clusterCenters = new List<Vector3>();
        for (int i = 0; i < numberOfClusters; i++)
            clusterCenters.Add(GetCircularPosition(spawnCenter, spawnRadius * 2));

        SpawnAgents(() =>
        {
            Vector3 center = clusterCenters[Random.Range(0, numberOfClusters)];
            return GetCircularPosition(center, spawnRadius / 2);
        });
    }

    private void SpawnAgents(System.Func<Vector3> getPosition)
    {
        for (int i = 0; i < initialAgentCount; i++)
            CreateBaseAgent(getPosition());

    }

    private void CreateBaseAgent(Vector3 position)
    {
        // Generate a base genome and neural network
        Genome baseGenome = GenerateBaseGenome();
        NeuralNetwork baseNetwork = new NeuralNetwork(baseGenome);

        // Call CreateAgent with base parameters
        AgentManager.instance.CreateAgent(
            position,
            baseSize,
            baseSpeed,
            baseColour,
            baseVisionDistance,
            baseVisionAngle,
            baseHealth,
            baseMaxEnergy,
            baseMaxReproductionCooldown,
            baseReproductionEnergyCost,
            baseGenome,
            baseNetwork
        );
        Debug.Log("Agent Created");
    }






    // Create a Neural Network from the Genome

    private Genome GenerateBaseGenome()
    {

        List<NodeGene> nodeGenes = new List<NodeGene>();

        for (int i = 0; i < baseInputNum; i++)
        {
            nodeGenes.Add(new NodeGene(i, NodeType.Input, 0.0, ActivationFunctions.Sigmoid));
        }

        for (int o = 0; o < baseOutputNum; o++)
        {
            nodeGenes.Add(new NodeGene(baseInputNum + o - 1, NodeType.Output, 0.0, ActivationFunctions.ReLU));
        }
        List<ConnectionGene> connectionGenes = new List<ConnectionGene>();

        Genome genome = new Genome(nodeGenes, connectionGenes);
        return genome;
    }
}


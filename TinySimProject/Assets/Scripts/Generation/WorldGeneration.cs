using UnityEngine;
using UnityEngine.Tilemaps;
public class WorldGeneration : MonoBehaviour
{


    [Header("Agent Initialisation")]
    public AgentInitialiser agentInitialiser;
    public int initialAgentPopulation = 1;
    public AgentInitialiser.SpawnPattern spawnPattern = AgentInitialiser.SpawnPattern.Central;

    [Header("World Settings")]
    public float worldRadius = 50f;
    public Vector3 worldCenter = Vector3.zero;

    [Header("Food Settings")]
    public int initialFoodCount = 100;

    private void Start()
    {
        // Initialise the world

        InitialiseWorld();
    }

    private void InitialiseWorld()
    {
        // Generate Terrain
        //terrainGenerator.GenerateTerrain((int)worldRadius * 2, (int)worldRadius * 2, terrainScale, terrainOffset);

        // Generate Temperature Map
        //temperatureMap.GenerateTemperatureMap((int)worldRadius * 2, (int)worldRadius * 2, temperatureScale);

        // Configure the agent initializer
        agentInitialiser.initialAgentCount = initialAgentPopulation;
        agentInitialiser.spawnPattern = spawnPattern;
        agentInitialiser.spawnRadius = worldRadius;
        agentInitialiser.spawnCenter = worldCenter;

        // Initialise agents
        agentInitialiser.InitialiseAgents();


    }
}


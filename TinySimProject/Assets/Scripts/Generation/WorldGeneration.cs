using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGeneration : MonoBehaviour
{
    [Header("Agent Initialisation")]
    public AgentInitialiser agentInitialiser;
    public int initialAgentPopulation = 50;
    public AgentInitialiser.SpawnPattern spawnPattern = AgentInitialiser.SpawnPattern.Central;

    [Header("World Settings")]
    public int worldSize;
    public Vector3 worldCenter;

    [Header("Temperature Map Settings")]
    public TemperatureMap temperatureMap; // Reference to the TemperatureMap script
    public float temperatureScale = 4f; // Scale for temperature map generation

    [Header("Terrain Settings")]
    public TerrainGenerator terrainGenerator; // Reference to the TerrainGenerator script
    public float terrainScale = 20f; // Scale for terrain generation
    public Vector2 terrainOffset = Vector2.zero; // Offset for terrain generation

    private void Start()
    {
        // Initialise the world
        if (worldSize == 0) worldSize = 64;
        worldCenter = new Vector3(worldSize / 2, worldSize / 2, 0);
        InitialiseWorld();
    }

    private void InitialiseWorld()
    {

        SimulationManager.instance.worldSize = worldSize;
        // Generate Terrain
        terrainGenerator.GenerateTerrain(worldSize, worldSize, terrainScale, terrainOffset);

        // Generate Temperature Map
        temperatureMap.GenerateTemperatureMap(worldSize, worldSize, temperatureScale);

        // Configure the agent initializer
        agentInitialiser.initialAgentCount = initialAgentPopulation;
        agentInitialiser.spawnPattern = spawnPattern;
        agentInitialiser.spawnRadius = worldSize / 2;
        agentInitialiser.spawnCenter = worldCenter;

        // Initialise agents
        agentInitialiser.InitialiseAgents();
        
        //Spawn the first food
        FoodSpawner.instance.SpawnInitialFood();


    }
}


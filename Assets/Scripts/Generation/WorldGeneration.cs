using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGeneration : MonoBehaviour
{
    [Header("Agent Initialisation")]
    public AgentInitialiser agentInitialiser;
    public int initialAgentPopulation = 80;
    public AgentInitialiser.SpawnPattern spawnPattern = AgentInitialiser.SpawnPattern.Clusters;
    public bool uniformStart = false; // All agents start identical — evolution from a single ancestor

    [Header("World Settings")]
    public int worldSize;
    public Vector3 worldCenter;

    [Header("Temperature Map Settings")]
    public TemperatureMap temperatureMap; // Reference to the TemperatureMap script
    public float temperatureScale = 3f; // Scale for temperature map generation (lower = smoother)

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
        SimulationConfig cfg = SimulationManager.instance.config;

        // World size comes from SimulationConfig
        worldSize = cfg.worldSize;

        // Wire config values to runtime components
        AgentManager.instance.maxPopulation = cfg.maxPopulation;
        FoodSpawner.instance.maxFoodCount = cfg.maxFoodCount;
        FoodSpawner.instance.initialFoodCount = cfg.initialFoodCount;
        FoodSpawner.instance.maxSpawnTime = cfg.foodSpawnInterval;

        // Generate Temperature Map (also creates the visual background)
        temperatureMap.GenerateTemperatureMap(worldSize, worldSize, temperatureScale);

        // Configure the agent initializer
        agentInitialiser.initialAgentCount = cfg.initialAgentCount;
        agentInitialiser.spawnPattern = spawnPattern;
        agentInitialiser.uniformStart = uniformStart;
        agentInitialiser.spawnRadius = worldSize / 2;
        agentInitialiser.spawnCenter = worldCenter;

        // Initialise agents
        agentInitialiser.InitialiseAgents();

        //Spawn the first food
        FoodSpawner.instance.SpawnInitialFood();
    }
}


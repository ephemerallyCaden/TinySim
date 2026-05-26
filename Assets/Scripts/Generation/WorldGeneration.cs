using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGeneration : MonoBehaviour
{
    [Header("References")]
    public AgentInitialiser agentInitialiser;
    public TemperatureMap temperatureMap;
    public TerrainGenerator terrainGenerator;

    [Header("Terrain Settings")]
    public float terrainScale = 20f;
    public Vector2 terrainOffset = Vector2.zero;

    private int worldSize;
    private Vector3 worldCenter;

    private void Start()
    {
        InitialiseWorld();
    }

    private void InitialiseWorld()
    {
        SimulationConfig cfg = SimulationManager.instance.config;

        // World size from config
        worldSize = cfg.worldSize;
        worldCenter = new Vector3(worldSize / 2f, worldSize / 2f, 0);

        // Wire config values to runtime components
        AgentManager.instance.maxPopulation = cfg.maxPopulation;
        FoodSpawner.instance.maxFoodCount = cfg.maxFoodCount;
        FoodSpawner.instance.initialFoodCount = cfg.initialFoodCount;
        FoodSpawner.instance.maxSpawnTime = cfg.foodSpawnInterval;

        // Generate Temperature Map (reads scale + skew from config)
        temperatureMap.GenerateTemperatureMap(worldSize, worldSize, cfg.temperatureScale, cfg.coldSkewPower);

        // Configure the agent initializer (reads spawn settings from config)
        agentInitialiser.initialAgentCount = cfg.initialAgentCount;
        agentInitialiser.spawnPattern = (AgentInitialiser.SpawnPattern)cfg.spawnPattern;
        agentInitialiser.uniformStart = cfg.uniformStart;
        agentInitialiser.spawnRadius = worldSize / 2f;
        agentInitialiser.spawnCenter = worldCenter;
        agentInitialiser.numberOfClusters = cfg.numberOfClusters;

        // Initialise agents
        agentInitialiser.InitialiseAgents();

        // Spawn the first food
        FoodSpawner.instance.SpawnInitialFood();
    }
}


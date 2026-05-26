using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public static FoodSpawner instance;

    [Header("Food Settings")]
    public GameObject foodPrefab;
    public GameObject poisonFoodPrefab;
    public GameObject meatFoodPrefab;
    public int initialFoodCount = 150;
    public int maxFoodCount = 250;
    public float foodSpawnTime;
    public float maxSpawnTime = 1f;
    private DeferredEntityList<Food> _foodList = new DeferredEntityList<Food>();
    public IReadOnlyList<Food> foodList => _foodList.Items;
    private int plantFoodCount = 0;
    public TemperatureMap temperatureMap;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SimEvents.OnFoodConsumed += HandleFoodConsumed;
    }

    private void OnDisable()
    {
        SimEvents.OnFoodConsumed -= HandleFoodConsumed;
    }

    private void HandleFoodConsumed(Food food)
    {
        _foodList.QueueRemove(food);
        if (food is not MeatFood && food is not PoisonFood)
            plantFoodCount--;
    }

    public void DropDeathFood(Vector3 position, float agentEnergy)
    {
        SimulationConfig cfg = SimulationManager.instance.config;
        if (cfg.deathFoodDropCount <= 0) return;
        if (agentEnergy <= 0f) return; // Starved agents drop no meat

        float totalNutrition = agentEnergy * cfg.deathFoodNutritionPerEnergy;
        float nutritionPerDrop = totalNutrition / cfg.deathFoodDropCount;

        for (int i = 0; i < cfg.deathFoodDropCount; i++)
        {

            Vector2 offset = new Vector2(
                SimRandom.Range(-cfg.deathFoodScatter, cfg.deathFoodScatter),
                SimRandom.Range(-cfg.deathFoodScatter, cfg.deathFoodScatter));
            Vector2 dropPos = (Vector2)position + offset;
            dropPos.x = Mathf.Clamp(dropPos.x, 0, cfg.worldSize);
            dropPos.y = Mathf.Clamp(dropPos.y, 0, cfg.worldSize);

            GameObject prefab = meatFoodPrefab != null ? meatFoodPrefab : foodPrefab;
            GameObject foodObject = Instantiate(prefab, dropPos, Quaternion.identity);
            Food food = foodObject.GetComponent<Food>();
            food.position = dropPos;
            food.biomeTemperature = 0.5f;
            food.nutritionValue = nutritionPerDrop;
            FoodListAdd(food);
        }
    }

    private void Start()
    {
        foodSpawnTime = maxSpawnTime;
    }

    public void UpdateFoodSpawner(float deltaTime)
    {
        foodSpawnTime -= deltaTime;

        // Spawn food for every spawn interval that elapsed this tick (handles high sim speed)
        SimulationConfig cfg = SimulationManager.instance.config;
        int spawnBatches = 0;
        while (foodSpawnTime <= 0 && spawnBatches < cfg.maxSpawnBatchesPerTick)
        {
            foodSpawnTime += Mathf.Max(maxSpawnTime, 0.1f);
            spawnBatches++;
            for (int i = 0; i < cfg.foodSpawnBatchSize; i++)
            {
                SpawnFood();
            }
        }

        for (int i = _foodList.Count - 1; i >= 0; i--)
        {
            if (_foodList[i] != null)
                _foodList[i].UpdateFood(deltaTime);
            else
                _foodList.RemoveNullAt(i);
        }

        _foodList.ApplyChanges();
    }

    // Spawn initial food
    public void SpawnInitialFood()
    {
        for (int i = 0; i < initialFoodCount; i++)
        {
            SpawnFood();
        }
    }

    public void SpawnFood()
    {
        if (plantFoodCount >= maxFoodCount) return;

        Vector2 randomPosition = GetRandomPositionInWorld();
        if (FoodCheck(randomPosition))
        {
            // Decide whether to spawn poison or normal food based on temperature
            // Poison spawns in cold zones (low temperature), normal food in warm zones
            float temperature = temperatureMap.GetTemperatureAt(
                Mathf.FloorToInt(randomPosition.x),
                Mathf.FloorToInt(randomPosition.y));

            SimulationConfig cfg = SimulationManager.instance.config;
            bool spawnPoison = temperature < cfg.poisonTemperatureThreshold && SimRandom.NextFloat() < cfg.poisonSpawnChance;
            GameObject prefab = (spawnPoison && poisonFoodPrefab != null) ? poisonFoodPrefab : foodPrefab;

            GameObject foodObject = Instantiate(prefab, randomPosition, Quaternion.identity);
            Food food = foodObject.GetComponent<Food>();

            food.position = randomPosition;
            food.biomeTemperature = temperature; // Pass temperature for nutrition scaling
            FoodListAdd(food);
            if (!spawnPoison)
                plantFoodCount++;
        }
    }

    public void FoodListAdd(Food food)
    {
        _foodList.QueueAdd(food);
    }
    public void FoodListRemove(Food food)
    {
        _foodList.QueueRemove(food);
        if (food is not MeatFood && food is not PoisonFood)
            plantFoodCount--;
    }
    private Vector2 GetRandomPositionInWorld()
    {
        float randomX = SimRandom.Range(0f, SimulationManager.instance.config.worldSize);
        float randomY = SimRandom.Range(0f, SimulationManager.instance.config.worldSize);
        return new Vector2(randomX, randomY);
    }

    // Check based on temperature map, the probability of food spawning
    // Cold zones are harsh (10%), warm zones are fertile (90%)
    private bool FoodCheck(Vector2 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);

        float temperature = temperatureMap.GetTemperatureAt(x, y);
        SimulationConfig cfg = SimulationManager.instance.config;
        float spawnChance = Mathf.Lerp(cfg.foodSpawnChanceCold, cfg.foodSpawnChanceWarm, temperature);
        return SimRandom.NextFloat() < spawnChance;
    }
}
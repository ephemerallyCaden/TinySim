using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FoodSpawner : MonoBehaviour
{
    public static FoodSpawner instance;

    [Header("Food Settings")]
    public GameObject foodPrefab;
    public int initialFoodCount = 100;
    public float foodSpawnTime;
    public float maxSpawnTime = 1f;
    public List<Food> foodList;
    public TemperatureMap temperatureMap;

    // Queues for deferred modifications
    private readonly List<Food> foodToAdd = new List<Food>();
    private readonly List<Food> foodToRemove = new List<Food>();



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
    private void Start()
    {
        foodSpawnTime = maxSpawnTime;
    }

    public void UpdateFoodSpawner(float deltaTime)
    {
        foodSpawnTime -= deltaTime;

        // Spawn food if timer hits 0
        if (foodSpawnTime <= 0)
        {
            foodSpawnTime = maxSpawnTime;
            SpawnFood();
        }

        foreach (Food food in foodList)
        {
            // Null check for deleted food, and updating each food object
            if (food != null) food.UpdateFood(deltaTime);
        }

        // Execute deferred modifications
        if (foodToAdd.Count > 0)
        {
            foodList.AddRange(foodToAdd);
            foodToAdd.Clear();
        }

        if (foodToRemove.Count > 0)
        {
            foreach (Food food in foodToRemove)
            {
                foodList.Remove(food);
            }
            foodToRemove.Clear();
        }
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
        Vector2 randomPosition = GetRandomPositionInWorld();
        if (FoodCheck(randomPosition))
        {
            GameObject foodObject = Instantiate(foodPrefab, randomPosition, Quaternion.identity);
            Food food = foodObject.GetComponent<Food>();

            food.position = randomPosition;
            // Notify Manager object
            FoodListAdd(food);
        }
    }

    public void FoodListAdd(Food food)
    {
        foodToAdd.Add(food);
    }
    public void FoodListRemove(Food food)
    {
        foodToRemove.Add(food);
    }
    private Vector2 GetRandomPositionInWorld()
    {
        float randomX = Random.Range(0, SimulationManager.instance.worldSize);
        float randomY = Random.Range(0, SimulationManager.instance.worldSize);
        return new Vector2(randomX, randomY);
    }

    // Check based on temperature map, the probability of food spawning
    private bool FoodCheck(Vector2 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);

        float temperature = temperatureMap.GetTemperatureAt(x, y);
        return Random.value < temperature;
    }
}
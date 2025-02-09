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
    public float maxSpawnTime = 2f;
    public List<Food> foodList;

    // Queues for deferred modifications
    private readonly List<Food> foodToAdd = new List<Food>();
    private readonly List<Food> foodToRemove = new List<Food>();

    [Header("World Settings")]
    public TemperatureMap temperatureMap;
    public int worldSize;

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

        if (foodSpawnTime <= 0)
        {
            foodSpawnTime = maxSpawnTime;
            SpawnFood();
        }

        foreach (Food food in foodList)
        {
            if (food != null) food.UpdateFood(deltaTime);
        }

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

            FoodListAdd(food);
            //Debug.Log($"Spawned food at: {randomPosition}");
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

    private bool FoodCheck(Vector2 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);

        float spawnChance = temperatureMap.GetTemperatureAt(x, y);
        return Random.value < spawnChance;
    }
}
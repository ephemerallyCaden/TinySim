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
    public float maxSpawnTime = 5f;
    public List<Food> foodList;

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

    private void FixedUpdate()
    {
        foodSpawnTime -= Time.deltaTime;

        if (foodSpawnTime <= 0)
        {
            foodSpawnTime = maxSpawnTime;
            SpawnFood();
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
            Debug.Log($"Spawned food at: {randomPosition}");
        }
    }

    public void FoodListAdd(Food food)
    {
        foodList.Add(food);
    }
    public void FoodListRemove(Food food)
    {
        foodList.Remove(food);
    }
    private Vector2 GetRandomPositionInWorld()
    {
        float randomX = Random.Range(0, worldSize);
        float randomY = Random.Range(0, worldSize);
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
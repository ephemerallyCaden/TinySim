using System.Collections;
using UnityEngine;

public class Food : MonoBehaviour
{
    public Vector3 position;
    public float size;
    public Color colour = new Color(0.2f, 0.5f, 0.05f, 1);
    public float despawnTime;
    public float nutritionValue;
    public bool isPoison = false;
    public float biomeTemperature = 0.5f; // Set by FoodSpawner based on spawn location

    public CircleCollider2D col;

    private float timer;

    private void Start()
    {
        //Instantiate variables
        position = transform.position;

        SimulationConfig cfg = SimulationManager.instance.config;
        despawnTime = Random.Range(cfg.foodDespawnMin, cfg.foodDespawnMax);
        timer = despawnTime;

        // Nutrition scales with biome temperature: warm zones = more nutritious food
        // Cold (0.0) -> 5-15 nutrition, Warm (1.0) -> 30-60 nutrition
        float minNutrition = Mathf.Lerp(5f, 30f, biomeTemperature);
        float maxNutrition = Mathf.Lerp(15f, 60f, biomeTemperature);
        nutritionValue = SimRandom.Range(minNutrition, maxNutrition);

        size = nutritionValue * cfg.foodSizePerNutrition;
        col.radius = size;
        AgentComponentCache.RegisterFood(col, this);

        // Food colour reflects nutrition: brighter green = more nutritious
        if (!isPoison)
        {
            float greenIntensity = Mathf.Lerp(0.3f, 0.7f, biomeTemperature);
            colour = new Color(0.1f, greenIntensity, 0.05f, 1);
        }

        // Poison food looks distinct (purple) and damages instead of feeding
        if (isPoison)
        {
            colour = new Color(0.8f, 0.0f, 0.9f, 1);
            size *= cfg.poisonSizeMultiplier; // Slightly larger so visually distinct
            col.radius = size;
        }
    }

    public void UpdateFood(float deltaTime)
    {
        //Spawn food if the timer hits 0
        timer -= deltaTime;

        if (timer <= 0f)
        {
            DespawnFood();
        }
    }

    //Despawn Food
    private void DespawnFood()
    {
        AgentComponentCache.UnregisterFood(col);
        FoodSpawner.instance.FoodListRemove(this);
        Destroy(gameObject);
    }

}
using UnityEngine;

public class Food : MonoBehaviour
{
    public Vector3 position;
    public float size;
    public Color colour = new Color(0.2f, 0.5f, 0.05f, 1);
    public float despawnTime;
    public float nutritionValue;
    public float biomeTemperature = 0.5f; // Set by FoodSpawner based on spawn location

    public CircleCollider2D col;

    protected float timer;

    protected virtual void Start()
    {
        position = transform.position;

        SimulationConfig cfg = SimulationManager.instance.config;
        despawnTime = Random.Range(cfg.foodDespawnMin, cfg.foodDespawnMax);
        timer = despawnTime;

        // If nutritionValue was set before Start (e.g. meat), keep it; otherwise calculate from biome
        if (nutritionValue <= 0f)
        {
            float minNutrition = Mathf.Lerp(cfg.foodNutritionMin * cfg.coldNutritionMultiplier, cfg.foodNutritionMin, biomeTemperature);
            float maxNutrition = Mathf.Lerp(cfg.foodNutritionMin, cfg.foodNutritionMax, biomeTemperature);
            nutritionValue = SimRandom.Range(minNutrition, maxNutrition);
        }

        size = nutritionValue * cfg.foodSizePerNutrition;
        col.radius = size;
        AgentComponentCache.RegisterFood(col, this);

        // Plant food colour reflects nutrition: brighter green = more nutritious
        float greenIntensity = Mathf.Lerp(0.3f, 0.7f, biomeTemperature);
        colour = new Color(0.1f, greenIntensity, 0.05f, 1);
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
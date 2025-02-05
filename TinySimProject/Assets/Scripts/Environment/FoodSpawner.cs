using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public float foodSpawnChance = 0.1f;

    public void SpawnFood(int width, int height, TemperatureMap temperatureMap)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float temperature = temperatureMap.GetTemperatureAt(x, y);

                if (Random.value < foodSpawnChance)
                {
                    if (temperature < 50f) // Temperature threshold for food spawning
                    {
                        Vector3 spawnPosition = new Vector3(x, 0, y);
                        Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
                    }
                }
            }
        }
    }
}
using System.Collections;
using UnityEngine;

public class Food : MonoBehaviour
{
    public float despawnTime = 10f; // Time before food despawns in seconds
    public float nutritionValue = 10f; // Amount of energy a creature gets when it eats the food

    private float timer;

    private void Start()
    {
        // Initialize the timer with the despawn time
        timer = despawnTime;
    }

    private void FixedUpdate()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            DespawnFood();
        }
    }

    private void DespawnFood()
    {
        // Destroy the food if the timer runs out
        Destroy(gameObject);
    }
}
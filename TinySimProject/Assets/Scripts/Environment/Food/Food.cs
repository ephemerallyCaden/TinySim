using System.Collections;
using UnityEngine;

public class Food : MonoBehaviour
{
    public Vector3 position;
    public float size;
    public Color colour = new Color(0.2f, 0.5f, 0.05f, 1);
    public float despawnTime;
    public float nutritionValue;

    public CircleCollider2D col;

    private float timer;

    private void Start()
    {
        //Instantiate variables
        position = transform.position;

        despawnTime = Random.Range(800f, 1200f);
        timer = despawnTime;

        nutritionValue = Random.Range(10f, 40f);
        size = nutritionValue * 0.01f;
        col.radius = size;
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
        //Notify the manager object for removal of food
        FoodSpawner.instance.FoodListRemove(this);
        
        Destroy(gameObject);
    }

}
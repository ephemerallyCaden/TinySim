using System.Collections;
using UnityEngine;

public class Food : MonoBehaviour
{
    public Vector3 position;
    public float size;
    public Color colour = new Color(0.2f, 0.5f, 0.05f, 1);
    public float despawnTime = 100f;
    public float nutritionValue;

    CircleCollider2D col;

    private float timer;

    private void Start()
    {
        position = transform.position;
        timer = despawnTime;
        nutritionValue = Random.Range(5f, 20f);
        size = nutritionValue * 0.02f;

        col.radius = size;
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
        FoodSpawner.instance.FoodListRemove(this);
        Destroy(gameObject);
    }

}
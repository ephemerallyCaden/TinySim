using UnityEngine;
using System.Collections.Generic;

public class FoodRenderer : MonoBehaviour
{
    private Mesh circleMesh; // Mesh for rendering food
    public Material foodMaterial; // Material for food rendering

    private List<Matrix4x4> matrices = new List<Matrix4x4>(); // Transformation matrices for food
    private List<Vector4> colours = new List<Vector4>(); // Colors for food

    private List<Food> foodList; // Reference to the list of food objects

    private void Start()
    {
        // Generate the 2D circle mesh for food rendering
        circleMesh = CircleMeshGenerator.GenerateCircleMesh(6); // 6 segments for the circle as they are small so can afford to be lower quality
    }

    private void Update()
    {
        foodList = FoodSpawner.instance.foodList;

        // Clear the lists for this frame
        matrices.Clear();
        colours.Clear();

        foreach (Food food in foodList)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(
                food.position, // Food position
                Quaternion.identity, // No rotation
                Vector3.one * food.size // Scale by food size
            );

            matrices.Add(matrix);
            colours.Add(food.colour); // Store the color of the food
        }

        // Skip rendering if there are no food objects
        if (matrices.Count == 0 || colours.Count == 0)
        {
            return;
        }

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        propertyBlock.SetVectorArray("_Color", colours);

        for (int i = 0; i < matrices.Count; i += 1023)
        {
            int count = Mathf.Min(1023, matrices.Count - i);
            Graphics.DrawMeshInstanced(circleMesh, 0, foodMaterial, matrices.GetRange(i, count), propertyBlock);
        }
    }
}
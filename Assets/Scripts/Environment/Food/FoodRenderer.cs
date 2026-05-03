using UnityEngine;

public class FoodRenderer : InstancedRenderer
{
    protected override Mesh CreateMesh()
    {
        return CircleMeshGenerator.GenerateCircleMesh(6);
    }

    protected override void PopulateRenderData()
    {
        var foodList = FoodSpawner.instance.foodList;

        foreach (Food food in foodList)
        {
            if (food == null) continue;

            AddInstance(
                Matrix4x4.TRS(food.position, Quaternion.identity, Vector3.one * food.size),
                food.colour
            );
        }
    }
}

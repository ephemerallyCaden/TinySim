using UnityEngine;

public static class CircleMeshGenerator
{
    public static Mesh GenerateCircleMesh(int segments = 32)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        // Center vertex
        vertices[0] = Vector3.zero;

        // Circle vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = (2 * Mathf.PI * i) / segments;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        }

        // Create triangles
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0; // Center vertex
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 2 > segments) ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}

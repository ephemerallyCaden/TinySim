using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for GPU-instanced mesh rendering.
/// Uses pre-allocated arrays to avoid per-frame GC allocations.
/// </summary>
public abstract class InstancedRenderer : MonoBehaviour
{
    private const int GPU_INSTANCE_BATCH_LIMIT = 1023;
    private const int INITIAL_CAPACITY = 1024;

    protected Mesh mesh;
    public Material material;
    protected MaterialPropertyBlock propertyBlock;

    // Pre-allocated arrays — grow as needed, never shrink
    private Matrix4x4[] _matrixArray = new Matrix4x4[INITIAL_CAPACITY];
    private Vector4[] _colourArray = new Vector4[INITIAL_CAPACITY];
    private Matrix4x4[] _batchMatrices = new Matrix4x4[GPU_INSTANCE_BATCH_LIMIT];
    private Vector4[] _batchColours = new Vector4[GPU_INSTANCE_BATCH_LIMIT];
    protected int instanceCount;

    protected virtual void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
        mesh = CreateMesh();
    }

    protected abstract Mesh CreateMesh();
    protected abstract void PopulateRenderData();

    /// <summary>
    /// Add an instance to render this frame. Call from PopulateRenderData.
    /// </summary>
    protected void AddInstance(Matrix4x4 matrix, Vector4 colour)
    {
        if (instanceCount >= _matrixArray.Length)
        {
            // Double capacity
            int newSize = _matrixArray.Length * 2;
            var newMatrices = new Matrix4x4[newSize];
            var newColours = new Vector4[newSize];
            System.Array.Copy(_matrixArray, newMatrices, _matrixArray.Length);
            System.Array.Copy(_colourArray, newColours, _colourArray.Length);
            _matrixArray = newMatrices;
            _colourArray = newColours;
        }
        _matrixArray[instanceCount] = matrix;
        _colourArray[instanceCount] = colour;
        instanceCount++;
    }

    protected virtual void Update()
    {
        instanceCount = 0;

        PopulateRenderData();

        if (instanceCount == 0) return;

        RenderBatched();
    }

    private void RenderBatched()
    {
        for (int i = 0; i < instanceCount; i += GPU_INSTANCE_BATCH_LIMIT)
        {
            int count = Mathf.Min(GPU_INSTANCE_BATCH_LIMIT, instanceCount - i);

            System.Array.Copy(_colourArray, i, _batchColours, 0, count);
            System.Array.Copy(_matrixArray, i, _batchMatrices, 0, count);

            propertyBlock.Clear();
            propertyBlock.SetVectorArray("_Color", _batchColours);
            Graphics.DrawMeshInstanced(mesh, 0, material, _batchMatrices, count, propertyBlock);
        }
    }
}

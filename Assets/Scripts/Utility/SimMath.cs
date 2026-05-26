using UnityEngine;

// General-purpose math utilities for the simulation.
public static class SimMath
{
    // Clamps a value using min/max bounds. A value of -1 means "no limit" for that bound.
    public static float Clamp(float value, float min, float max)
    {
        float effectiveMin = (min == -1f) ? float.MinValue : min;
        float effectiveMax = (max == -1f) ? float.MaxValue : max;
        return Mathf.Clamp(value, effectiveMin, effectiveMax);
    }
}

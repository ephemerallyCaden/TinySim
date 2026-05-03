using System;
using UnityEngine;

/// <summary>
/// Central random number provider for the simulation.
/// All systems should use this instead of creating their own System.Random
/// or calling UnityEngine.Random directly.
/// </summary>
public static class SimRandom
{
    private static System.Random random;
    private static int currentSeed;

    /// <summary>
    /// Initialise with a specific seed for reproducible runs,
    /// or -1 for a time-based random seed.
    /// </summary>
    public static void Initialise(int seed)
    {
        if (seed < 0)
            seed = Environment.TickCount;

        currentSeed = seed;
        random = new System.Random(seed);
        UnityEngine.Random.InitState(seed);
    }

    public static int Seed => currentSeed;

    /// <summary>Returns a random double in [0, 1).</summary>
    public static double NextDouble() => random.NextDouble();

    /// <summary>Returns a random float in [0, 1).</summary>
    public static float NextFloat() => (float)random.NextDouble();

    /// <summary>Returns a random int in [0, max).</summary>
    public static int NextInt(int max) => random.Next(max);

    /// <summary>Returns a random int in [min, max).</summary>
    public static int Range(int min, int max) => random.Next(min, max);

    /// <summary>Returns a random float in [min, max).</summary>
    public static float Range(float min, float max) => min + (float)random.NextDouble() * (max - min);

    /// <summary>Returns a random value in [-1, 1).</summary>
    public static float NextSignedFloat() => (float)(random.NextDouble() * 2.0 - 1.0);
}

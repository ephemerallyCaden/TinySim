using System;
using UnityEngine;

// Central random number provider for the simulation.
// All systems should use this
public static class SimRandom
{
    private static System.Random random;
    private static int currentSeed;

    // Initialise with a specific seed or -1 for a random seed.
    public static void Initialise(int seed)
    {
        if (seed < 0)
            seed = Environment.TickCount;

        currentSeed = seed;
        random = new System.Random(seed);
        UnityEngine.Random.InitState(seed);
    }

    public static int Seed => currentSeed;

    // Returns a random double in [0, 1];
    public static double NextDouble() => random.NextDouble();

    // Returns a random float in [0, 1].
    public static float NextFloat() => (float)random.NextDouble();

    // Returns a random int in [0, max].
    public static int NextInt(int max) => random.Next(max);

    // Returns a random int in [min, max].
    public static int Range(int min, int max) => random.Next(min, max);

    // Returns a random float in [min, max[].
    public static float Range(float min, float max) => min + (float)random.NextDouble() * (max - min);

    // Returns a random value in [-1, 1].
    public static float NextSignedFloat() => (float)(random.NextDouble() * 2.0 - 1.0);

    // Returns a Gaussian-distributed value (mean=0, std=1) using Box-Muller transform.
    // ~68% within ±1, ~95% within ±2, rare outliers beyond ±3.
    private static bool hasSpare = false;
    private static double spare;

    public static float Gaussian()
    {
        if (hasSpare)
        {
            hasSpare = false;
            return (float)spare;
        }

        double u, v, s;
        do
        {
            u = random.NextDouble() * 2.0 - 1.0;
            v = random.NextDouble() * 2.0 - 1.0;
            s = u * u + v * v;
        } while (s >= 1.0 || s == 0.0);

        s = Math.Sqrt(-2.0 * Math.Log(s) / s);
        spare = v * s;
        hasSpare = true;
        return (float)(u * s);
    }
}

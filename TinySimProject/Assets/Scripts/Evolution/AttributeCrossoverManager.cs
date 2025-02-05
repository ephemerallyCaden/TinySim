using System;
using UnityEngine;

public static class AttributeCrossoverManager
{
    // Crossover attributes from two parents
    public static (float size, float speed, Color colour, float visionDistance, float visionAngle) CrossoverAttributes(Agent parent1, Agent parent2)
    {
        // Average the attributes
        float size = (parent1.size + parent2.size) / 2f;
        float speed = (parent1.speed + parent2.speed) / 2f;
        Color colour = Color.Lerp(parent1.colour, parent2.colour, 0.5f);
        float visionDistance = (parent1.visionDistance + parent2.visionDistance) / 2f;
        float visionAngle = (parent1.visionAngle + parent2.visionAngle) / 2f;

        return (size, speed, colour, visionDistance, visionAngle);
    }

    // Mutate attributes
    public static void MutateAttributes(ref float size, ref float speed, ref Color colour, ref float visionDistance, ref float visionAngle, float mutationChance, float mutationMagnitude)
    {
        System.Random random = new System.Random();
        // Mutate size
        if (random.NextDouble() < mutationChance)
        {
            size += (float)(random.NextDouble() - 0.5) * mutationMagnitude;
            size = Mathf.Clamp(size, 0.1f, 10f); // Clamp to reasonable values
        }

        // Mutate speed
        if (random.NextDouble() < mutationChance)
        {
            speed += (float)(random.NextDouble() - 0.5) * mutationMagnitude;
            speed = Mathf.Clamp(speed, 0.1f, 10f); // Clamp to reasonable values
        }

        // Mutate colour
        if (random.NextDouble() < mutationChance)
        {
            colour.r += (float)(random.NextDouble() - 0.5) * mutationMagnitude;
            colour.g += (float)(random.NextDouble() - 0.5) * mutationMagnitude;
            colour.b += (float)(random.NextDouble() - 0.5) * mutationMagnitude;

            colour.r = Mathf.Clamp01(colour.r);
            colour.g = Mathf.Clamp01(colour.g);
            colour.b = Mathf.Clamp01(colour.b);
        }

        //Mutate Vision Distance
        if (random.NextDouble() < mutationChance)
        {
            visionDistance += (float)(random.NextDouble() - 0.5) * mutationMagnitude;
            visionDistance = Mathf.Clamp(visionDistance, 0.1f, 10f); // Clamp to reasonable values
        }

        //Mutate Vision Angle
        if (random.NextDouble() < mutationChance)
        {
            visionAngle += (float)(random.NextDouble() - 0.5) * mutationMagnitude;
            visionAngle = Mathf.Clamp(visionAngle, 0f, 360f); // Clamp to reasonable values
        }
    }
}
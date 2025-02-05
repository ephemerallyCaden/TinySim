using System;
using UnityEngine;

public static class ReproductionManager
{
    public static Agent Reproduce(Agent parent1, Agent parent2)
    {
        // Create new child agent
        Agent child = new GameObject("ChildAgent").AddComponent<Agent>();

        // Crossover genomes
        child.genome = CrossoverManager.Crossover(parent1.genome, parent2.genome);

        // Crossover attributes
        var (size, speed, colour, visionDistance, visionAngle) = AttributeCrossoverManager.CrossoverAttributes(parent1, parent2);
        child.size = size;
        child.speed = speed;
        child.colour = colour;
        child.visionDistance = visionDistance;
        child.visionAngle = visionAngle;

        // Mutate genome
        MutationManager.Mutate(child.genome, parent1.mutationChance, parent1.mutationMagnitude);

        // Mutate attributes
        AttributeCrossoverManager.MutateAttributes(
            ref child.size,
            ref child.speed,
            ref child.colour,
            ref child.visionDistance,
            ref child.visionAngle,
            parent1.mutationChance,
            parent1.mutationMagnitude
        );

        // Initializ=se offspring neural network
        child.network = new NeuralNetwork(child.genome);

        // Set other properties
        child.generation = parent1.generation+=1;
        child.mutationChance = parent1.mutationChance;
        child.mutationMagnitude = parent1.mutationMagnitude;
        child.maxEnergy = parent1.maxEnergy;
        child.movementCost = parent1.movementCost;

        return child;
    }
}
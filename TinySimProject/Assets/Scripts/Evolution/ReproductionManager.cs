using System;
using UnityEngine;

public static class ReproductionManager
{
    public static void Reproduce(Agent parent1, Agent parent2, Vector3 position)
    {
        // Crossover genomes
        Genome childGenome = CrossoverManager.Crossover(parent1.genome, parent2.genome);

        // Crossover attributes
        var (size, speed, colour, visionDistance, visionAngle, mutationChanceMod, mutationMagnitudeMod, maxReproductionCooldown, reproductionEnergyCost) =
            AttributeCrossoverManager.CrossoverAttributes(parent1, parent2);

        // Mutate genome
        MutationManager.Mutate(childGenome, parent1.mutationChance, parent1.mutationMagnitude);

        // Mutate attributes
        AttributeCrossoverManager.MutateAttributes(
            ref size,
            ref speed,
            ref colour,
            ref visionDistance,
            ref visionAngle,
            ref mutationChanceMod,
            ref mutationMagnitudeMod,
            ref maxReproductionCooldown,
            ref reproductionEnergyCost,
            parent1.mutationChance,
            parent1.mutationMagnitude
        );

        // Initialize offspring neural network
        NeuralNetwork childNetwork = new NeuralNetwork(childGenome);

        // Create offspring agent
        AgentManager.instance.CreateAgent(
            position,
            size,
            speed,
            colour,
            visionDistance,
            visionAngle,
            100f, //This is the health
            parent1.maxEnergy,  // Starts at full energy
            maxReproductionCooldown,
            reproductionEnergyCost,
            childGenome,
            childNetwork
        );
    }
}
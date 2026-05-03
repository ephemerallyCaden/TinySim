using UnityEngine;

public static class ReproductionManager
{
    public static void Reproduce(Agent parent1, Agent parent2, Vector3 position)
    {
        // Crossover genomes
        Genome childGenome = CrossoverManager.Crossover(parent1.genome, parent2.genome);

        // Crossover and mutate attributes
        AgentAttributes attrs = AttributeCrossoverManager.CrossoverAttributes(parent1, parent2);
        MutationManager.Mutate(childGenome, parent1.mutationChance, parent1.mutationMagnitude);
        AttributeCrossoverManager.MutateAttributes(ref attrs, parent1.mutationChance, parent1.mutationMagnitude);

        // Initialize offspring neural network
        NeuralNetwork childNetwork = new NeuralNetwork(childGenome);

        // Create offspring agent
        SimulationConfig cfg = SimulationManager.instance.config;
        AgentManager.instance.CreateAgent(
            parent1.generation + 1,
            position,
            attrs,
            cfg.offspringHealthBase,
            parent1.reproductionEnergyCost * cfg.offspringEnergyMultiplier,
            attrs.size * cfg.maxEnergyPerSize,
            childGenome,
            childNetwork,
            parent1.speciesId
        );
    }
}

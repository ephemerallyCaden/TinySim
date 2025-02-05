using System;
using System.Collections.Generic;
using System.Linq;

public class CrossoverManager
{
    public static Genome Crossover(Genome parent1, Genome parent2)
    {
        // Create a new child genome
        Genome child = new Genome(parent1.nodeGenes, new List<ConnectionGene>());

        // Create a dictionary of connections for quick lookup
        var parent1Connections = parent1.connectionGenes.ToDictionary(c => c.linkid.id);
        var parent2Connections = parent2.connectionGenes.ToDictionary(c => c.linkid.id);

        // Iterate through all connections in both parents
        foreach (var innovation in parent1Connections.Keys)
        {
            ConnectionGene gene;

            // If both parents have the same innovation number, randomly choose one
            if (parent1Connections.TryGetValue(innovation, out var p1Gene) &&
                parent2Connections.TryGetValue(innovation, out var p2Gene))
            {
                gene = UnityEngine.Random.value < 0.5 ? p1Gene : p2Gene;
            }
            // If only one parent has the innovation, take it from the fitter parent
            else
            {
                gene = p1Gene;
            }

            // Add the chosen gene to the child
            child.connectionGenes.Add(new ConnectionGene
            {
                linkid = gene.linkid,
                weight = gene.weight,
                enabled = gene.enabled
            });
        }

        return child;
    }
}
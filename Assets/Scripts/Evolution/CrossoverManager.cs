using System;
using System.Collections.Generic;
using System.Linq;

public class CrossoverManager
{


    public static Genome Crossover(Genome parent1, Genome parent2)
    {
        // Create a new child genome
        List<NodeGene> childNodeGenes = new List<NodeGene>();
        Genome child = new Genome(childNodeGenes, new List<ConnectionGene>());

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
                gene = SimRandom.NextFloat() < 0.5f ? p1Gene : p2Gene;
            }
            // If only one parent has the innovation, take it from the fitter parent
            else
            {
                gene = p1Gene;  // Adjust this based on fitness if needed
            }

            // Add the chosen gene to the child
            child.connectionGenes.Add(new ConnectionGene(gene.linkid, gene.weight, gene.enabled));
        }
        // Collect all the node IDs used in the connections
        HashSet<int> usedNodes = new HashSet<int>();

        foreach (var conn in child.connectionGenes)
        {
            usedNodes.Add(conn.linkid.source);
            usedNodes.Add(conn.linkid.target);
        }

        // Add only the nodes that are used in the connections to the child nodeGenes
        // Also add input and output nodes. Deep-clone to avoid shared references with parents.
        HashSet<int> addedNodeIds = new HashSet<int>();

        foreach (NodeGene nodegene in parent1.nodeGenes)
        {
            if ((usedNodes.Contains(nodegene.id) || nodegene.type != NodeType.Hidden) && !addedNodeIds.Contains(nodegene.id))
            {
                child.nodeGenes.Add(new NodeGene(nodegene.id, nodegene.type, nodegene.bias, nodegene.activationFunction));
                addedNodeIds.Add(nodegene.id);
            }
        }

        // Also check parent2 for hidden nodes referenced by connections but missing from parent1
        foreach (NodeGene nodegene in parent2.nodeGenes)
        {
            if (usedNodes.Contains(nodegene.id) && !addedNodeIds.Contains(nodegene.id))
            {
                child.nodeGenes.Add(new NodeGene(nodegene.id, nodegene.type, nodegene.bias, nodegene.activationFunction));
                addedNodeIds.Add(nodegene.id);
            }
        }

        return child;

    }
}
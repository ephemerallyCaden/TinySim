using System;
using System.Collections.Generic;
using System.Linq;
public class MutationManager
{
    private static readonly Random random = new Random();

    public static void Mutate(Genome genome, float mutationChance, float mutationMagnitude)
    {
        int mutationCount = (int)Math.Round(8 * random.NextDouble() * mutationMagnitude);

        for (int i = 0; i < 128; i++)
        {
            if (random.NextDouble() < mutationChance)
            {
                double mutationType = random.NextDouble();
                if (mutationType < 0.4)
                {
                    MutateWeights(genome);
                }
                else if (mutationType < 0.7)
                {
                    AddRandomConnection(genome);
                }
                else if (mutationType < 0.75)
                {
                    DisableRandomConnection(genome);
                }
                else if (mutationType < 0.9)
                {
                    ChangeRandomNodeActivation(genome);
                }
                else
                {
                    AddRandomNode(genome);
                }
            }
        }

    }

    // 1. Mutate connection weights
    private static void MutateWeights(Genome genome)
    {
        float weightMutationChance = 0.8f;
        foreach (var connection in genome.connectionGenes)
        {
            if (random.NextDouble() < weightMutationChance)
            {
                // Small random change to the weight
                connection.weight += (random.NextDouble() - 0.5) * 0.2;
            }
        }
    }

    //Add a new random connection
    private static void AddRandomConnection(Genome genome)
    {
        if (genome.nodeGenes.Count < 2) return; // Ensure at least two nodes exist

        NodeGene node1, node2;
        int attempts = 0, maxAttempts = 10;

        do
        {
            node1 = genome.nodeGenes[random.Next(genome.nodeGenes.Count)];
            node2 = genome.nodeGenes[random.Next(genome.nodeGenes.Count)];
            attempts++;

            // Ensure valid node types and avoid loops
        } while ((node1.id == node2.id ||
                  (node1.type == NodeType.Output) ||
                  (node2.type == NodeType.Input) ||
                  genome.connectionGenes.Exists(c => c.linkid.source == node1.id && c.linkid.target == node2.id))
                 && attempts < maxAttempts);

        if (attempts >= maxAttempts) return; // Avoid infinite loops if no valid nodes are found

        // Create a new connection
        var link = new LinkID
        {
            id = InnovationTracker.GetInnovation(node1.id, node2.id),
            source = node1.id,
            target = node2.id
        };

        genome.connectionGenes.Add(new ConnectionGene
        {
            linkid = link,
            weight = random.NextDouble() * 2 - 1, // Random weight between -1 and 1
            enabled = true
        });
    }


    // Disable random connection
    private static void DisableRandomConnection(Genome genome)
    {
        if (genome.connectionGenes.Count > 0)
        {
            // Select random connection to disable
            var connection = genome.connectionGenes[random.Next(genome.connectionGenes.Count)];
            connection.enabled = false;

            // Check if the target node has no incoming connections
            var targetNode = genome.nodeGenes.FirstOrDefault(n => n.id == connection.linkid.target);
            if (targetNode != null && targetNode.type == NodeType.Hidden && !genome.connectionGenes.Any(c => c.linkid.target == targetNode.id && c.enabled))
            {
                // Remove the loose node and its outgoing connections
                genome.nodeGenes.Remove(targetNode);
                genome.connectionGenes.RemoveAll(c => c.linkid.source == targetNode.id || c.linkid.target == targetNode.id);
            }
        }
    }

    //Add a node between an existing connection
    private static void AddRandomNode(Genome genome)
    {
        if (genome.connectionGenes.Count > 0)
        {
            // Select a random connection to split
            var connection = genome.connectionGenes[random.Next(genome.connectionGenes.Count)];
            connection.enabled = false; // Disable the old connection

            // Create a new node
            var newNode = new NodeGene(
                InnovationTracker.GetNextNodeId(),
                NodeType.Hidden,
                0.0,
                ActivationFunctions.Sigmoid
            );

            genome.nodeGenes.Add(newNode);

            // Create two new connections
            var link1 = new LinkID
            {
                id = InnovationTracker.GetInnovation(connection.linkid.source, newNode.id),
                source = connection.linkid.source,
                target = newNode.id
            };
            genome.connectionGenes.Add(new ConnectionGene
            {
                linkid = link1,
                weight = 1.0, // Fixed weight for the first connection
                enabled = true
            });

            var link2 = new LinkID
            {
                id = InnovationTracker.GetInnovation(newNode.id, connection.linkid.target),
                source = newNode.id,
                target = connection.linkid.target
            };
            genome.connectionGenes.Add(new ConnectionGene
            {
                linkid = link2,
                weight = connection.weight, // Inherit weight from the old connection
                enabled = true
            });
        }
    }

    //Change a random node's activation function
    private static void ChangeRandomNodeActivation(Genome genome)
    {
        if (genome.nodeGenes.Count > 0)
        {
            var node = genome.nodeGenes[random.Next(genome.nodeGenes.Count)];
            node.activationFunction = ActivationFunctions.Sigmoid;
        }
    }
}

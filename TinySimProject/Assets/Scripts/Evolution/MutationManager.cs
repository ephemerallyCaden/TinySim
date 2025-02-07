using System;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;
public class MutationManager
{
    private static readonly System.Random random = new System.Random();

    public static void Mutate(Genome genome, float mutationChance, float mutationMagnitude)
    {
        int mutationCount = (int)Math.Ceiling(4 * mutationMagnitude);
        for (int i = 0; i < mutationCount; i++)
        {
            double mutationValue = random.NextDouble();
            if (mutationValue < mutationChance + 10)
            {
                double mutationType = random.NextDouble();
                if (mutationType < 0.7)
                {
                    if (genome.connectionGenes.Count >= 0 && mutationType < 0.5)
                    {
                        MutateWeights(genome);
                        Debug.Log("MutateWeights");
                        return;
                    }

                    AddRandomConnection(genome);
                    Debug.Log("AddRandomConnection");


                }
                else if (mutationType < 0.75)
                {
                    DisableRandomConnection(genome);
                    Debug.Log("DisableRandomConnection");
                }
                else if (mutationType < 0.9)
                {

                    ChangeRandomNodeActivation(genome);
                    Debug.Log("ChangeRandomNodeActivation");
                }
                else
                {
                    AddRandomNode(genome);
                    Debug.Log("AddRandomNode");
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
        List<NodeGene> possibleSources = new List<NodeGene>();
        List<NodeGene> possibleTargets = new List<NodeGene>();

        foreach (var node in genome.nodeGenes)
        {
            if (node.type != NodeType.Output) possibleSources.Add(node); // Outputs cannot be sources
            if (node.type != NodeType.Input) possibleTargets.Add(node);  // Inputs cannot be targets
        }

        if (possibleSources.Count == 0 || possibleTargets.Count == 0) return; // No valid nodes

        int maxAttempts = 10, attempts = 0;
        NodeGene randomSource, randomTarget;

        do
        {
            randomSource = possibleSources[UnityEngine.Random.Range(0, possibleSources.Count)];
            randomTarget = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
            attempts++;

            // Avoid self-connections and duplicate links
        } while ((randomSource == randomTarget || genome.connectionGenes.Exists(c => c.linkid.source == randomSource.id && c.linkid.target == randomTarget.id))
                 && attempts < maxAttempts);

        if (attempts >= maxAttempts) return; // No valid connection found

        double randomWeight = UnityEngine.Random.Range(-1f, 1f);

        // Get unique innovation number
        int innovationID = InnovationTracker.GetInnovation(randomSource.id, randomTarget.id);

        LinkID linkid = new LinkID(innovationID, randomSource.id, randomTarget.id);
        genome.connectionGenes.Add(new ConnectionGene(linkid, randomWeight, true));

        Debug.Log($"New connection added: {randomSource.id} -> {randomTarget.id} (Weight: {randomWeight})");
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
            int newNodeId = 0;
            do
            {
                newNodeId = InnovationTracker.GetNextNodeId();
            } while (genome.nodeGenes.Any(n => n.id == newNodeId));
            // Create a new node
            var newNode = new NodeGene(
                newNodeId,
                NodeType.Hidden,
                0.0,
                ActivationFunctions.Sigmoid
            );

            genome.nodeGenes.Add(newNode);

            // Create two new connections
            int sourceId = connection.linkid.source;
            int targetId = connection.linkid.target;
            var link1 = new LinkID(InnovationTracker.GetInnovation(sourceId, newNode.id), sourceId, newNode.id);

            genome.connectionGenes.Add(new ConnectionGene(link1, 1.0, true));

            var link2 = new LinkID(InnovationTracker.GetInnovation(newNode.id, targetId), newNode.id, targetId);
            genome.connectionGenes.Add(new ConnectionGene(link2, connection.weight, true));
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

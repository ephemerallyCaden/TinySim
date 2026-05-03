using System;
using System.Collections.Generic;

using System.Linq;
using UnityEngine;
public class MutationManager
{
    public static void Mutate(Genome genome, float mutationChance, float mutationMagnitude)
    {
        SimulationConfig cfg = SimulationManager.instance?.config;
        int mutationsPerMag = cfg != null ? cfg.mutationsPerMagnitude : 4;
        int mutationCount = (int)Math.Ceiling(mutationsPerMag * mutationMagnitude);
        for (int i = 0; i < mutationCount; i++)
        {
            double mutationValue = SimRandom.NextDouble();
            if (mutationValue < mutationChance)
            {
                double mutationType = SimRandom.NextDouble();

                // Thresholds from SimulationConfig
                float t1 = cfg != null ? cfg.weightOrConnectionThreshold : 0.50f;
                float t2 = cfg != null ? cfg.disableConnectionThreshold : 0.75f;
                float t3 = cfg != null ? cfg.changeActivationThreshold : 0.90f;
                float weightFraction = cfg != null ? cfg.weightMutationFraction : 0.57f;

                if (mutationType < t1)
                {
                    if (genome.connectionGenes.Count >= 1 && mutationType < t1 * weightFraction)
                    {
                        MutateWeights(genome);
                        return;
                    }
                    AddRandomConnection(genome);
                }
                else if (mutationType < t2)
                {
                    // Half the time disable a connection, half the time try to prune a hidden node
                    if (SimRandom.NextFloat() < 0.5f)
                        DisableRandomConnection(genome);
                    else
                        PruneRandomHiddenNode(genome);
                }
                else if (mutationType < t3)
                {
                    ChangeRandomNodeActivationFunction(genome);
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
        SimulationConfig cfg = SimulationManager.instance?.config;
        float weightMutationChance = cfg != null ? cfg.weightPerConnectionMutationChance : 0.25f;
        float perturbationScale = cfg != null ? cfg.weightPerturbationScale : 0.2f;
        foreach (var connection in genome.connectionGenes)
        {
            if (SimRandom.NextDouble() < weightMutationChance)
            {
                connection.weight += (SimRandom.NextDouble() - 0.5) * perturbationScale;
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

        int maxAttempts = 20, attempts = 0;
        NodeGene randomSource, randomTarget;

        do
        {
            randomSource = possibleSources[SimRandom.NextInt(possibleSources.Count)];
            randomTarget = possibleTargets[SimRandom.NextInt(possibleTargets.Count)];
            attempts++;

            // Avoid self-connections, duplicate links, and cycles (don't connect output->hidden or hidden->input direction)
        } while ((randomSource == randomTarget
                 || genome.connectionGenes.Exists(c => c.linkid.source == randomSource.id && c.linkid.target == randomTarget.id)
                 || WouldCreateCycle(genome, randomSource.id, randomTarget.id))
                 && attempts < maxAttempts);

        if (attempts >= maxAttempts) return; // No valid connection found

        double randomWeight = SimRandom.Range(-1f, 1f);

        // Get unique innovation number
        int innovationID = InnovationTracker.GetInnovation(randomSource.id, randomTarget.id);

        LinkID linkid = new LinkID(innovationID, randomSource.id, randomTarget.id);
        genome.connectionGenes.Add(new ConnectionGene(linkid, randomWeight, true));
    }




    // Check if adding a connection from source to target would create a cycle
    private static bool WouldCreateCycle(Genome genome, int sourceId, int targetId)
    {
        // If target can reach source through existing connections, adding source->target creates a cycle
        HashSet<int> visited = new HashSet<int>();
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(targetId);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            if (current == sourceId) return true;
            if (!visited.Add(current)) continue;

            foreach (var conn in genome.connectionGenes)
            {
                if (conn.enabled && conn.linkid.source == current)
                {
                    queue.Enqueue(conn.linkid.target);
                }
            }
        }
        return false;
    }

    // Remove a random hidden node and all its connections, then cascade to clean up orphans
    private static void PruneRandomHiddenNode(Genome genome)
    {
        // Find all hidden nodes
        List<NodeGene> hiddenNodes = genome.nodeGenes.FindAll(n => n.type == NodeType.Hidden);
        if (hiddenNodes.Count == 0)
        {
            // No hidden nodes to prune, fall back to disabling a connection
            DisableRandomConnection(genome);
            return;
        }

        // Pick a random hidden node to remove
        NodeGene nodeToRemove = hiddenNodes[SimRandom.NextInt(hiddenNodes.Count)];
        RemoveHiddenNode(genome, nodeToRemove);
    }

    private static void RemoveHiddenNode(Genome genome, NodeGene node)
    {
        genome.nodeGenes.Remove(node);
        genome.connectionGenes.RemoveAll(c =>
            c.linkid.source == node.id || c.linkid.target == node.id);

        // Cascade: remove any hidden nodes that are now orphaned (no connections in or out)
        List<NodeGene> orphans = genome.nodeGenes.FindAll(n =>
            n.type == NodeType.Hidden &&
            !genome.connectionGenes.Any(c => c.linkid.source == n.id || c.linkid.target == n.id));

        foreach (NodeGene orphan in orphans)
        {
            genome.nodeGenes.Remove(orphan);
        }
    }

    // Disable random connection
    private static void DisableRandomConnection(Genome genome)
    {
        if (genome.connectionGenes.Count > 0)
        {
            // Select random connection to disable
            var connection = genome.connectionGenes[SimRandom.NextInt(genome.connectionGenes.Count)];
            DisableConnection(genome, connection);
        }
    }

    private static void DisableConnection(Genome genome, ConnectionGene connection)
    {
        connection.enabled = false;

        // Check if the source node has no incoming connections
        var sourceNode = genome.nodeGenes.FirstOrDefault(n => n.id == connection.linkid.source);
        if (sourceNode != null && sourceNode.type == NodeType.Hidden && !genome.connectionGenes.Any(c => c.linkid.source == sourceNode.id && c.enabled))
        {
            // Remove the loose node and its outgoing connections
            if (genome.nodeGenes.Contains(sourceNode)) genome.nodeGenes.Remove(sourceNode);
            foreach (ConnectionGene recursiveConnection in genome.connectionGenes.FindAll(c => c.linkid.source == sourceNode.id))
            {
                DisableConnection(genome, recursiveConnection);
            }
        }
        // Check if the target node has no incoming connections
        var targetNode = genome.nodeGenes.FirstOrDefault(n => n.id == connection.linkid.target);
        if (targetNode != null && targetNode.type == NodeType.Hidden && !genome.connectionGenes.Any(c => c.linkid.target == targetNode.id && c.enabled))
        {
            // Remove the loose node and its outgoing connections
            if (genome.nodeGenes.Contains(targetNode)) genome.nodeGenes.Remove(targetNode);
            foreach (ConnectionGene recursiveConnection in genome.connectionGenes.FindAll(c => c.linkid.source == targetNode.id))
            {
                DisableConnection(genome, recursiveConnection);
            }
        }
    }

    //Add a node between an existing connection
    private static void AddRandomNode(Genome genome)
    {
        if (genome.connectionGenes.Count > 0)
        {
            // Select a random connection to split
            var connection = genome.connectionGenes[SimRandom.NextInt(genome.connectionGenes.Count)];
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
    private static void ChangeRandomNodeActivationFunction(Genome genome)
    {
        if (genome.nodeGenes.Count > 0)
        {
            var node = genome.nodeGenes[SimRandom.NextInt(genome.nodeGenes.Count)];
            node.activationFunction = GetRandomActivationFunction();
        }
    }

    // Gets a random activation function
    private static Func<double, double> GetRandomActivationFunction()
    {
        int functionIndex = SimRandom.NextInt(5);
        return ActivationFunctions.functionList[functionIndex];

    }
}

using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Node
{
    public int id;
    public double activation; //The value of the node
    public List<Connection> incoming = new List<Connection>();

}

public class Connection
{
    public LinkID link;
    public double weight;
    public bool enabled;

    public Connection(LinkID _link, double _weight, bool _enabled)
    {
        link = _link;
        weight = _weight;
        enabled = _enabled;
    }
}

public class NeuralNetwork
{
    private List<Node> nodes;
    private List<Connection> connections;
    private List<Node> evaluationOrder;

    public NeuralNetwork(Genome genome)
    {
        BuildNetworkFromGenome(genome);
        BuildEvaluationOrder();
    }

    private void BuildNetworkFromGenome(Genome genome)
    {
        Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();

        foreach (var nodeGene in genome.nodeGenes)
        {
            nodeMap[nodeGene.id] = new Node();
        }

        connections = new List<Connection>();
        foreach (var connectionGene in genome.connectionGenes)
        {
            if (connectionGene.enabled)
            {
                var connection = new Connection(
                    connectionGene.linkid,
                    connectionGene.weight,
                    true
                );
                nodeMap[connection.link.source.id].incoming.Add(connection);
                connections.Add(connection);
            }
        }

        nodes = new List<Node>(nodeMap.Values);
    }
    private void BuildEvaluationOrder()
    {
        evaluationOrder = new List<Node>();
        Dictionary<int, int> inDegree = new Dictionary<int, int>();

        // Initialize in-degree for each node
        foreach (var node in nodes)
        {
            inDegree[node.id] = 0;
        }

        // Count incoming connections (in-degree) for each node
        foreach (var connection in connections)
        {
            if (connection.enabled)
            {
                inDegree[connection.link.target.id]++;
            }
        }

        // Queue for nodes with zero in-degree (ready to evaluate)
        Queue<Node> readyNodes = new Queue<Node>();

        foreach (var node in nodes)
        {
            if (inDegree[node.id] == 0)
            {
                readyNodes.Enqueue(node);
            }
        }

        // Topological sort
        int processedNodes = 0;
        while (readyNodes.Count > 0)
        {
            Node currentNode = readyNodes.Dequeue();
            evaluationOrder.Add(currentNode);
            processedNodes++;

            foreach (var connection in currentNode.incoming)
            {
                inDegree[connection.link.source.id]--;
                if (inDegree[connection.link.source.id] == 0)
                {
                    Node sourceNode = nodes.Find(n => n.id == connection.link.source.id);
                    readyNodes.Enqueue(sourceNode);
                }
            }
        }

        // Cycle detection
        if (processedNodes != nodes.Count)
        {
            Debug.LogError("Cycle detected: Cannot evaluate neural network.");
        }
    }
}


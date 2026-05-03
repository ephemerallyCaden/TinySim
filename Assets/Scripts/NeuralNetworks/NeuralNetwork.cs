using System;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int id;
    public NodeType type;
    public List<Connection> incoming = new List<Connection>();
    public double activation;
    public double bias;
    public Func<double, double> activationFunction;

    public Node(int _id, NodeType _type, double _bias, Func<double, double> _func)
    {
        id = _id;
        type = _type;
        incoming = new List<Connection>();
        activation = 0.0;
        bias = _bias;
        activationFunction = _func;
    }
}

public class Connection
{
    public LinkID link;
    public Node sourceNode; // Pre-resolved reference — eliminates dictionary lookup in hot path
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
    public List<Node> nodes;
    public List<Connection> connections;
    private List<Node> evaluationOrder;
    public List<Node> inputNodes;
    public List<Node> outputNodes;
    private Dictionary<int, Node> nodeMap;
    private double[] _outputCache; // Pre-allocated — no per-frame allocation

    public NeuralNetwork(Genome genome)
    {
        BuildNetworkFromGenome(genome);
        BuildEvaluationOrder();
        _outputCache = new double[outputNodes.Count];
    }

    private void BuildNetworkFromGenome(Genome genome)
    {
        nodeMap = new Dictionary<int, Node>();
        inputNodes = new List<Node>();
        outputNodes = new List<Node>();

        foreach (var nodeGene in genome.nodeGenes)
        {
            Node node = new Node(
                nodeGene.id,
                nodeGene.type,
                nodeGene.bias,
                nodeGene.activationFunction
            );
            nodeMap[nodeGene.id] = node;

            if (nodeGene.type == NodeType.Input) { inputNodes.Add(node); }
            if (nodeGene.type == NodeType.Output) { outputNodes.Add(node); }
        }

        connections = new List<Connection>();
        foreach (var connectionGene in genome.connectionGenes)
        {
            if (connectionGene.enabled)
            {
                if (!nodeMap.ContainsKey(connectionGene.linkid.source) ||
                    !nodeMap.ContainsKey(connectionGene.linkid.target))
                    continue;

                var connection = new Connection(
                    connectionGene.linkid,
                    connectionGene.weight,
                    true
                );
                // Pre-resolve source node reference for O(1) access during evaluation
                connection.sourceNode = nodeMap[connectionGene.linkid.source];
                nodeMap[connection.link.target].incoming.Add(connection);
                connections.Add(connection);
            }
        }

        nodes = new List<Node>(nodeMap.Values);
    }

    private void BuildEvaluationOrder()
    {
        evaluationOrder = new List<Node>();

        // Build outgoing adjacency list for O(N+E) topological sort
        Dictionary<int, List<Connection>> outgoing = new Dictionary<int, List<Connection>>();
        foreach (var node in nodes)
            outgoing[node.id] = new List<Connection>();
        foreach (var connection in connections)
            outgoing[connection.link.source].Add(connection);

        // Count incoming dependencies
        Dictionary<Node, int> dependencyCount = new Dictionary<Node, int>();
        foreach (var node in nodes)
            dependencyCount[node] = node.incoming.Count;

        // Initialise queue with nodes that have no dependencies
        Queue<Node> readyNodes = new Queue<Node>();
        foreach (var node in nodes)
        {
            if (dependencyCount[node] == 0)
                readyNodes.Enqueue(node);
        }

        // Process in topological order using adjacency list (O(N+E))
        while (readyNodes.Count > 0)
        {
            Node currentNode = readyNodes.Dequeue();
            evaluationOrder.Add(currentNode);

            foreach (var connection in outgoing[currentNode.id])
            {
                Node targetNode = nodeMap[connection.link.target];
                dependencyCount[targetNode]--;
                if (dependencyCount[targetNode] == 0)
                    readyNodes.Enqueue(targetNode);
            }
        }

        if (evaluationOrder.Count != nodes.Count)
        {
            Debug.LogError("Cycle detected in neural network topology!!");
        }
    }

    public double[] FeedForward(double[] inputValues)
    {
        // Assign input activations
        for (int i = 0; i < inputNodes.Count; i++)
        {
            inputNodes[i].activation = inputValues[i];
        }

        // Evaluate nodes in topological order
        for (int i = 0; i < evaluationOrder.Count; i++)
        {
            Node node = evaluationOrder[i];
            if (node.type != NodeType.Input)
            {
                double sum = node.bias;
                var incoming = node.incoming;

                for (int j = 0; j < incoming.Count; j++)
                {
                    var c = incoming[j];
                    sum += c.sourceNode.activation * c.weight; // Direct reference — no dictionary lookup
                }
                node.activation = node.activationFunction(sum);
                if (double.IsNaN(node.activation) || double.IsInfinity(node.activation))
                    node.activation = 0.0;
            }
        }

        // Write to pre-allocated cache — zero allocation
        for (int i = 0; i < outputNodes.Count; i++)
        {
            _outputCache[i] = outputNodes[i].activation;
        }

        return _outputCache;
    }
}

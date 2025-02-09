using System;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int id;
    public NodeType type;
    public List<Connection> incoming = new List<Connection>();
    public double activation; //The value of the node
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

    public NeuralNetwork(Genome genome)
    {
        BuildNetworkFromGenome(genome);
        BuildEvaluationOrder();
    }

    private void BuildNetworkFromGenome(Genome genome)
    {
        Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();
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
                var connection = new Connection(
                    connectionGene.linkid,
                    connectionGene.weight,
                    true
                );
                nodeMap[connection.link.target].incoming.Add(connection);
                connections.Add(connection);
            }
        }

        nodes = new List<Node>(nodeMap.Values);
    }
    private void BuildEvaluationOrder()
    {
        evaluationOrder = new List<Node>();
        Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();

        // Map node IDs for quick access
        foreach (var node in nodes)
        {
            nodeMap[node.id] = node;
        }

        // Step 1: Count incoming dependencies for each node
        Dictionary<Node, int> dependencyCount = new Dictionary<Node, int>();
        foreach (var node in nodes)
        {
            dependencyCount[node] = node.incoming.Count;
        }

        // Step 2: Initialize queue with nodes that have no dependencies (input/bias nodes)
        Queue<Node> readyNodes = new Queue<Node>();
        foreach (var node in nodes)
        {
            if (dependencyCount[node] == 0)
            {
                readyNodes.Enqueue(node);
            }
        }

        // Step 3: Process nodes in topological order
        while (readyNodes.Count > 0)
        {
            Node currentNode = readyNodes.Dequeue();
            evaluationOrder.Add(currentNode);

            // Reduce dependency counts for target nodes of outgoing connections
            foreach (var connection in connections)
            {
                if (connection.link.source == currentNode.id && connection.enabled)
                {
                    Node targetNode = nodeMap[connection.link.target];
                    dependencyCount[targetNode]--;

                    if (dependencyCount[targetNode] == 0)
                    {
                        readyNodes.Enqueue(targetNode);
                    }
                }
            }
        }

        // Step 4: Cycle detection
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

                // Fast iteration using for-loop
                for (int j = 0; j < incoming.Count; j++)
                {
                    var c = incoming[j];
                    if (c.enabled)
                    {
                        Node source = nodes.Find(n => n.id == c.link.source);
                        sum += source.activation * c.weight;
                    }
                }
                node.activation = node.activationFunction(sum);
            }
        }

        // Collect output activations
        double[] outputValues = new double[outputNodes.Count];
        for (int i = 0; i < outputValues.Length; i++)
        {
            outputValues[i] = outputNodes[i].activation;
        }

        return outputValues;
    }
}


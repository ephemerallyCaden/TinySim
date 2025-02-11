using UnityEngine;
using System;
using System.Collections.Generic;

public enum NodeType { Input, Hidden, Output };

//A gene to store node information
public class NodeGene
{
    public int id;
    public NodeType type;
    public double bias;
    public Func<double, double> activationFunction;

    public NodeGene(int _id, NodeType _type, double _bias, Func<double, double> _activationFunction)
    {
        id = _id;
        type = _type;
        bias = _bias;
        activationFunction = _activationFunction;
    }
}
public class LinkID
{
    public int id; // acting as the innovation number
    public int source;
    public int target;

    public LinkID(int _id, int _source, int _target)
    {
        id = _id;
        source = _source;
        target = _target;
    }
}

// a gene to store connection information
public class ConnectionGene
{
    public LinkID linkid;
    public double weight;
    public bool enabled;

    public ConnectionGene(LinkID _linkid, double _weight, bool _enabled)
    {
        linkid = _linkid;
        weight = _weight;
        enabled = _enabled;
    }
}

// Genome class
public class Genome
{
    public List<NodeGene> nodeGenes;
    public List<ConnectionGene> connectionGenes;

    public Genome(List<NodeGene> _nodeGenes, List<ConnectionGene> _connectionGenes)
    {
        nodeGenes = _nodeGenes;
        connectionGenes = _connectionGenes;
    }
}
using UnityEngine;
using System;
using System.Collections.Generic;

public enum NodeType { Input, Hidden, Output };
public class NodeGene
{
    public int id;
    public NodeType type;
    public int innovationNumber;
}
public class LinkID
{
    public int id; //acting as the innovation number
    public NodeGene source;
    public NodeGene target;
}

public class ConnectionGene
{
    public LinkID linkid;
    public double weight;
    public bool enabled;
}

public class Genome
{
    public List<NodeGene> nodeGenes;
    public List<ConnectionGene> connectionGenes;
}
using UnityEngine;

/// <summary>
/// Value type holding all heritable/mutable agent attributes.
/// Used by crossover, mutation, and agent creation to avoid long parameter lists.
/// </summary>
public struct AgentAttributes
{
    public float size;
    public float speed;
    public Color colour;
    public float visionDistance;
    public float visionAngle;
    public float mutationChanceMod;
    public float mutationMagnitudeMod;
    public float maxReproductionCooldown;
    public float reproductionEnergyCost;
    public float reproductionRange;
}

using UnityEngine;

// Value type holding all inheritable/mutable agent attributes.
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
    public float attackDamage;
    public float dietPreference;     // 0 = herbivore, 1 = carnivore
}

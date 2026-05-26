
/// Single source of truth for neural network node labels.
public static class NeuralNetworkLabels
{
    // Input node labels — index matches the INPUT_* constants in Agent.cs
    public static readonly string[] InputLabels = new string[]
    {
        "Control",          // 0  - Always 1.0
        "Health",           // 1  - Own health
        "Age",              // 2  - Own age
        "Hunger",           // 3  - Energy deficit (MaxEnergy - Energy) / MaxEnergy
        "Temperature",      // 4  - Local temperature
        "See Agent",        // 5  - Agent detected
        "Agent Proximity",  // 6  - Distance to closest agent
        "Agent Angle",      // 7  - Angle to closest agent
        "Agent Health",     // 8  - Closest agent's health
        "Agent Energy",     // 9  - Closest agent's energy
        "Agent Size",       // 10 - Closest agent's size relative to self
        "Same Species",     // 11 - Closest agent is same species
        "See Food",         // 12 - Plant food detected
        "Food Proximity",   // 13 - Distance to closest plant food
        "Food Angle",       // 14 - Angle to closest plant food
        "See Meat",         // 15 - Meat detected
        "Meat Proximity",   // 16 - Distance to closest meat
        "Meat Angle",       // 17 - Angle to closest meat
        "See Poison",       // 18 - Poison detected
        "Poison Proximity", // 19 - Distance to closest poison
        "Poison Angle",     // 20 - Angle to closest poison
        "Desire",           // 21 - Recurrent desire value
    };

    // Output node labels — index matches output array indices in Agent.ProcessNetwork()
    public static readonly string[] OutputLabels = new string[]
    {
        "Move",    // 0 - Movement speed
        "Turn",    // 1 - Turning rate
        "Desire",  // 2 - Desire value
        "Attack",  // 3 - Attack intent (predation)
    };

    /// Get the label for a node by its ID.
    /// Input nodes: IDs 0 to (InputLabels.Length - 1)
    /// Output nodes: IDs InputLabels.Length to (InputLabels.Length + OutputLabels.Length - 1)
    /// Hidden nodes: all other IDs

    public static string GetLabel(int nodeId)
    {
        if (nodeId < InputLabels.Length)
        {
            return InputLabels[nodeId];
        }

        int outputIndex = nodeId - InputLabels.Length;
        if (outputIndex >= 0 && outputIndex < OutputLabels.Length)
        {
            return OutputLabels[outputIndex];
        }

        return null; // Hidden node — no label
    }
}

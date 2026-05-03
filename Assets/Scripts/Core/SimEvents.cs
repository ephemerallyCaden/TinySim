using System;

/// <summary>
/// Central event bus for simulation events.
/// Systems subscribe to events they care about instead of calling each other directly.
/// </summary>
public static class SimEvents
{
    /// <summary>Fired when an agent dies. Subscribers: AgentManager, AgentStatsUI.</summary>
    public static event Action<Agent> OnAgentDied;

    /// <summary>Fired when food is consumed by an agent. Subscribers: FoodSpawner.</summary>
    public static event Action<Food> OnFoodConsumed;

    /// <summary>Fired when a new agent is born.</summary>
    public static event Action<Agent> OnAgentBorn;

    /// <summary>Fired when a new species is created. (speciesId, parentSpeciesId)</summary>
    public static event Action<int, int> OnSpeciesCreated;

    /// <summary>Fired when a species goes extinct. (speciesId)</summary>
    public static event Action<int> OnSpeciesExtinct;

    public static void AgentDied(Agent agent) => OnAgentDied?.Invoke(agent);
    public static void FoodConsumed(Food food) => OnFoodConsumed?.Invoke(food);
    public static void AgentBorn(Agent agent) => OnAgentBorn?.Invoke(agent);
    /// <summary>Fired when a species evolves into a new one via drift (anagenesis). (oldId, newId)</summary>
    public static event Action<int, int> OnSpeciesEvolved;

    public static void SpeciesCreated(int speciesId, int parentSpeciesId) => OnSpeciesCreated?.Invoke(speciesId, parentSpeciesId);
    public static void SpeciesExtinct(int speciesId) => OnSpeciesExtinct?.Invoke(speciesId);
    public static void SpeciesEvolved(int oldSpeciesId, int newSpeciesId) => OnSpeciesEvolved?.Invoke(oldSpeciesId, newSpeciesId);
}

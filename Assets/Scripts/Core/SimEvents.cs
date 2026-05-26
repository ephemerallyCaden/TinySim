using System;

// Central event bus for simulation events.
// Systems subscribe to events they require.
public static class SimEvents
{
    // Fired when an agent dies. Subscribers: AgentManager, AgentStatsUI, AnalyticsPanel, SimDiagnostics, SpeciationManager.
    public static event Action<Agent> OnAgentDied;

    // Fired when food is consumed by an agent. Subscribers: FoodSpawner, AnalyticsPanel, SimDiagnostics.
    public static event Action<Food> OnFoodConsumed;

    // Fired when a new agent is born. Subscribers: AnalyticsPanel, SimDiagnostics.
    public static event Action<Agent> OnAgentBorn;

    // Fired when a new species is created. Subscribers: AnalyticsPanel, SpeciesHistoryTracker.
    public static event Action<int, int> OnSpeciesCreated;

    // Fired when a species goes extinct. Subscribers: AnalyticsPanel, SpeciesHistoryTracker.
    public static event Action<int> OnSpeciesExtinct;

    // Fired when a species evolves into a new one via drift. Subscribers: SpeciesHistoryTracker.
    public static event Action<int, int> OnSpeciesEvolved;

    public static void AgentDied(Agent agent) => OnAgentDied?.Invoke(agent);
    public static void FoodConsumed(Food food) => OnFoodConsumed?.Invoke(food);
    public static void AgentBorn(Agent agent) => OnAgentBorn?.Invoke(agent);
    public static void SpeciesCreated(int speciesId, int parentSpeciesId) => OnSpeciesCreated?.Invoke(speciesId, parentSpeciesId);
    public static void SpeciesExtinct(int speciesId) => OnSpeciesExtinct?.Invoke(speciesId);
    public static void SpeciesEvolved(int oldSpeciesId, int newSpeciesId) => OnSpeciesEvolved?.Invoke(oldSpeciesId, newSpeciesId);
}

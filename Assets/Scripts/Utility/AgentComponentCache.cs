using System.Collections.Generic;
using UnityEngine;

// Caches Agent/Food component lookups by Collider2D to avoid expensive GetComponent calls
// Components register/unregister themselves at Start/OnDestroy.
public static class AgentComponentCache
{
    private static readonly Dictionary<Collider2D, Agent> _agents = new Dictionary<Collider2D, Agent>();
    private static readonly Dictionary<Collider2D, Food> _food = new Dictionary<Collider2D, Food>();

    public static void RegisterAgent(Collider2D col, Agent agent)
    {
        _agents[col] = agent;
    }

    public static void UnregisterAgent(Collider2D col)
    {
        _agents.Remove(col);
    }

    public static void RegisterFood(Collider2D col, Food food)
    {
        _food[col] = food;
    }

    public static void UnregisterFood(Collider2D col)
    {
        _food.Remove(col);
    }

    public static Agent GetAgent(Collider2D col)
    {
        _agents.TryGetValue(col, out Agent agent);
        return agent;
    }

    public static Food GetFood(Collider2D col)
    {
        _food.TryGetValue(col, out Food food);
        return food;
    }
}

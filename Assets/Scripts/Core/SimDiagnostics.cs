using System.Collections.Generic;
using UnityEngine;

// Periodic agent and environment diagnostic logging
public class SimDiagnostics : MonoBehaviour
{
    [Header("Settings")]
    public float logInterval = 10f; // Log every N seconds of world time
    public bool enableLogging = true;

    private float nextLogTime = 0f;
    private float startTime;

    // Tracked metrics across intervals
    private int totalDeaths = 0;
    private int totalBirths = 0;
    private int foodEaten = 0;
    private int poisonEaten = 0;

    private void OnEnable()
    {
        SimEvents.OnAgentDied += OnDeath;
        SimEvents.OnAgentBorn += OnBirth;
        SimEvents.OnFoodConsumed += OnFoodEaten;
    }

    private void OnDisable()
    {
        SimEvents.OnAgentDied -= OnDeath;
        SimEvents.OnAgentBorn -= OnBirth;
        SimEvents.OnFoodConsumed -= OnFoodEaten;
    }

    private void OnDeath(Agent a) => totalDeaths++;
    private void OnBirth(Agent a) => totalBirths++;
    private void OnFoodEaten(Food f)
    {
        if (f is PoisonFood) poisonEaten++;
        else foodEaten++;
    }

    private void Start()
    {
        startTime = Time.time;
        nextLogTime = logInterval;
    }

    private void Update()
    {
        if (!enableLogging) return;

        float worldTime = SimulationManager.instance.worldTime;
        if (worldTime < nextLogTime) return;
        nextLogTime += logInterval;

        LogSnapshot(worldTime);
    }

    private void LogSnapshot(float worldTime)
    {
        var agents = AgentManager.instance.agents;
        int pop = agents.Count;

        if (pop == 0)
        {
            Debug.Log($"[DIAG t={worldTime:F1}] POPULATION EXTINCT. Deaths={totalDeaths} Births={totalBirths}");
            return;
        }

        // Gather agent stats
        float totalEnergy = 0, totalMaxEnergy = 0;
        float totalSpeed = 0, totalSize = 0;
        int totalConnections = 0, totalNodes = 0;
        int agentsSeeingFood = 0, agentsSeeingAgent = 0;
        float totalMovementSpeed = 0, totalTurningRate = 0;
        float minEnergy = float.MaxValue, maxEnergy = 0;
        float totalAge = 0;
        int maxGen = 0;
        int agentsWithMultipleConnections = 0;

        for (int i = 0; i < pop; i++)
        {
            Agent a = agents[i];
            if (a == null) continue;

            totalEnergy += a.energy;
            totalMaxEnergy += a.maxEnergy;
            totalSpeed += a.speed;
            totalSize += a.size;
            totalAge += a.age;
            totalMovementSpeed += Mathf.Abs(a.movementSpeed);
            totalTurningRate += Mathf.Abs(a.turningRate);

            if (a.energy < minEnergy) minEnergy = a.energy;
            if (a.energy > maxEnergy) maxEnergy = a.energy;
            if (a.generation > maxGen) maxGen = a.generation;

            int conns = a.genome.connectionGenes.Count;
            totalConnections += conns;
            totalNodes += a.genome.nodeGenes.Count;
            if (conns > 1) agentsWithMultipleConnections++;

            // Check if they're seeing things (inputs[9] = see food, inputs[4] = see agent)
            if (a.inputs != null)
            {
                if (a.inputs[9] > 0) agentsSeeingFood++;
                if (a.inputs[4] > 0) agentsSeeingAgent++;
            }
        }

        float avgEnergy = totalEnergy / pop;
        float avgSize = totalSize / pop;
        float avgSpeed = totalSpeed / pop;
        float avgAge = totalAge / pop;
        float avgMove = totalMovementSpeed / pop;
        float avgTurn = totalTurningRate / pop;
        float avgConns = (float)totalConnections / pop;
        float avgNodes = (float)totalNodes / pop;

        Debug.Log($"[DIAG t={worldTime:F1}] Pop={pop} | Gen max={maxGen} avg={AgentManager.instance.avgGeneration}" +
                  $"\n  Energy: avg={avgEnergy:F1} min={minEnergy:F1} max={maxEnergy:F1} (of avg_cap={totalMaxEnergy / pop:F1})" +
                  $"\n  Body: size={avgSize:F2} speed={avgSpeed:F2}" +
                  $"\n  Movement: avgMoveSpeed={avgMove:F3} avgTurnRate={avgTurn:F3}" +
                  $"\n  Vision: seeFood={agentsSeeingFood}/{pop} seeAgent={agentsSeeingAgent}/{pop}" +
                  $"\n  Genome: avgConns={avgConns:F1} avgNodes={avgNodes:F1} multiConn={agentsWithMultipleConnections}/{pop}" +
                  $"\n  Events: deaths={totalDeaths} births={totalBirths} foodEaten={foodEaten} poisonEaten={poisonEaten}");

        // Reset per-interval counters
        totalDeaths = 0;
        totalBirths = 0;
        foodEaten = 0;
        poisonEaten = 0;
    }
}

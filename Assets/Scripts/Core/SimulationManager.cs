using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager instance { get; private set; }

    [Header("Simulation Settings")]
    public float simulationSpeed = 1f;
    public bool isPaused = false;
    public float worldTime = 0f;

    [Header("Configuration (required)")]
    public SimulationConfig config;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            if (config == null)
            {
                Debug.LogError("SimulationManager: No SimulationConfig assigned! Create one via Assets > Create > TinySim > Simulation Config");
                return;
            }

            SimRandom.Initialise(config.seed);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (isPaused) return;

        // Progress the simulation
        float deltaTime = Time.fixedDeltaTime * simulationSpeed;
        worldTime += deltaTime;
        StepSimulation(deltaTime);
    }

    public void StepSimulation(float deltaTime)
    {
        // Update all simulation components
        AgentManager.instance.UpdateAgents(deltaTime);
        FoodSpawner.instance.UpdateFoodSpawner(deltaTime);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
    }

    public void SetSimulationSpeed(float speed)
    {
        simulationSpeed = Mathf.Max(0f, speed);
    }
}

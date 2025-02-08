using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager instance { get; private set; }
    [Header("Simulation Settings")]
    public float simulationSpeed = 1f; // Controls how fast the simulation runs
    public bool isPaused = false;
    public float worldTime = 0f;

    [Header("Global Variables")]
    public int worldSize = 64;
    public float globalMutationChance = 0.5f;
    public float globalMutationMagnitude = 0.5f;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
        //EnvironmentManager.Instance.UpdateEnvironment(deltaTime);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
    }

    public void SetSimulationSpeed(float speed)
    {
        simulationSpeed = Mathf.Max(0f, speed); // Prevent negative speeds
    }

}
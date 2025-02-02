using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance { get; private set; }
    [Header("Simulation Settings")]
    public float simulationSpeed = 1f; // Controls how fast the simulation runs
    public bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        StepSimulation(deltaTime);
    }

    public void StepSimulation(float deltaTime)
    {
        // Update all simulation components
        //AgentManager.Instance.UpdateAgents(deltaTime);
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
using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class Agent : MonoBehaviour
{
    // Agent Characteristics (can mutate)
    public float size;                // Size of the agent
    public float speed;               // Base speed
    public float visionDistance = 10f; // How far the agent can see
    public float visionAngle = 90f;   // Field of view in degrees
    public Color color;               // Agent's color
    public float mutationMagnitudeMod;
    public float mutationChanceMod;
    public float mutationMagnitude;
    public float mutationChance;

    // Agent State
    public float generation;
    public float age;                 // Current age (world time - birth time)
    public float health;              // Current health
    public float maxEnergy;           // Maximum energy capacity
    public float energy;              // Current energy
    public float metabolismCost;      // Energy cost per frame
    public float movementCost;        // Additional energy cost when moving
    public float birthTime;           // Time when the agent was born

    // Neural Network
    public NeuralNetwork network;       // The agent's neural network
    public Genome genome;             // The agent's genome

    // Inputs (preallocated list)
    public double[] inputs;
    public double[] outputs;

    // Outputs
    public float movementSpeed;       // Movement speed (-1 to 1)
    public float turningRate;         // Turning rate (-1 to 1)
    public float desireValue;         // Desire value (used for decision-making)

    // Environment Sensors
    public LayerMask creatureLayer;   // Layer for creatures
    public LayerMask foodLayer;       // Layer for food

    // Closest objects
    private Agent closestAgent;
    private Food closestFood;
    float closestAgentDistance;
    float closestAgentAngle;
    float closestFoodDistance;
    float closestFoodAngle;

    // Preallocated collider buffer for vision detection
    [SerializeField] private Collider2D[] hitList;

    private void Start()
    {
        //Variable Calculations
        metabolismCost = 0.15f * size * speed;

        // Global mutation parameters from the SimulationManager
        float globalMutationChance = SimulationManager.instance.globalMutationChance;
        float globalMutationMagnitude = SimulationManager.instance.globalMutationMagnitude;

        // Final mutation values after applying the agent-specific modifier
        mutationChance = globalMutationChance * (1 + mutationChanceMod);
        mutationMagnitude = globalMutationMagnitude * (1 + mutationMagnitudeMod);

        // Preallocate inputs
        InitialiseInputs();

        // Preallocate collider buffer
        hitList = new Collider2D[20]; // Adjust size based on expected number of objects

        // Set birth time
        birthTime = SimulationManager.instance.worldTime;
    }

    // Preallocate the input list
    private void InitialiseInputs()
    {
        inputs[0] = 1.0;              // Control input (always 1)
        inputs = new double[network.inputNodes.Count]; // Preallocate for 13 inputs
        for (int i = 1; i < inputs.Length; i++)
        {
            inputs[i] = 0.0; // Initialise with default values
        }
        outputs = new double[network.outputNodes.Count]; // Preallocate for 13 inputs
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 0.0; // Initialise with default values
        }
    }

    // Update the agent's state (called by AgentManager every timestep)
    public void UpdateAgent(float deltaTime)
    {
        // Update vision
        UpdateVision();

        // Update age
        age = SimulationManager.instance.worldTime - birthTime;

        // Update energy and health
        UpdateEnergyAndHealth(deltaTime);

        // Update inputs
        UpdateInputs();

        // Process inputs through the neural network
        ProcessNetwork();

        // Execute outputs
        ExecuteOutputs(deltaTime);
    }

    // Update energy and health
    private void UpdateEnergyAndHealth(float deltaTime)
    {
        // Calculate metabolism cost
        float totalCost = metabolismCost + (movementCost * Math.Abs(movementSpeed));

        // Deduct energy
        energy -= totalCost * deltaTime;

        // Check for starvation
        if (energy <= 0)
        {
            health -= 10 * deltaTime; // Lose health if energy is depleted
            energy = 0;
        }

        // Check for death
        if (health <= 0)
        {
            Die();
        }
    }

    // Update detected objects within vision cone
    private void UpdateVision()
    {
        closestAgent = null;
        closestFood = null;

        closestAgentDistance = float.MaxValue;
        closestAgentAngle = -1;
        closestFoodDistance = float.MaxValue;
        closestFoodAngle = -1;

        Vector3 position = transform.position;

        // Detect objects within vision range
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, visionDistance, hitList, creatureLayer | foodLayer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitList[i];

            if (hit.gameObject == this.gameObject) continue; // Skip self-detection

            Vector3 directionToTarget = hit.transform.position - position;

            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            if (angleToTarget <= visionAngle / 2)
            {
                float distanceToTarget = directionToTarget.magnitude;

                // Detect other agents (creatures)
                Agent currentAgent = hit.GetComponent<Agent>();
                if (currentAgent != null && distanceToTarget < closestAgentDistance)
                {
                    closestAgent = currentAgent;
                    closestAgentDistance = distanceToTarget;
                    closestAgentAngle = angleToTarget;
                }

                // Detect food
                Food currentFood = hit.GetComponent<Food>();
                if (currentFood != null && distanceToTarget < closestFoodDistance)
                {
                    closestFood = currentFood;
                    closestFoodDistance = distanceToTarget;
                    closestFoodAngle = angleToTarget;
                }
            }
        }
    }

    // Update inputs for the neural network
    private void UpdateInputs()
    {
        // Update base inputs
        inputs[1] = health;           // Health
        inputs[2] = age;              // Age
        inputs[3] = maxEnergy - energy; // Hungriness
        inputs[4] = closestAgent != null ? 1 : 0; // See a creature?
        inputs[12] = desireValue;     // Desire value

        // Get data for the closest creature
        if (closestAgent != null)
        {
            inputs[5] = closestAgentDistance; // Distance to closest creature
            inputs[6] = closestAgentAngle; // Angle to closest creature
            inputs[7] = closestAgent.health; // Health of closest creature
            inputs[8] = closestAgent.energy; // Energy of closest creature
        }

        // Get data for the closest food
        if (closestFood != null)
        {
            inputs[9] = closestFood != null ? 1 : 0; //See a food?
            inputs[10] = closestFoodDistance; // Distance to closest food
            inputs[11] = closestFoodAngle; // Angle to closest food
        }

    }

    // Process inputs through the neural network
    private void ProcessNetwork()
    {

        // Feed inputs through the neural network
        outputs = network.FeedForward(inputs);

        // Map outputs to agent behavior
        movementSpeed = (float)outputs[0]; // Movement speed (-1 to 1)
        turningRate = (float)outputs[1];   // Turning rate (-1 to 1)
        desireValue = (float)outputs[2];   // Desire value

    }

    // Execute outputs (movement, turning, etc.)
    private void ExecuteOutputs(float deltaTime)
    {
        // Move the agent
        transform.Translate(Vector3.forward * movementSpeed * speed * deltaTime);

        // Turn the agent
        transform.Rotate(Vector3.up, turningRate * 100 * deltaTime);
    }

    // Handle agent death
    private void Die()
    {
        // Notify AgentManager or other systems
        AgentManager.instance.RemoveAgent(this);

        // Destroy the agent
        Destroy(gameObject);
    }

    // Debugging: Draw vision cone in the editor
    private void OnDrawGizmosSelected()
    {
        // Draw vision distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance);

        // Draw vision cone
        Vector3 leftDir = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * visionDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * visionDistance);
    }
}
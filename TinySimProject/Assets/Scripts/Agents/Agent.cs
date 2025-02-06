using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public int id;
    // Agent Characteristics (can mutate)
    public Vector3 position;
    public float size;                // Size of the agent
    public float speed;               // Base speed
    public float visionDistance = 10f; // How far the agent can see
    public float visionAngle = 90f;   // Field of view in degrees
    public Color colour;               // Agent's colour
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
    public LayerMask agentLayer = 8;   // Layer for agents
    public LayerMask foodLayer = 7;       // Layer for food

    // Closest objects
    private Agent closestAgent;
    private Food closestFood;
    float closestAgentDistance;
    float closestAgentAngle;
    float closestFoodDistance;
    float closestFoodAngle;
    private Collider2D[] hitList = new Collider2D[20];

    // Reproduction Variables
    public float reproductionCooldown;      // Time required between reproductions
    public float reproductionEnergyCost;    // Energy required to reproduce
    public float maxReproductionCooldown;   // Mutates to change reproductive strategy
    public float reproductionRange = 8.0f;  // Max distance to mate with another agent
    private void Start()
    {
        //Variable Calculations
        metabolismCost = 0.15f * speed * size;
        energy = maxEnergy;
        reproductionCooldown = maxReproductionCooldown;
        // Global mutation parameters from the SimulationManager
        float globalMutationChance = SimulationManager.instance.globalMutationChance;
        float globalMutationMagnitude = SimulationManager.instance.globalMutationMagnitude;

        // Final mutation values after applying the agent-specific modifier
        mutationChance = globalMutationChance * (1 + mutationChanceMod);
        mutationMagnitude = globalMutationMagnitude * (1 + mutationMagnitudeMod);

        // Preallocate inputs
        InitialiseNetworkVariables();


        // Set birth time
        birthTime = SimulationManager.instance.worldTime;

    }

    // Preallocate the input list
    private void InitialiseNetworkVariables()
    {

        inputs = new double[network.inputNodes.Count]; // Preallocate for 13 inputs
        inputs[0] = 1.0;              // Control input (always 1)
        for (int i = 1; i < network.inputNodes.Count; i++)
        {
            inputs[i] = 0.0; // Initialise with default values
        }
        outputs = new double[network.outputNodes.Count]; // Preallocate for 13 inputs
        for (int i = 0; i < network.outputNodes.Count; i++)
        {
            outputs[i] = 0.0; // Initialise with default values
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

        //Reproduction
        reproductionCooldown -= deltaTime;
        if (reproductionCooldown <= 0)
        {
            AttemptReproduction();
        }

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

        // Detect objects within vision range
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, visionDistance, hitList, agentLayer | foodLayer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitList[i];

            if (hit.gameObject == gameObject) continue; // Skip self-detection

            Vector3 directionToTarget = hit.transform.position - position;

            float angleToTarget = Vector3.Angle(transform.up, directionToTarget);
            if (angleToTarget <= visionAngle)
            {
                float distanceToTarget = directionToTarget.magnitude;
                Debug.Log(angleToTarget);
                // Detect other agents (creatures)
                Agent currentAgent = hit.gameObject.GetComponent<Agent>();
                if (currentAgent != null && distanceToTarget < closestAgentDistance)
                {
                    closestAgent = currentAgent;
                    closestAgentDistance = distanceToTarget;
                    closestAgentAngle = angleToTarget;
                    Debug.Log(distanceToTarget);
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

        position = transform.position;

    }

    private void AttemptReproduction()
    {
        if (closestAgent == null || closestAgentDistance > reproductionRange) return;
        if (!isFertile() || !closestAgent.isFertile()) return;

        // Ensure the older agent is the one initiating reproduction
        Agent parent1 = age >= closestAgent.age ? this : closestAgent;
        Agent parent2 = parent1 == this ? closestAgent : this;

        Vector3 offspringPosition = (parent1.position + parent2.position) / 2;

        // Modify offspring size & energy based on reproductive strategy
        float offspringSize = Mathf.Clamp((parent1.size + parent2.size) / 2 * (reproductionEnergyCost / 30f), 0.5f, 3f);
        float offspringEnergy = Mathf.Clamp((parent1.maxEnergy + parent2.maxEnergy) / 2 * (reproductionEnergyCost / 30f), 10f, 100f);

        // Create offspring
        ReproductionManager.Reproduce(parent1, parent2, offspringPosition);

        // Apply reproduction costs
        parent1.energy -= parent1.reproductionEnergyCost;
        parent2.energy -= parent2.reproductionEnergyCost;
        parent1.reproductionCooldown = maxReproductionCooldown;
        parent2.reproductionCooldown = maxReproductionCooldown;
    }
    private bool isFertile()
    {
        return reproductionCooldown <= 0 && energy > reproductionEnergyCost;
    }

    // Handle agent death
    private void Die()
    {
        // Notify AgentManager or other systems
        AgentManager.instance.AgentListRemove(this);

        // Destroy the agent
        Destroy(gameObject);
    }

    // Debugging: Draw vision cone in the editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, visionDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, 0, -visionAngle) * transform.up * visionDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, visionAngle) * transform.up * visionDistance;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }

}
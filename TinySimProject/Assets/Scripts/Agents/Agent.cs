using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public int id;
    // Agent Characteristics (can mutate)
    public Vector3 position;
    public float rotation;
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
    public float speedMax = 10f;
    public float turningRate;         // Turning rate (-1 to 1)
    public float turnRateMax = 10f;
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
    private CircleCollider2D col;
    //Visuals
    public int eyeNumber;


    // Reproduction Variables
    public float reproductionCooldown;      // Time required between reproductions
    public float reproductionEnergyCost;    // Energy required to reproduce
    public float maxReproductionCooldown;   // Mutates to change reproductive strategy
    public float reproductionRange;  // Max distance to mate with another agent
    public float reproductionMod = 5f;
    private void Start()
    {
        //Variable Calculations
        metabolismCost = 0.5f * speed * size;
        energy = maxEnergy;
        reproductionCooldown = maxReproductionCooldown;

        col = GetComponent<CircleCollider2D>();
        col.radius = size;
        // Global mutation parameters from the SimulationManager
        float globalMutationChance = SimulationManager.instance.globalMutationChance;
        float globalMutationMagnitude = SimulationManager.instance.globalMutationMagnitude;

        // Final mutation values after applying the agent-specific modifier
        mutationChance = globalMutationChance * (1 + mutationChanceMod);
        mutationMagnitude = globalMutationMagnitude * (1 + mutationMagnitudeMod);

        eyeNumber = ((int)visionDistance + 16) / 16 * 2;
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

        Eat();

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
        Vector3 transformRight = transform.right;
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitList[i];

            if (hit.gameObject == gameObject) continue; // Skip self-detection

            Vector3 directionToTarget = hit.transform.position - position;

            float angleToTarget = Vector3.Angle(transformRight, directionToTarget);
            if (angleToTarget <= visionAngle)
            {
                float distanceToTarget = directionToTarget.magnitude;
                // Detect other agents (creatures)
                Agent currentAgent = hit.gameObject.GetComponent<Agent>();
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
        movementSpeed = Mathf.Clamp(movementSpeed, 0, speedMax);
        float modulus = movementSpeed * speed * deltaTime * 0.5f;



        // Turn the agent
        turningRate = Mathf.Clamp(turningRate, -turnRateMax, turnRateMax);

        rotation += turningRate * deltaTime * 10f;
        rotation = Mathf.Repeat(rotation, 360);

        Vector3 movementVector = new Vector3(modulus * Mathf.Cos(rotation * Mathf.Deg2Rad), modulus * Mathf.Sin(rotation * Mathf.Deg2Rad), 0);
        //Wrapping Behaviour
        position.x = Mathf.Repeat(movementVector.x + transform.position.x, SimulationManager.instance.worldSize);
        position.y = Mathf.Repeat(movementVector.y + transform.position.y, SimulationManager.instance.worldSize);

        transform.position = position;

    }

    private void AttemptReproduction()
    {
        if (closestAgent == null || closestAgentDistance > reproductionRange) return;
        if (!isFertile() || !closestAgent.isFertile()) return;

        // Ensure the older agent is the one initiating reproduction
        Agent parent1 = age >= closestAgent.age ? this : closestAgent;
        Agent parent2 = parent1 == this ? closestAgent : this;

        Vector3 offspringPosition = (parent1.position + parent2.position) / 2;

        // Create offspring
        ReproductionManager.Reproduce(parent1, parent2, offspringPosition);

        // Apply reproduction costs
        parent1.energy -= parent1.reproductionEnergyCost;
        parent2.energy -= parent2.reproductionEnergyCost;
        parent1.reproductionCooldown = maxReproductionCooldown + UnityEngine.Random.Range(-reproductionMod, reproductionMod);
        parent2.reproductionCooldown = maxReproductionCooldown + UnityEngine.Random.Range(-reproductionMod, reproductionMod);
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

    private void Eat()
    {
        if (closestFood != null)
        {
            if (closestFoodDistance <= size)
            {
                energy = Mathf.Min(energy + closestFood.nutritionValue, maxEnergy);
                closestFood.gameObject.SetActive(false); // Hide the food before removal
                FoodSpawner.instance.FoodListRemove(closestFood);
                Destroy(closestFood.gameObject);
            }
        }
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
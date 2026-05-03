using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    // Neural network input indices
    private const int INPUT_BIAS = 0;
    private const int INPUT_HEALTH = 1;
    private const int INPUT_AGE = 2;
    private const int INPUT_HUNGER = 3;
    private const int INPUT_SEE_AGENT = 4;
    private const int INPUT_AGENT_PROXIMITY = 5;
    private const int INPUT_AGENT_ANGLE = 6;
    private const int INPUT_AGENT_HEALTH = 7;
    private const int INPUT_AGENT_ENERGY = 8;
    private const int INPUT_SEE_FOOD = 9;
    private const int INPUT_FOOD_PROXIMITY = 10;
    private const int INPUT_FOOD_ANGLE = 11;
    private const int INPUT_DESIRE = 12;
    private const int INPUT_SEE_POISON = 13;
    private const int INPUT_POISON_PROXIMITY = 14;
    private const int INPUT_POISON_ANGLE = 15;
    private const int INPUT_TEMPERATURE = 16;

    public int id;
    // Agent Characteristics (can mutate)
    public Vector3 position;           //Agent current position
    public float rotation;             //Agent current rotation
    public float size;                 // Size of the agent
    public float speed;                // Base speed
    public float visionDistance = 10f; // How far the agent can see
    public float visionAngle = 90f;    // Field of view in degrees
    public Color colour;               // Agent's colour
    public float mutationMagnitudeMod; //Mutation magnitude modifier
    public float mutationChanceMod;    //Mutation chance modifier
    public float mutationMagnitude;    //Mutation magnitude
    public float mutationChance;       //Mutation chance

    // Speciation
    public int speciesId = -1;        // Which species this agent belongs to

    // Agent State
    public int generation;            //Current agent generation
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

    // Inputs and outputs (preallocated list)
    public double[] inputs;
    public double[] outputs;

    // Outputs
    public float movementSpeed;       // Movement speed
    public float speedMax;
    public float turningRate;         // Turning rate
    public float turnRateMax;
    public float desireValue;         // Desire value (used for decision-making)

    // Environment Sensors
    public LayerMask agentLayer = 8;   // Sorting layer for agents
    public LayerMask foodLayer = 7;       // Sorting layer for food

    // Closest objects
    private Agent closestAgent;
    private Food closestFood;
    private Food closestPoison;
    float closestAgentDistance;
    float closestAgentAngle;
    float closestFoodDistance;
    float closestFoodAngle;
    float closestPoisonDistance;
    float closestPoisonAngle;
    private Collider2D[] hitList = new Collider2D[20];
    private CircleCollider2D col;



    // Reproduction Variables
    public float reproductionCooldown;      // Time required between reproductions
    public float reproductionEnergyCost;    // Energy required to reproduce
    public float maxReproductionCooldown;   // Reproduction cooldown max initially
    public float reproductionRange;         // Max distance to mate with another agent
    public float reproductionCooldownJitter = 5f;      //Reproduction cooldown modifier
    public int offspringCount = 0;          //No. of offspring agent has
    public float eatingRadius;              //Size of radius around the agent that it can eat from
    private void Start()
    {

        //Metabolism cost: speed, size, and brain complexity all contribute
        SimulationConfig cfg = SimulationManager.instance.config;
        speedMax = cfg.maxAgentSpeed;
        turnRateMax = cfg.maxTurnRate;
        int brainConnections = genome.connectionGenes.Count;
        metabolismCost = (cfg.metabolismSpeedFactor * speed) + (cfg.metabolismSizeFactor * size * size) + (cfg.metabolismBrainFactor * brainConnections);
        movementCost = cfg.movementCostFactor * size;

        reproductionCooldown = maxReproductionCooldown;

        //Collision variable calculation
        col = GetComponent<CircleCollider2D>();
        col.radius = size * cfg.collisionRadiusScale;
        eatingRadius = size + cfg.eatingRadiusPadding;

        // Register in component cache for fast vision lookups
        AgentComponentCache.RegisterAgent(col, this);

        // Global mutation parameters from config
        float globalMutationChance = cfg.globalMutationChance;
        float globalMutationMagnitude = cfg.globalMutationMagnitude;

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
        outputs = new double[network.outputNodes.Count]; // Preallocate for 3 outputs
        for (int i = 0; i < network.outputNodes.Count; i++)
        {
            outputs[i] = 0.0; // Initialise with default values
        }
    }

    // Cached config reference — set once per update cycle
    private SimulationConfig _cfg;

    // Update the agent's state (called by AgentManager every timestep)
    public void UpdateAgent(float deltaTime)
    {
        _cfg = SimulationManager.instance.config;

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
    }

    public void UpdateReproduction(float deltaTime)
    {
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
        SimulationConfig cfg = _cfg;

        // Calculate metabolism cost — spinning is expensive
        float turnCost = cfg.metabolismTurnFactor * Mathf.Abs(turningRate);
        float totalCost = metabolismCost + (movementCost * Mathf.Abs(movementSpeed)) + turnCost;

        // Deduct energy
        energy -= totalCost * deltaTime;

        if (age > cfg.agingOnsetAge)
        {
            float agingRate = (age - cfg.agingOnsetAge) * cfg.agingRateMultiplier;
            health -= agingRate * deltaTime;
        }

        // Check for starvation
        if (energy <= 0)
        {
            health -= cfg.starvationHealthDrainRate * deltaTime;
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
        closestPoison = null;

        closestAgentDistance = float.MaxValue;
        closestAgentAngle = 0;
        closestFoodDistance = float.MaxValue;
        closestFoodAngle = 0;
        closestPoisonDistance = float.MaxValue;
        closestPoisonAngle = 0;

        // Compute the agent's actual facing direction from its rotation angle
        float radians = rotation * Mathf.Deg2Rad;
        Vector2 facingDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        // Detect objects within vision range
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, visionDistance, hitList, agentLayer | foodLayer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitList[i];

            if (hit.gameObject == gameObject) continue; // Skip self-detection

            Vector2 directionToTarget = (Vector2)(hit.transform.position - position);
            float distanceToTarget = directionToTarget.magnitude;
            if (distanceToTarget < 0.001f) continue;

            Vector2 normDir = directionToTarget / distanceToTarget;

            // Unsigned angle for vision cone check
            float dot = Vector2.Dot(facingDirection, normDir);
            float unsignedAngle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;

            if (unsignedAngle <= visionAngle)
            {
                // Signed angle: positive = target is to the left, negative = to the right
                float cross = facingDirection.x * normDir.y - facingDirection.y * normDir.x;
                float signedAngle = unsignedAngle * Mathf.Sign(cross);

                // Detect other agents (creatures) — cached component lookup
                Agent currentAgent = AgentComponentCache.GetAgent(hit);
                if (currentAgent != null && currentAgent != this && distanceToTarget < closestAgentDistance)
                {
                    closestAgent = currentAgent;
                    closestAgentDistance = distanceToTarget;
                    closestAgentAngle = signedAngle;
                }

                // Detect food (separate tracking for normal food vs poison)
                Food currentFood = AgentComponentCache.GetFood(hit);
                if (currentFood != null)
                {
                    if (currentFood.isPoison && distanceToTarget < closestPoisonDistance)
                    {
                        closestPoison = currentFood;
                        closestPoisonDistance = distanceToTarget;
                        closestPoisonAngle = signedAngle;
                    }
                    else if (!currentFood.isPoison && distanceToTarget < closestFoodDistance)
                    {
                        closestFood = currentFood;
                        closestFoodDistance = distanceToTarget;
                        closestFoodAngle = signedAngle;
                    }
                }
            }
        }
    }

    // Update inputs for the neural network
    // All inputs are normalised to roughly [-1, 1] so the NN can learn equally from all
    private void UpdateInputs()
    {
        SimulationConfig cfg = _cfg;

        // Update base inputs (normalised)
        inputs[INPUT_HEALTH] = health / cfg.offspringHealthBase;
        inputs[INPUT_AGE] = Mathf.Clamp01(age / cfg.ageSaturationCap);
        inputs[INPUT_HUNGER] = (maxEnergy - energy) / maxEnergy;
        inputs[INPUT_SEE_AGENT] = closestAgent != null ? 1 : 0;
        inputs[INPUT_DESIRE] = desireValue;

        // Local temperature
        int tx = Mathf.Clamp(Mathf.FloorToInt(position.x), 0, cfg.worldSize - 1);
        int ty = Mathf.Clamp(Mathf.FloorToInt(position.y), 0, cfg.worldSize - 1);
        inputs[INPUT_TEMPERATURE] = FoodSpawner.instance.temperatureMap.GetTemperatureAt(tx, ty);

        // Reset sensor inputs to prevent stale data
        inputs[INPUT_AGENT_PROXIMITY] = 0;
        inputs[INPUT_AGENT_ANGLE] = 0;
        inputs[INPUT_AGENT_HEALTH] = 0;
        inputs[INPUT_AGENT_ENERGY] = 0;
        inputs[INPUT_SEE_FOOD] = 0;
        inputs[INPUT_FOOD_PROXIMITY] = 0;
        inputs[INPUT_FOOD_ANGLE] = 0;
        inputs[INPUT_SEE_POISON] = 0;
        inputs[INPUT_POISON_PROXIMITY] = 0;
        inputs[INPUT_POISON_ANGLE] = 0;

        // Closest creature
        if (closestAgent != null)
        {
            inputs[INPUT_AGENT_PROXIMITY] = 1.0 - (closestAgentDistance / visionDistance);
            inputs[INPUT_AGENT_ANGLE] = closestAgentAngle / visionAngle;
            inputs[INPUT_AGENT_HEALTH] = closestAgent.health / cfg.offspringHealthBase;
            inputs[INPUT_AGENT_ENERGY] = closestAgent.energy / closestAgent.maxEnergy;
        }

        // Closest food
        if (closestFood != null)
        {
            inputs[INPUT_SEE_FOOD] = 1;
            inputs[INPUT_FOOD_PROXIMITY] = 1.0 - (closestFoodDistance / visionDistance);
            inputs[INPUT_FOOD_ANGLE] = closestFoodAngle / visionAngle;
        }

        // Closest poison
        if (closestPoison != null)
        {
            inputs[INPUT_SEE_POISON] = 1;
            inputs[INPUT_POISON_PROXIMITY] = 1.0 - (closestPoisonDistance / visionDistance);
            inputs[INPUT_POISON_ANGLE] = closestPoisonAngle / visionAngle;
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
        // Guard against NaN from neural network
        if (float.IsNaN(movementSpeed)) movementSpeed = 0f;
        if (float.IsNaN(turningRate)) turningRate = 0f;

        // Move the agent
        movementSpeed = Mathf.Clamp(movementSpeed, 0, speedMax);
        SimulationConfig cfg = _cfg;
        float modulus = movementSpeed * speed * deltaTime * cfg.movementDampening;

        // Turn the agent
        turningRate = Mathf.Clamp(turningRate, -turnRateMax, turnRateMax);

        rotation += turningRate * deltaTime * cfg.turnRateScale;
        rotation = Mathf.Repeat(rotation, 360);

        Vector3 movementVector = new Vector3(modulus * Mathf.Cos(rotation * Mathf.Deg2Rad), modulus * Mathf.Sin(rotation * Mathf.Deg2Rad), 0);
        // Clamp to world bounds (no wrapping)
        float worldSize = cfg.worldSize;
        position.x = Mathf.Clamp(movementVector.x + transform.position.x, 0, worldSize);
        position.y = Mathf.Clamp(movementVector.y + transform.position.y, 0, worldSize);

        transform.position = position;

    }

    private void AttemptReproduction()
    {
        if (closestAgent == null || closestAgentDistance > reproductionRange) return;
        if (!isFertile() || !closestAgent.isFertile()) return;

        // Prefer same-species mating. Inter-species mating has only 10% chance.
        bool sameSpecies = (speciesId == closestAgent.speciesId);
        if (!sameSpecies && SimRandom.NextFloat() > 0.1f) return;

        // Random agent is parent1, as there is no fitness in RT-NEAT
        Agent parent1 = SimRandom.NextFloat() > 0.5f ? this : closestAgent;
        Agent parent2 = parent1 == this ? closestAgent : this;

        Vector3 offspringPosition = (parent1.position + parent2.position) / 2;

        // Create offspring
        ReproductionManager.Reproduce(parent1, parent2, offspringPosition);

        // Apply reproduction costs
        float repCost = _cfg.parentReproductionEnergyCost;
        parent1.energy -= repCost;
        parent2.energy -= repCost;
        parent1.reproductionCooldown = maxReproductionCooldown + SimRandom.Range(-reproductionCooldownJitter, reproductionCooldownJitter);
        parent2.reproductionCooldown = maxReproductionCooldown + SimRandom.Range(-reproductionCooldownJitter, reproductionCooldownJitter);
        parent1.offspringCount++;
        parent2.offspringCount++;
    }
    public float GetScaledReproductionCost()
    {
        // Reproduction gets more expensive the more offspring an agent has produced
        return reproductionEnergyCost * (1f + _cfg.reproductionCostScaling * offspringCount);
    }

    public bool isFertile()
    {
        return reproductionCooldown <= 0 && energy > GetScaledReproductionCost();
    }

    // Handle agent death
    private void Die()
    {
        AgentComponentCache.UnregisterAgent(col);
        SimEvents.AgentDied(this);
        Destroy(gameObject);
    }

    private void Eat()
    {
        // Eat normal food
        if (closestFood != null)
        {
            if (closestFoodDistance <= eatingRadius)
            {
                energy = Mathf.Min(energy + closestFood.nutritionValue * _cfg.foodEnergyMultiplier, maxEnergy);
                closestFood.gameObject.SetActive(false);
                SimEvents.FoodConsumed(closestFood);
                Destroy(closestFood.gameObject);
            }
        }

        // Eat poison (still auto-eat if within radius — agents must learn to avoid)
        if (closestPoison != null)
        {
            if (closestPoisonDistance <= eatingRadius)
            {
                energy -= closestPoison.nutritionValue * _cfg.poisonEnergyMultiplier;
                health -= closestPoison.nutritionValue * _cfg.poisonHealthMultiplier;
                closestPoison.gameObject.SetActive(false);
                SimEvents.FoodConsumed(closestPoison);
                Destroy(closestPoison.gameObject);
            }
        }
    }
}
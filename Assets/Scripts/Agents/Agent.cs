using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    // Neural network input indices
    private const int INPUT_BIAS = 0;
    private const int INPUT_HEALTH = 1;
    private const int INPUT_AGE = 2;
    private const int INPUT_HUNGER = 3;
    private const int INPUT_TEMPERATURE = 4;
    private const int INPUT_SEE_AGENT = 5;
    private const int INPUT_AGENT_PROXIMITY = 6;
    private const int INPUT_AGENT_ANGLE = 7;
    private const int INPUT_AGENT_HEALTH = 8;
    private const int INPUT_AGENT_ENERGY = 9;
    private const int INPUT_AGENT_SIZE = 10;
    private const int INPUT_SAME_SPECIES = 11;
    private const int INPUT_SEE_FOOD = 12;
    private const int INPUT_FOOD_PROXIMITY = 13;
    private const int INPUT_FOOD_ANGLE = 14;
    private const int INPUT_SEE_MEAT = 15;
    private const int INPUT_MEAT_PROXIMITY = 16;
    private const int INPUT_MEAT_ANGLE = 17;
    private const int INPUT_SEE_POISON = 18;
    private const int INPUT_POISON_PROXIMITY = 19;
    private const int INPUT_POISON_ANGLE = 20;
    private const int INPUT_DESIRE = 21;

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
    public float maxHealth;           // Maximum health (size * healthPerSize)
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
    private float attackIntent;       // Attack output (predation)

    // Predation
    public float attackDamage;

    // Diet
    public float dietPreference;         // 0 = herbivore, 1 = carnivore
    private float attackCooldown;

    // Attack visuals
    public float damageFlashTimer;

    // Environment Sensors
    public LayerMask agentLayer = 8;   // Sorting layer for agents
    public LayerMask foodLayer = 7;       // Sorting layer for food

    // Closest objects
    private Agent closestAgent;
    private Food closestFood;
    private Food closestPoison;
    private Food closestMeat;
    float closestAgentDistance;
    float closestAgentAngle;
    float closestFoodDistance;
    float closestFoodAngle;
    float closestPoisonDistance;
    float closestPoisonAngle;
    float closestMeatDistance;
    float closestMeatAngle;
    private Collider2D[] hitList = new Collider2D[20];
    private CircleCollider2D col;



    // Reproduction Variables
    public float reproductionCooldown;      // Time required between reproductions
    public float reproductionEnergyCost;    // Energy required to reproduce
    public float maxReproductionCooldown;   // Reproduction cooldown max initially
    public float reproductionCooldownJitter;      //Reproduction cooldown modifier
    public int offspringCount = 0;          //No. of offspring agent has
    public float interactionRadius;         // Base radius derived from size
    public float eatingRadius;              // Radius for consuming food (defaults to interactionRadius)
    public float reproductionRange;         // Radius for mating (defaults to interactionRadius)
    public float attackRange;               // Radius for attacking (defaults to interactionRadius)
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
        reproductionCooldownJitter = cfg.reproductionCooldownJitter;

        //Collision variable calculation
        col = GetComponent<CircleCollider2D>();
        col.radius = size * cfg.collisionRadiusScale;
        interactionRadius = size;
        eatingRadius = cfg.eatingRadius < 0 ? interactionRadius : interactionRadius + cfg.eatingRadius;
        reproductionRange = cfg.reproductionRange < 0 ? interactionRadius : interactionRadius + cfg.reproductionRange;
        attackRange = cfg.attackRange < 0 ? interactionRadius : interactionRadius + cfg.attackRange;

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

        // Decay attack visuals
        if (damageFlashTimer > 0f) damageFlashTimer -= deltaTime;

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
        Attack(deltaTime);
    }

    public void UpdateReproduction(float deltaTime, bool canReproduce)
    {
        if (reproductionCooldown > 0)
        {
            reproductionCooldown -= deltaTime;
            if (reproductionCooldown < 0)
                reproductionCooldown = 0;
        }

        if (canReproduce && reproductionCooldown <= 0)
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
        closestMeat = null;

        closestAgentDistance = float.MaxValue;
        closestAgentAngle = 0;
        closestFoodDistance = float.MaxValue;
        closestFoodAngle = 0;
        closestPoisonDistance = float.MaxValue;
        closestPoisonAngle = 0;
        closestMeatDistance = float.MaxValue;
        closestMeatAngle = 0;

        // Compute the agent's facing direction from its rotation angle
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

                // Detect food (separate tracking for plant, meat, poison)
                Food currentFood = AgentComponentCache.GetFood(hit);
                if (currentFood != null)
                {
                    if (currentFood is PoisonFood && distanceToTarget < closestPoisonDistance)
                    {
                        closestPoison = currentFood;
                        closestPoisonDistance = distanceToTarget;
                        closestPoisonAngle = signedAngle;
                    }
                    else if (currentFood is MeatFood && distanceToTarget < closestMeatDistance)
                    {
                        closestMeat = currentFood;
                        closestMeatDistance = distanceToTarget;
                        closestMeatAngle = signedAngle;
                    }
                    else if (distanceToTarget < closestFoodDistance)
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
        inputs[INPUT_HEALTH] = health / maxHealth;
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
        inputs[INPUT_AGENT_SIZE] = 0;
        inputs[INPUT_SEE_FOOD] = 0;
        inputs[INPUT_FOOD_PROXIMITY] = 0;
        inputs[INPUT_FOOD_ANGLE] = 0;
        inputs[INPUT_SEE_POISON] = 0;
        inputs[INPUT_POISON_PROXIMITY] = 0;
        inputs[INPUT_POISON_ANGLE] = 0;
        inputs[INPUT_SEE_MEAT] = 0;
        inputs[INPUT_MEAT_PROXIMITY] = 0;
        inputs[INPUT_MEAT_ANGLE] = 0;
        inputs[INPUT_SAME_SPECIES] = 0;

        // Closest creature
        if (closestAgent != null)
        {
            inputs[INPUT_AGENT_PROXIMITY] = 1.0 - (closestAgentDistance / visionDistance);
            inputs[INPUT_AGENT_ANGLE] = closestAgentAngle / visionAngle;
            inputs[INPUT_AGENT_HEALTH] = closestAgent.health / health;
            inputs[INPUT_AGENT_ENERGY] = closestAgent.energy / energy;
            inputs[INPUT_AGENT_SIZE] = closestAgent.size / size; // Relative size: >1 means bigger than self
            inputs[INPUT_SAME_SPECIES] = (speciesId == closestAgent.speciesId) ? 1 : 0;
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

        // Closest meat
        if (closestMeat != null)
        {
            inputs[INPUT_SEE_MEAT] = 1;
            inputs[INPUT_MEAT_PROXIMITY] = 1.0 - (closestMeatDistance / visionDistance);
            inputs[INPUT_MEAT_ANGLE] = closestMeatAngle / visionAngle;
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

        // Attack intent (only exists when predation is enabled)
        if (outputs.Length > 3)
            attackIntent = (float)outputs[3];

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
        FoodSpawner.instance.DropDeathFood(position, energy);
        SimEvents.AgentDied(this);
        Destroy(gameObject);
    }

    private Collider2D[] eatHitList = new Collider2D[10];

    private void Eat()
    {
        float plantEfficiency = 1f - dietPreference;
        float meatEfficiency = dietPreference;
        float threshold = _cfg.dietEfficiencyThreshold;

        int hitCount = Physics2D.OverlapCircleNonAlloc(position, eatingRadius, eatHitList, foodLayer);
        for (int i = 0; i < hitCount; i++)
        {
            Food food = AgentComponentCache.GetFood(eatHitList[i]);
            if (food == null) continue;

            if (food is PoisonFood)
            {
                ConsumeFood(food);
                energy -= food.nutritionValue * _cfg.poisonEnergyMultiplier;
                health -= food.nutritionValue * _cfg.poisonHealthMultiplier;
                return;
            }

            float efficiency = food is MeatFood ? meatEfficiency : plantEfficiency;
            if (efficiency < threshold) continue;

            ConsumeFood(food);
            energy = Mathf.Min(energy + food.nutritionValue * efficiency, maxEnergy);
            return;
        }
    }

    private void ConsumeFood(Food food)
    {
        food.gameObject.SetActive(false);
        SimEvents.FoodConsumed(food);
        Destroy(food.gameObject);
    }

    private void Attack(float deltaTime)
    {
        if (!_cfg.enablePredation) return;
        if (attackDamage <= 0f) return;

        attackCooldown -= deltaTime;
        if (attackCooldown > 0f) return;

        // Only attack if NN output is above threshold
        if (attackIntent <= 0f) return;

        if (closestAgent == null) return;
        if (closestAgentDistance > attackRange) return;

        // Deal damage scaled by intent strength
        float damage = attackDamage * attackIntent;
        closestAgent.health -= damage;

        energy -= _cfg.attackEnergyCostMultiplier * damage;

        // Victim flashes red
        closestAgent.damageFlashTimer = _cfg.damageFlashDuration;

        // Attacker lunges toward prey (instant small offset)
        Vector3 dirToPrey = (closestAgent.position - position).normalized;
        float lungeDistance = Mathf.Min(_cfg.attackDashStrength, closestAgentDistance * 0.5f);
        position += dirToPrey * lungeDistance;
        transform.position = position;

        attackCooldown = _cfg.attackCooldownDuration;
    }
}
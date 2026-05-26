using UnityEngine;

// Central config for all parameters in the simulation
[CreateAssetMenu(fileName = "SimulationConfig", menuName = "TinySim/Simulation Config")]
public class SimulationConfig : ScriptableObject
{
    [Header("World")]
    public int worldSize = 128;
    public int seed = -1;

    [Header("Temperature")]
    public float temperatureScale = 3f;       // Perlin noise scale (lower = smoother, larger biomes)
    public float coldSkewPower = 1.5f;        // Power applied to raw noise (higher = more cold area)

    [Header("Spawn")]
    public int spawnPattern = 1;              // 0=Central, 1=Clusters, 2=Random
    public bool uniformStart = false;         // All agents start identical
    public int numberOfClusters = 5;          // Cluster spawn pattern: number of cluster centers

    [Header("Population")]
    public int maxPopulation = 200;
    public int initialAgentCount = 50;

    [Header("Agent Output Limits")]
    public float maxAgentSpeed = 10f;
    public float maxTurnRate = 10f;

    [Header("Metabolism & Energy")]
    public float metabolismSpeedFactor = 0.02f;
    public float metabolismSizeFactor = 0.05f;   // Applied as sizeFactor * size²
    public float metabolismBrainFactor = 0.005f;
    public float movementCostFactor = 0.01f;
    public float metabolismTurnFactor = 0.03f;
    public float movementDampening = 0.5f;
    public float turnRateScale = 10f;
    public float maxEnergyPerSize = 200f;

    [Header("Health & Aging")]
    public float healthPerSize = 50f;            // Max health = size * healthPerSize
    public float agingOnsetAge = 2000f;
    public float agingRateMultiplier = 0.0005f;
    public float starvationHealthDrainRate = 10f;
    public float ageSaturationCap = 500f;

    [Header("Interaction Radii")]
    public float collisionRadiusScale = 1.5f;
    public float eatingRadius = -1f;       // -1 = use interactionRadius
    public float reproductionRange = -1f;  // -1 = use interactionRadius
    public float attackRange = -1f;        // -1 = use interactionRadius

    [Header("Reproduction")]
    public float offspringEnergyMultiplier = 2.5f;
    public float parentReproductionEnergyCost = 10f;
    public float reproductionCostScaling = 0.1f; // Per-offspring cost increase
    public float reproductionCooldownJitter = 5f; // +/- random time added to cooldown after mating

    [Header("Mutation")]
    public float globalMutationChance = 0.1f;
    public float globalMutationMagnitude = 0.4f;
    public float disableVsPruneChance = 0.5f; // Probability of disabling a connection vs pruning a hidden node

    [Header("Mutation Type Thresholds")]
    [Tooltip("0-50%: weight mutation or add connection")]
    public float weightOrConnectionThreshold = 0.50f;
    [Tooltip("50-75%: disable/remove connection (25% chance)")]
    public float disableConnectionThreshold = 0.75f;
    [Tooltip("75-90%: change activation function (15% chance)")]
    public float changeActivationThreshold = 0.90f;
    // 90-100%: add node (10% chance)
    public int mutationsPerMagnitude = 4;
    public float weightMutationFraction = 0.57f;
    public float weightPerConnectionMutationChance = 0.25f;
    public float weightPerturbationScale = 0.2f;

    [Header("Attribute Mutation Clamps")]
    public float minSize = 0.3f;
    public float maxSize = 4f;
    public float minSpeed = 0.1f;
    public float maxSpeed = 40f;
    public float minVisionDistance = 0.1f;
    public float maxVisionDistance = 10f;
    public float minVisionAngle = 0f;
    public float maxVisionAngle = 360f;
    public float minMutationChanceMod = 0.01f;
    public float maxMutationChanceMod = 1f;
    public float minMutationMagnitudeMod = 0.01f;
    public float maxMutationMagnitudeMod = 10f;
    public float minReproductionCooldown = 1f;
    public float maxReproductionCooldown = 400f;
    public float minReproductionEnergyCost = 20f;
    public float maxReproductionEnergyCost = 100f;
    public float minAttackDamage = 0f;
    public float maxAttackDamage = 20f;

    [Header("Uniform Start Base Values")]
    public float baseSize = 1.0f;
    public float baseSpeed = 2.0f;
    public float baseVisionDistance = 10f;
    public float baseVisionAngle = 90f;
    public float baseMaxEnergy = 200f;
    public float baseMutationChanceMod = 1f;
    public float baseMutationMagnitudeMod = 1f;
    public float baseMaxReproductionCooldown = 10f;
    public float baseReproductionEnergyCost = 20f;
    public float baseDietPreference = 0.5f;
    public float baseAttackDamage = 0f;

    [Header("Initial Agent Ranges")]
    public float initialSizeMin = 0.5f;
    public float initialSizeMax = 2.0f;
    public float initialSpeedMin = 1.0f;
    public float initialSpeedMax = 4.0f;
    public float initialVisionDistanceMin = 5f;
    public float initialVisionDistanceMax = 15f;
    public float initialVisionAngleMin = 45f;
    public float initialVisionAngleMax = 180f;
    public float initialMutationChanceModMin = 0.5f;
    public float initialMutationChanceModMax = 2.0f;
    public float initialMutationMagnitudeModMin = 0.5f;
    public float initialMutationMagnitudeModMax = 2.0f;
    public float initialMaxRepCooldownMin = 5f;
    public float initialMaxRepCooldownMax = 20f;
    public float initialRepEnergyCostMin = 10f;
    public float initialRepEnergyCostMax = 40f;
    public float initialMaxEnergyMin = 100f;
    public float initialMaxEnergyMax = 300f;
    public float initialDietPreferenceMin = 0.0f;
    public float initialDietPreferenceMax = 1.0f;
    public float initialAttackDamageMin = 0f;
    public float initialAttackDamageMax = 5f;

    [Header("Diet")]
    public float dietEfficiencyThreshold = 0.2f; // Won't eat food type if efficiency is below this

    [Header("Food")]
    public int maxFoodCount = 500;
    public int initialFoodCount = 100;
    public float foodSpawnInterval = 1f;
    public int foodSpawnBatchSize = 5;
    public int maxSpawnBatchesPerTick = 20;
    public float foodNutritionMin = 10f;
    public float foodNutritionMax = 40f;
    public float coldNutritionMultiplier = 0.5f; // Nutrition floor multiplier in coldest biomes
    public float foodDespawnMin = 800f;
    public float foodDespawnMax = 1200f;
    public float foodSizePerNutrition = 0.0017f;
    public float foodSpawnChanceCold = 0.1f;
    public float foodSpawnChanceWarm = 0.9f;

    [Header("Poison")]
    public float poisonEnergyMultiplier = 4f;
    public float poisonHealthMultiplier = 2f;
    public float poisonTemperatureThreshold = 0.3f;
    public float poisonSpawnChance = 0.5f;
    public float poisonSizeMultiplier = 1.3f;

    [Header("Death / Meat")]
    public int deathFoodDropCount = 1;           // Number of meat items dropped on death
    public float deathFoodNutritionPerEnergy = 5f;
    public float deathFoodScatter = 1.5f;        // Scatter radius for dropped meat
    public float meatDespawnMultiplier = 0.4f;   // Meat decays faster (multiplied against normal despawn)
    public Color meatColour = new Color(0.7f, 0.15f, 0.1f, 1f); // Dark red

    [Header("Predation")]
    public bool enablePredation = false;
    public float attackCooldownDuration = 1.0f;
    public float attackEnergyCostMultiplier = 5f;
    public float attackDashStrength = 0.3f; // Lunge distance toward prey on attack
    public float damageFlashDuration = 0.15f; // Seconds of red flash when hit

    [Header("Speciation")]
    public float excessCoefficient = 1.0f;
    public float disjointCoefficient = 1.0f;
    public float weightDiffCoefficient = 0.4f;
    public float attributeDistanceCoefficient = 2.0f; // Weight for physical trait differences in speciation
    public float compatibilityThreshold = 3.0f;
    public float interSpeciesMatingChance = 0.1f;   // Chance of mating with a different species
    public int speciationNormalisationThreshold = 20;
    public int speciesViabilityThreshold = 5;
    public float anagenesisThreshold = 6.0f;
    public float anagenesisCheckInterval = 100f;
    public float anagenesisMinAge = 200f;
}

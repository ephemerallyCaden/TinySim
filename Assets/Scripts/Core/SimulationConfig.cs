using UnityEngine;

/// <summary>
/// Central configuration for all simulation parameters.
/// Create an instance via Assets > Create > TinySim > Simulation Config.
/// Set any clamp value to -1 to disable that limit.
/// </summary>
[CreateAssetMenu(fileName = "SimulationConfig", menuName = "TinySim/Simulation Config")]
public class SimulationConfig : ScriptableObject
{
    /// <summary>
    /// Clamps a value using config min/max. A value of -1 means "no limit" for that bound.
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        float effectiveMin = (min == -1f) ? float.MinValue : min;
        float effectiveMax = (max == -1f) ? float.MaxValue : max;
        return Mathf.Clamp(value, effectiveMin, effectiveMax);
    }

    [Header("World")]
    public int worldSize = 128;
    public int seed = -1;

    [Header("Population")]
    public int maxPopulation = 200;
    public int initialAgentCount = 50;

    [Header("Metabolism")]
    public float metabolismSpeedFactor = 0.02f;
    public float metabolismSizeFactor = 0.05f;   // Applied as sizeFactor * size²
    public float metabolismBrainFactor = 0.005f;
    public float movementCostFactor = 0.01f;
    public float metabolismTurnFactor = 0.03f;
    public float movementDampening = 0.5f;
    public float turnRateScale = 10f;

    [Header("Health & Aging")]
    public float agingOnsetAge = 2000f;
    public float agingRateMultiplier = 0.002f;
    public float starvationHealthDrainRate = 10f;
    public float ageSaturationCap = 500f;
    public float collisionRadiusScale = 1.5f;
    public float eatingRadiusPadding = 0.2f;

    [Header("Agent Output Limits")]
    public float maxAgentSpeed = 10f;
    public float maxTurnRate = 10f;

    [Header("Reproduction")]
    public float reproductionRange = 8.0f;
    public float offspringHealthBase = 100f;
    public float offspringEnergyMultiplier = 2.5f;
    public float maxEnergyPerSize = 200f;
    public float parentReproductionEnergyCost = 10f;
    public float reproductionCostScaling = 0.1f; // Per-offspring cost increase

    [Header("Mutation")]
    public float globalMutationChance = 0.1f;
    public float globalMutationMagnitude = 0.4f;

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
    public float minReproductionRange = 1f;
    public float maxReproductionRange = 50f;

    [Header("Initial Agent Ranges (Random Start)")]
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
    public float initialRepRangeMin = 10f;
    public float initialRepRangeMax = 30f;

    [Header("Food")]
    public int maxFoodCount = 500;
    public int initialFoodCount = 100;
    public float foodSpawnInterval = 1f;
    public int foodSpawnBatchSize = 5;
    public int maxSpawnBatchesPerTick = 20;
    public float foodEnergyMultiplier = 3f;
    public float poisonEnergyMultiplier = 4f;
    public float poisonHealthMultiplier = 2f;
    public float poisonTemperatureThreshold = 0.3f;
    public float poisonSpawnChance = 0.5f;
    public float foodNutritionMin = 10f;
    public float foodNutritionMax = 40f;
    public float foodDespawnMin = 800f;
    public float foodDespawnMax = 1200f;
    public float foodSizePerNutrition = 0.01f;
    public float poisonSizeMultiplier = 1.3f;
    public float foodSpawnChanceCold = 0.1f;
    public float foodSpawnChanceWarm = 0.9f;

    [Header("Speciation")]
    public float excessCoefficient = 1.0f;
    public float disjointCoefficient = 1.0f;
    public float weightDiffCoefficient = 0.4f;
    public float compatibilityThreshold = 3.0f;
    public int speciationNormalisationThreshold = 20;
    public int speciesViabilityThreshold = 5;
    public float anagenesisThreshold = 6.0f;
    public float anagenesisCheckInterval = 100f;
    public float anagenesisMinAge = 200f;
}

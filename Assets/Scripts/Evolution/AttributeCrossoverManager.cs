using UnityEngine;

public static class AttributeCrossoverManager
{
    // Randomly select each attribute from one parent (instead of averaging)
    public static AgentAttributes CrossoverAttributes(Agent parent1, Agent parent2)
    {
        AgentAttributes attrs;
        attrs.size = SimRandom.NextFloat() < 0.5f ? parent1.size : parent2.size;
        attrs.speed = SimRandom.NextFloat() < 0.5f ? parent1.speed : parent2.speed;
        attrs.colour = SimRandom.NextFloat() < 0.5f ? parent1.colour : parent2.colour;
        attrs.visionDistance = SimRandom.NextFloat() < 0.5f ? parent1.visionDistance : parent2.visionDistance;
        attrs.visionAngle = SimRandom.NextFloat() < 0.5f ? parent1.visionAngle : parent2.visionAngle;
        attrs.mutationChanceMod = SimRandom.NextFloat() < 0.5f ? parent1.mutationChanceMod : parent2.mutationChanceMod;
        attrs.mutationMagnitudeMod = SimRandom.NextFloat() < 0.5f ? parent1.mutationMagnitudeMod : parent2.mutationMagnitudeMod;
        attrs.maxReproductionCooldown = SimRandom.NextFloat() < 0.5f ? parent1.maxReproductionCooldown : parent2.maxReproductionCooldown;
        attrs.reproductionEnergyCost = SimRandom.NextFloat() < 0.5f ? parent1.reproductionEnergyCost : parent2.reproductionEnergyCost;
        attrs.reproductionRange = SimRandom.NextFloat() < 0.5f ? parent1.reproductionRange : parent2.reproductionRange;
        return attrs;
    }

    // Mutate attributes in-place
    public static void MutateAttributes(ref AgentAttributes attrs, float mutationChance, float mutationMagnitude)
    {
        SimulationConfig cfg = SimulationManager.instance.config;

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.size += (SimRandom.NextFloat() - 0.5f) * mutationMagnitude;
            attrs.size = SimulationConfig.Clamp(attrs.size, cfg.minSize, cfg.maxSize);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.speed += (SimRandom.NextFloat() - 0.5f) * mutationMagnitude * 2;
            attrs.speed = SimulationConfig.Clamp(attrs.speed, cfg.minSpeed, cfg.maxSpeed);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.colour.r += (SimRandom.NextFloat() - 0.5f) * 0.2f * mutationMagnitude;
            attrs.colour.g += (SimRandom.NextFloat() - 0.5f) * 0.2f * mutationMagnitude;
            attrs.colour.b += (SimRandom.NextFloat() - 0.5f) * 0.2f * mutationMagnitude;
            attrs.colour.r = Mathf.Clamp01(attrs.colour.r);
            attrs.colour.g = Mathf.Clamp01(attrs.colour.g);
            attrs.colour.b = Mathf.Clamp01(attrs.colour.b);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.visionDistance += (SimRandom.NextFloat() - 0.5f) * mutationMagnitude;
            attrs.visionDistance = SimulationConfig.Clamp(attrs.visionDistance, cfg.minVisionDistance, cfg.maxVisionDistance);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.visionAngle += (SimRandom.NextFloat() - 0.5f) * mutationMagnitude;
            attrs.visionAngle = SimulationConfig.Clamp(attrs.visionAngle, cfg.minVisionAngle, cfg.maxVisionAngle);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.mutationChanceMod += (SimRandom.NextFloat() - 0.5f) * mutationMagnitude;
            attrs.mutationChanceMod = SimulationConfig.Clamp(attrs.mutationChanceMod, cfg.minMutationChanceMod, cfg.maxMutationChanceMod);

            attrs.mutationMagnitudeMod += (SimRandom.NextFloat() - 0.5f) * mutationMagnitude;
            attrs.mutationMagnitudeMod = SimulationConfig.Clamp(attrs.mutationMagnitudeMod, cfg.minMutationMagnitudeMod, cfg.maxMutationMagnitudeMod);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.maxReproductionCooldown += (SimRandom.NextFloat() - 0.5f) * mutationMagnitude;
            attrs.maxReproductionCooldown = SimulationConfig.Clamp(attrs.maxReproductionCooldown, cfg.minReproductionCooldown, cfg.maxReproductionCooldown);

            attrs.reproductionEnergyCost += (SimRandom.NextFloat() - 0.5f) * mutationMagnitude;
            attrs.reproductionEnergyCost = SimulationConfig.Clamp(attrs.reproductionEnergyCost, cfg.minReproductionEnergyCost, cfg.maxReproductionEnergyCost);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.reproductionRange += (SimRandom.NextFloat() - 0.5f) * mutationMagnitude * 2;
            attrs.reproductionRange = SimulationConfig.Clamp(attrs.reproductionRange, cfg.minReproductionRange, cfg.maxReproductionRange);
        }
    }
}

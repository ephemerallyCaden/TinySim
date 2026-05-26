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
        attrs.attackDamage = SimRandom.NextFloat() < 0.5f ? parent1.attackDamage : parent2.attackDamage;
        attrs.dietPreference = SimRandom.NextFloat() < 0.5f ? parent1.dietPreference : parent2.dietPreference;
        return attrs;
    }

    // Mutate attributes in-place using Gaussian distribution.
    // Most mutations are small; rare outliers allow large jumps.
    public static void MutateAttributes(ref AgentAttributes attrs, float mutationChance, float mutationMagnitude)
    {
        SimulationConfig cfg = SimulationManager.instance.config;

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.size += SimRandom.Gaussian() * mutationMagnitude * 0.5f;
            attrs.size = SimMath.Clamp(attrs.size, cfg.minSize, cfg.maxSize);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.speed += SimRandom.Gaussian() * mutationMagnitude;
            attrs.speed = SimMath.Clamp(attrs.speed, cfg.minSpeed, cfg.maxSpeed);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.colour.r += SimRandom.Gaussian() * 0.1f * mutationMagnitude;
            attrs.colour.g += SimRandom.Gaussian() * 0.1f * mutationMagnitude;
            attrs.colour.b += SimRandom.Gaussian() * 0.1f * mutationMagnitude;
            attrs.colour.r = Mathf.Clamp01(attrs.colour.r);
            attrs.colour.g = Mathf.Clamp01(attrs.colour.g);
            attrs.colour.b = Mathf.Clamp01(attrs.colour.b);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.visionDistance += SimRandom.Gaussian() * mutationMagnitude * 0.5f;
            attrs.visionDistance = SimMath.Clamp(attrs.visionDistance, cfg.minVisionDistance, cfg.maxVisionDistance);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.visionAngle += SimRandom.Gaussian() * mutationMagnitude * 5f;
            attrs.visionAngle = SimMath.Clamp(attrs.visionAngle, cfg.minVisionAngle, cfg.maxVisionAngle);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.mutationChanceMod += SimRandom.Gaussian() * mutationMagnitude * 0.5f;
            attrs.mutationChanceMod = SimMath.Clamp(attrs.mutationChanceMod, cfg.minMutationChanceMod, cfg.maxMutationChanceMod);

            attrs.mutationMagnitudeMod += SimRandom.Gaussian() * mutationMagnitude * 0.5f;
            attrs.mutationMagnitudeMod = SimMath.Clamp(attrs.mutationMagnitudeMod, cfg.minMutationMagnitudeMod, cfg.maxMutationMagnitudeMod);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.maxReproductionCooldown += SimRandom.Gaussian() * mutationMagnitude * 2f;
            attrs.maxReproductionCooldown = SimMath.Clamp(attrs.maxReproductionCooldown, cfg.minReproductionCooldown, cfg.maxReproductionCooldown);

            attrs.reproductionEnergyCost += SimRandom.Gaussian() * mutationMagnitude * 2f;
            attrs.reproductionEnergyCost = SimMath.Clamp(attrs.reproductionEnergyCost, cfg.minReproductionEnergyCost, cfg.maxReproductionEnergyCost);
        }

        if (cfg.enablePredation && SimRandom.NextFloat() < mutationChance)
        {
            attrs.attackDamage += SimRandom.Gaussian() * mutationMagnitude;
            attrs.attackDamage = SimMath.Clamp(attrs.attackDamage, cfg.minAttackDamage, cfg.maxAttackDamage);
        }

        if (SimRandom.NextFloat() < mutationChance)
        {
            attrs.dietPreference += SimRandom.Gaussian() * mutationMagnitude;
            attrs.dietPreference = SimMath.Clamp(attrs.dietPreference, 0f, 1f);
        }
    }
}

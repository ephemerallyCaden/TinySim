using UnityEngine;

public class MeatFood : Food
{
    protected override void Start()
    {
        // Ensure nutritionValue is never recalculated from biome —
        // it's always set by DropDeathFood before Start runs.
        if (nutritionValue <= 0f)
            nutritionValue = 1f; // Fallback: minimal nutrition rather than biome calc

        base.Start();
        SimulationConfig cfg = SimulationManager.instance.config;

        // Meat decays faster than plant food
        despawnTime *= cfg.meatDespawnMultiplier;
        timer = despawnTime;

        // Distinct red colour
        colour = cfg.meatColour;
    }
}

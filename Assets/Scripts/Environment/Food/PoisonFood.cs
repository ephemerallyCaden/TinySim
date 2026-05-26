using UnityEngine;

public class PoisonFood : Food
{
    protected override void Start()
    {
        base.Start();
        SimulationConfig cfg = SimulationManager.instance.config;

        colour = new Color(0.8f, 0.0f, 0.9f, 1);
        size *= cfg.poisonSizeMultiplier;
        col.radius = size;
    }
}

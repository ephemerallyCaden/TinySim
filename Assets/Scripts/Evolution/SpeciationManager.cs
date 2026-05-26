using System.Collections.Generic;
using UnityEngine;

// NEAT-style speciation. Groups agents into species based on genome compatibility distance
//  δ = (c₁·E / N) + (c₂·D / N) + (c₃·W̄) (Stanley K.O., Risto M., 2002).
public class SpeciationManager : MonoBehaviour
{
    public static SpeciationManager instance;

    [Header("Speciation Parameters")]
    public float excessCoefficient = 1.0f;       // c1: weight for excess genes
    public float disjointCoefficient = 1.0f;     // c2: weight for disjoint genes
    public float weightDiffCoefficient = 0.4f;   // c3: weight for average weight difference
    public float compatibilityThreshold = 3.0f;  // delta_t: threshold for same species

    [Header("Anagenesis (Drift Detection)")]
    public float anagenesisThreshold = 6.0f;    // Distance from founding genome to trigger evolution
    public float anagenesisCheckInterval = 100f; // World time between drift checks
    public float anagenesisMinAge = 200f;        // Species must be this old before drift is checked

    public List<Species> species = new List<Species>();
    private int nextSpeciesId = 0;
    private float nextAnagenesisCheck = 0f;

    // Reusable dictionaries for CompatibilityDistance — avoids per-call allocation
    private readonly Dictionary<int, ConnectionGene> _g1Cache = new Dictionary<int, ConnectionGene>();
    private readonly Dictionary<int, ConnectionGene> _g2Cache = new Dictionary<int, ConnectionGene>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        // Wire speciation parameters from config
        SimulationConfig cfg = SimulationManager.instance.config;
        excessCoefficient = cfg.excessCoefficient;
        disjointCoefficient = cfg.disjointCoefficient;
        weightDiffCoefficient = cfg.weightDiffCoefficient;
        compatibilityThreshold = cfg.compatibilityThreshold;
        anagenesisThreshold = cfg.anagenesisThreshold;
        anagenesisCheckInterval = cfg.anagenesisCheckInterval;
        anagenesisMinAge = cfg.anagenesisMinAge;
        Species.viabilityThreshold = cfg.speciesViabilityThreshold;
    }

    private void OnEnable()
    {
        SimEvents.OnAgentDied += HandleAgentDied;
    }

    private void OnDisable()
    {
        SimEvents.OnAgentDied -= HandleAgentDied;
    }

    private void Update()
    {
        float worldTime = SimulationManager.instance.worldTime;

        // Check for extinct species every frame (after deferred queues are processed)
        CheckForExtinctions();

        // Periodic anagenesis check
        if (worldTime >= nextAnagenesisCheck)
        {
            nextAnagenesisCheck = worldTime + anagenesisCheckInterval;
            CheckAnagenesis(worldTime);
        }
    }

    // NEAT compatibility distance between two genomes.
    public float CompatibilityDistance(Genome g1, Genome g2)
    {
        int excess = 0;
        int disjoint = 0;
        float weightDiffSum = 0f;
        int matchingCount = 0;

        // Reuse cached dictionaries — cleared each call, no allocation
        _g1Cache.Clear();
        _g2Cache.Clear();
        var g1Connections = _g1Cache;
        var g2Connections = _g2Cache;

        int maxInnovation1 = 0;
        int maxInnovation2 = 0;

        foreach (var c in g1.connectionGenes)
        {
            g1Connections[c.linkid.id] = c;
            if (c.linkid.id > maxInnovation1) maxInnovation1 = c.linkid.id;
        }

        foreach (var c in g2.connectionGenes)
        {
            g2Connections[c.linkid.id] = c;
            if (c.linkid.id > maxInnovation2) maxInnovation2 = c.linkid.id;
        }

        int maxInnovation = Mathf.Max(maxInnovation1, maxInnovation2);

        for (int i = 0; i <= maxInnovation; i++)
        {
            bool in1 = g1Connections.ContainsKey(i);
            bool in2 = g2Connections.ContainsKey(i);

            if (in1 && in2)
            {
                // Matching gene — accumulate weight difference
                weightDiffSum += Mathf.Abs((float)(g1Connections[i].weight - g2Connections[i].weight));
                matchingCount++;
            }
            else if (in1 || in2)
            {
                // Gene in one but not the other
                if (i > maxInnovation1 || i > maxInnovation2)
                    excess++;
                else
                    disjoint++;
            }
        }

        // Normalise by larger genome size (N), with minimum of 1 to avoid division by zero
        int N = Mathf.Max(g1.connectionGenes.Count, g2.connectionGenes.Count);
        int normThreshold = SimulationManager.instance.config.speciationNormalisationThreshold;
        if (N < normThreshold) N = 1; // Small genomes don't need normalisation

        float avgWeightDiff = matchingCount > 0 ? weightDiffSum / matchingCount : 0f;

        return (excessCoefficient * excess / N)
            + (disjointCoefficient * disjoint / N)
            + (weightDiffCoefficient * avgWeightDiff);
    }

    // Single species genetic drift
    private void CheckAnagenesis(float worldTime)
    {
        List<Species> toEvolve = new List<Species>();

        foreach (var s in species)
        {
            if (s.members.Count < Species.viabilityThreshold) continue;
            if (!s.isViable) continue;
            if (worldTime - s.birthTime < anagenesisMinAge) continue;
            if (s.representative == null || s.foundingGenome == null) continue;

            float drift = CompatibilityDistance(s.representative.genome, s.foundingGenome);
            if (drift >= anagenesisThreshold)
            {
                toEvolve.Add(s);
            }
        }

        foreach (var oldSpecies in toEvolve)
        {
            EvolveSpecies(oldSpecies);
        }
    }

    private void EvolveSpecies(Species oldSpecies)
    {
        // Create successor species with all members transferred
        int newId = nextSpeciesId++;
        int driftGen = oldSpecies.driftGeneration + 1;
        string newName = SpeciesNamer.GenerateSuccessorName(newId, oldSpecies.speciesName);

        Species newSpecies = new Species(newId, oldSpecies.representative, oldSpecies.id, SimulationManager.instance.worldTime);
        newSpecies.speciesName = newName;
        newSpecies.driftGeneration = driftGen;
        newSpecies.isViable = true; // Already proven viable

        // Transfer all members to new species
        foreach (var agent in oldSpecies.members)
        {
            agent.speciesId = newId;
        }
        newSpecies.members = new List<Agent>(oldSpecies.members);
        oldSpecies.members.Clear();

        species.Add(newSpecies);

        // Fire events: evolved + new species created (extinction handled by CheckForExtinctions)
        SimEvents.SpeciesEvolved(oldSpecies.id, newId);
        SimEvents.SpeciesCreated(newId, oldSpecies.id);
    }

    private void HandleAgentDied(Agent agent)
    {
        RemoveFromSpecies(agent);
    }

    // Assign an agent to an existing species or create a new one.
    // parentSpeciesId is the species of the parent that produced this agent (-1 default).
    public void AssignSpecies(Agent agent, int parentSpeciesId = -1)
    {
        SimulationConfig cfg = SimulationManager.instance.config;

        foreach (var s in species)
        {
            if (s.members.Count == 0) continue;

            // Compare genome topology + physical attributes
            float distance = CompatibilityDistance(agent.genome, s.representative.genome);
            distance += AttributeDistance(agent, s.representative, cfg);
            if (distance < compatibilityThreshold)
            {
                s.AddMember(agent);
                agent.speciesId = s.id;
                return;
            }
        }

        // No compatible species found
        int newId = nextSpeciesId++;
        Species newSpecies = new Species(newId, agent, parentSpeciesId, SimulationManager.instance.worldTime);
        species.Add(newSpecies);
        agent.speciesId = newSpecies.id;
    }

    // Attribute-based distance: agents with different physical traits speciate more readily.
    // Each term is normalised to [0,1], then weighted by the coefficient.
    private float AttributeDistance(Agent a, Agent b, SimulationConfig cfg)
    {
        float sizeRange = cfg.maxSize - cfg.minSize;
        float sizeDiff = Mathf.Abs(a.size - b.size) / sizeRange;

        float dietDiff = Mathf.Abs(a.dietPreference - b.dietPreference);

        float attackRange = cfg.maxAttackDamage - cfg.minAttackDamage;
        float attackDiff = attackRange > 0 ? Mathf.Abs(a.attackDamage - b.attackDamage) / attackRange : 0f;

        return cfg.attributeDistanceCoefficient * (sizeDiff + dietDiff + attackDiff);
    }

    // Remove agent from a species (called on death)
    public void RemoveFromSpecies(Agent agent)
    {
        foreach (var s in species)
        {
            if (s.id == agent.speciesId)
            {
                s.RemoveMember(agent);
                break;
            }
        }
    }

    /// Check for extinct species. Called periodically to avoid false positives
    public void CheckForExtinctions()
    {
        for (int i = species.Count - 1; i >= 0; i--)
        {
            if (species[i].members.Count == 0)
            {
                if (species[i].isViable)
                {
                    SimEvents.SpeciesExtinct(species[i].id);
                }
                species.RemoveAt(i);
            }
        }
    }
}

// A species is a group of genetically similar agents.
public class Species
{
    public int id;
    public int parentSpeciesId;
    public string speciesName;
    public float birthTime;
    public Agent representative;
    public Genome foundingGenome; // Initial state for drift detection
    public List<Agent> members = new List<Agent>();
    public bool isViable = false;
    public static int viabilityThreshold; // Minimum members to count as a real species
    public int driftGeneration = 0; // How many times this lineage has drifted

    public Species(int _id, Agent founder, int _parentSpeciesId, float _birthTime)
    {
        id = _id;
        parentSpeciesId = _parentSpeciesId;
        birthTime = _birthTime;
        representative = founder;
        foundingGenome = CloneGenome(founder.genome);
        members.Add(founder);

        // Name: inherit ancestor's prefix if parent exists, otherwise new root name
        string ancestorName = GetAncestorName(_parentSpeciesId);
        if (ancestorName != null)
            speciesName = SpeciesNamer.GenerateChildName(_id, ancestorName);
        else
            speciesName = SpeciesNamer.GenerateName(_id);
    }

    private static string GetAncestorName(int parentSpeciesId)
    {
        if (parentSpeciesId < 0 || SpeciationManager.instance == null) return null;
        foreach (var s in SpeciationManager.instance.species)
        {
            if (s.id == parentSpeciesId)
                return s.speciesName;
        }
        return null;
    }

    private Genome CloneGenome(Genome original)
    {
        return Genome.Clone(original);
    }

    public void AddMember(Agent agent)
    {
        members.Add(agent);

        // Fire species creation event once viability threshold is reached
        if (!isViable && members.Count >= viabilityThreshold)
        {
            isViable = true;
            SimEvents.SpeciesCreated(id, parentSpeciesId);
        }
    }

    public void RemoveMember(Agent agent)
    {
        members.Remove(agent);
        // If the representative was removed, pick a new one
        if (agent == representative && members.Count > 0)
        {
            representative = members[0];
        }
    }
}

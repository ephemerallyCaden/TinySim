using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Records species lifecycle data over time for the phylogenetic diagram.
/// </summary>
public class SpeciesHistoryTracker : MonoBehaviour
{
    public static SpeciesHistoryTracker instance;

    [Header("Sampling")]
    public float sampleInterval = 5f;
    public float minimumSurvivalDuration = 50f; // Species must survive this long or get wiped from history

    private Dictionary<int, SpeciesHistoryEntry> entries = new Dictionary<int, SpeciesHistoryEntry>();
    private float nextSampleTime = 0f;
    private int sampleCount = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        SimEvents.OnSpeciesCreated += HandleSpeciesCreated;
        SimEvents.OnSpeciesExtinct += HandleSpeciesExtinct;
        SimEvents.OnSpeciesEvolved += HandleSpeciesEvolved;
    }

    private void Start()
    {
        // Catch any species that were created before we subscribed (during initialisation)
        if (SpeciationManager.instance != null)
        {
            foreach (var s in SpeciationManager.instance.species)
            {
                if (s.isViable && !entries.ContainsKey(s.id))
                {
                    HandleSpeciesCreated(s.id, s.parentSpeciesId);
                }
            }
        }
    }

    private void OnDisable()
    {
        SimEvents.OnSpeciesCreated -= HandleSpeciesCreated;
        SimEvents.OnSpeciesExtinct -= HandleSpeciesExtinct;
        SimEvents.OnSpeciesEvolved -= HandleSpeciesEvolved;
    }

    private void HandleSpeciesCreated(int speciesId, int parentSpeciesId)
    {
        if (!entries.ContainsKey(speciesId))
        {
            // Root species that existed since the start should draw from sample 0
            int actualBirthSample = sampleCount;
            if (parentSpeciesId < 0)
            {
                // Check if this species was created at worldTime 0 (initial population)
                if (SpeciationManager.instance != null)
                {
                    foreach (var s in SpeciationManager.instance.species)
                    {
                        if (s.id == speciesId && s.birthTime < 1f)
                        {
                            actualBirthSample = 0;
                            break;
                        }
                    }
                }
            }

            // Get the actual species name from SpeciationManager (includes lineage)
            string name = null;
            if (SpeciationManager.instance != null)
            {
                foreach (var s in SpeciationManager.instance.species)
                {
                    if (s.id == speciesId) { name = s.speciesName; break; }
                }
            }
            if (name == null) name = SpeciesNamer.GenerateName(speciesId);

            var entry = new SpeciesHistoryEntry
            {
                speciesId = speciesId,
                parentSpeciesId = parentSpeciesId,
                speciesName = name,
                birthTime = SimulationManager.instance.worldTime,
                birthSampleIndex = actualBirthSample,
                extinctionTime = -1f,
                extinctionSampleIndex = -1,
                memberCountHistory = new List<int>(),
                colour = GetFounderColour(speciesId)
            };
            entries[speciesId] = entry;
        }
    }

    private void HandleSpeciesEvolved(int oldSpeciesId, int newSpeciesId)
    {
        if (entries.TryGetValue(oldSpeciesId, out var oldEntry))
        {
            UnityEngine.Debug.Log($"[PHYLO-TRACK] Species EVOLVED: {oldEntry.speciesName}(id:{oldSpeciesId}) -> new id:{newSpeciesId} at sample:{sampleCount}");
            oldEntry.extinctionTime = SimulationManager.instance.worldTime;
            oldEntry.extinctionSampleIndex = sampleCount;
        }
    }

    private void HandleSpeciesExtinct(int speciesId)
    {
        if (entries.TryGetValue(speciesId, out var entry))
        {
            // Skip if already marked extinct (e.g., from HandleSpeciesEvolved)
            if (entry.extinctionTime >= 0) return;

            float survivalDuration = SimulationManager.instance.worldTime - entry.birthTime;

            if (survivalDuration < minimumSurvivalDuration)
            {
                UnityEngine.Debug.Log($"[PHYLO-TRACK] Species WIPED (too short): {entry.speciesName}(id:{speciesId}) survived:{survivalDuration:F1}s < {minimumSurvivalDuration}s");
                entries.Remove(speciesId);
                needsRedraw = true;
            }
            else
            {
                UnityEngine.Debug.Log($"[PHYLO-TRACK] Species EXTINCT: {entry.speciesName}(id:{speciesId}) survived:{survivalDuration:F1}s at sample:{sampleCount}");
                entry.extinctionTime = SimulationManager.instance.worldTime;
                entry.extinctionSampleIndex = sampleCount;
            }
        }
    }

    private void Update()
    {
        float worldTime = SimulationManager.instance.worldTime;
        if (worldTime < nextSampleTime) return;

        // Process missed sample intervals (capped to avoid frame stalls at high speed)
        int maxCatchUp = 20;
        while (nextSampleTime <= worldTime && maxCatchUp > 0)
        {
            nextSampleTime += sampleInterval;
            sampleCount++;
            SampleSpecies();
            maxCatchUp--;
        }
    }

    private void SampleSpecies()
    {

        // Build a lookup of current member counts and pick up any viable species we missed
        Dictionary<int, int> currentCounts = new Dictionary<int, int>();
        foreach (var s in SpeciationManager.instance.species)
        {
            currentCounts[s.id] = s.members.Count;

            // If this species is viable but not yet tracked, add it now
            if (s.isViable && !entries.ContainsKey(s.id))
            {
                HandleSpeciesCreated(s.id, s.parentSpeciesId);
            }
        }

        // Ensure every tracked species gets exactly one entry this sample
        foreach (var kvp in entries)
        {
            var entry = kvp.Value;

            // Skip extinct species — their history is frozen
            if (entry.extinctionTime >= 0) continue;

            // Get current count (0 if not found in active species)
            int count = currentCounts.ContainsKey(entry.speciesId) ? currentCounts[entry.speciesId] : 0;

            // Pad any missed samples with the current count (shouldn't happen, but safety)
            while (entry.memberCountHistory.Count < sampleCount - entry.birthSampleIndex - 1)
            {
                entry.memberCountHistory.Add(count);
            }

            entry.memberCountHistory.Add(count);
        }
    }

    public Dictionary<int, SpeciesHistoryEntry> GetAllEntries() => entries;
    public int SampleCount => sampleCount;
    public bool needsRedraw = false;

    private Color GetFounderColour(int speciesId)
    {
        // Use the founder agent's actual colour so diagram matches simulation visuals
        if (SpeciationManager.instance != null)
        {
            foreach (var s in SpeciationManager.instance.species)
            {
                if (s.id == speciesId && s.representative != null)
                {
                    return s.representative.colour;
                }
            }
        }

        // Fallback: golden ratio hue if species/representative not found
        float hue = (speciesId * 0.618033988f) % 1.0f;
        return Color.HSVToRGB(hue, 0.7f, 0.9f);
    }
}

public class SpeciesHistoryEntry
{
    public int speciesId;
    public int parentSpeciesId;
    public string speciesName;
    public float birthTime;
    public int birthSampleIndex;
    public float extinctionTime;       // -1 if still alive
    public int extinctionSampleIndex;  // -1 if still alive
    public List<int> memberCountHistory;
    public Color colour;
}

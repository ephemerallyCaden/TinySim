using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeciesDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;
    public TMP_Text headerText;
    public TMP_Text statsText;
    public TMP_Text attributesText;
    public SpeciesModelPreview modelPreview;
    public NeuralNetworkVisualiser networkVisualiser;
    public AncestryView ancestryView;

    private int currentSpeciesId = -1;

    private void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Show(int speciesId)
    {
        currentSpeciesId = speciesId;
        Refresh();
        if (panelRoot != null)
            panelRoot.SetActive(true);
        if (ancestryView != null)
            ancestryView.ShowAncestry(speciesId);
    }

    public void Hide()
    {
        currentSpeciesId = -1;
        if (panelRoot != null)
            panelRoot.SetActive(false);
        if (ancestryView != null)
            ancestryView.Clear();
    }

    private void Refresh()
    {
        var entries = SpeciesHistoryTracker.instance.GetAllEntries();
        if (!entries.TryGetValue(currentSpeciesId, out var entry)) return;

        // Header
        bool alive = entry.extinctionTime < 0;
        string status = alive ? "<color=#88ff88>Alive</color>" : "<color=#ff8888>Extinct</color>";
        if (headerText != null)
            headerText.text = $"{entry.speciesName}  ({status})";

        // Model preview
        if (modelPreview != null)
            modelPreview.SetAgent(entry.colour, entry.foundingAttributes.size);

        // Stats
        if (statsText != null)
        {
            int currentPop = 0;
            if (alive && SpeciationManager.instance != null)
            {
                foreach (var s in SpeciationManager.instance.species)
                {
                    if (s.id == currentSpeciesId) { currentPop = s.members.Count; break; }
                }
            }

            string parentName = GetParentName(entry.parentSpeciesId);
            string extinctionInfo = alive ? "" : $"\nExtinct at: {entry.extinctionTime:F0}s";

            statsText.text =
                $"Born: {entry.birthTime:F0}s{extinctionInfo}" +
                $"\nPeak Population: {entry.peakPopulation}" +
                $"\nCurrent Population: {currentPop}" +
                $"\nParent: {parentName}";
        }

        // Attributes
        if (attributesText != null)
        {
            var attrs = entry.foundingAttributes;
            string dietLabel = attrs.dietPreference < 0.33f ? "Herbivore"
                : attrs.dietPreference > 0.66f ? "Carnivore"
                : "Omnivore";

            attributesText.text =
                $"Size: {attrs.size:F2}" +
                $"\nSpeed: {attrs.speed:F2}" +
                $"\nVision Dist: {attrs.visionDistance:F1}" +
                $"\nVision Angle: {attrs.visionAngle:F0}" +
                $"\nDiet: {dietLabel} ({attrs.dietPreference:F2})" +
                $"\nAttack: {attrs.attackDamage:F1}" +
                $"\nRep Cooldown: {attrs.maxReproductionCooldown:F1}" +
                $"\nRep Cost: {attrs.reproductionEnergyCost:F1}" +
                $"\nMut Chance Mod: {attrs.mutationChanceMod:F2}" +
                $"\nMut Magnitude Mod: {attrs.mutationMagnitudeMod:F2}";
        }

        // Neural Network
        if (networkVisualiser != null && entry.foundingGenome != null)
        {
            NeuralNetwork network = new NeuralNetwork(entry.foundingGenome);
            networkVisualiser.Visualise(network);
        }
    }

    private string GetParentName(int parentSpeciesId)
    {
        if (parentSpeciesId < 0) return "None (root)";

        var entries = SpeciesHistoryTracker.instance.GetAllEntries();
        if (entries.TryGetValue(parentSpeciesId, out var parentEntry))
            return parentEntry.speciesName;

        return $"Unknown ({parentSpeciesId})";
    }
}

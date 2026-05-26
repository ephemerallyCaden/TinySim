using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WorldCreationUI : MonoBehaviour
{
    [Header("Config")]
    public SimulationConfig config;

    [Header("Preview")]
    public TemperatureMapPreview mapPreview;

    [Header("World Controls")]
    public TMP_InputField seedField;
    public Button randomiseSeedButton;
    public Slider worldSizeSlider;
    public TMP_Text worldSizeLabel;

    [Header("Temperature Controls")]
    public Slider temperatureScaleSlider;
    public TMP_Text temperatureScaleLabel;
    public Slider coldSkewSlider;
    public TMP_Text coldSkewLabel;

    [Header("Population Controls")]
    public Slider initialAgentsSlider;
    public TMP_Text initialAgentsLabel;
    public Slider maxPopulationSlider;
    public TMP_Text maxPopulationLabel;
    public TMP_Dropdown spawnPatternDropdown;
    public Toggle uniformStartToggle;

    [Header("Food Controls")]
    public Slider initialFoodSlider;
    public TMP_Text initialFoodLabel;
    public Slider maxFoodSlider;
    public TMP_Text maxFoodLabel;

    [Header("Predation Controls")]
    public Toggle enablePredationToggle;

    [Header("Agent Attribute Rows")]
    public GameObject[] uniformRows;
    public GameObject[] rangeRows;

    [Header("Uniform Start Values")]
    public Slider baseSizeSlider;
    public TMP_Text baseSizeLabel;
    public Slider baseSpeedSlider;
    public TMP_Text baseSpeedLabel;
    public Slider baseVisionDistSlider;
    public TMP_Text baseVisionDistLabel;
    public Slider baseVisionAngleSlider;
    public TMP_Text baseVisionAngleLabel;
    public Slider baseRepCooldownSlider;
    public TMP_Text baseRepCooldownLabel;
    public Slider baseRepCostSlider;
    public TMP_Text baseRepCostLabel;
    public Slider baseDietSlider;
    public TMP_Text baseDietLabel;
    public Slider baseAttackSlider;
    public TMP_Text baseAttackLabel;
    public Slider baseMutChanceSlider;
    public TMP_Text baseMutChanceLabel;
    public Slider baseMutMagnitudeSlider;
    public TMP_Text baseMutMagnitudeLabel;

    [Header("Initial Agent Ranges (RangeSliders)")]
    public RangeSlider sizeRange;
    public RangeSlider speedRange;
    public RangeSlider visionDistRange;
    public RangeSlider visionAngleRange;
    public RangeSlider mutChanceRange;
    public RangeSlider mutMagnitudeRange;
    public RangeSlider repCooldownRange;
    public RangeSlider repCostRange;
    public RangeSlider dietRange;
    public RangeSlider attackRange;

    [Header("Start")]
    public Button startButton;

    private void Start()
    {
        LoadFromConfig();
        WireCallbacks();
        RefreshPreview();
        SetAttributeRowVisibility(config.uniformStart);
    }

    private void LoadFromConfig()
    {
        // World
        seedField.text = config.seed.ToString();
        worldSizeSlider.value = config.worldSize;
        UpdateLabel(worldSizeLabel, config.worldSize);

        // Temperature
        temperatureScaleSlider.value = config.temperatureScale;
        UpdateLabel(temperatureScaleLabel, config.temperatureScale);
        coldSkewSlider.value = config.coldSkewPower;
        UpdateLabel(coldSkewLabel, config.coldSkewPower);

        // Population
        initialAgentsSlider.value = config.initialAgentCount;
        UpdateLabel(initialAgentsLabel, config.initialAgentCount);
        maxPopulationSlider.value = config.maxPopulation;
        UpdateLabel(maxPopulationLabel, config.maxPopulation);
        spawnPatternDropdown.value = config.spawnPattern;
        uniformStartToggle.isOn = config.uniformStart;

        // Food
        initialFoodSlider.value = config.initialFoodCount;
        UpdateLabel(initialFoodLabel, config.initialFoodCount);
        maxFoodSlider.value = config.maxFoodCount;
        UpdateLabel(maxFoodLabel, config.maxFoodCount);

        // Predation
        enablePredationToggle.isOn = config.enablePredation;

        // Uniform Start Values
        LoadRange(baseSizeSlider, baseSizeLabel, config.baseSize);
        LoadRange(baseSpeedSlider, baseSpeedLabel, config.baseSpeed);
        LoadRange(baseVisionDistSlider, baseVisionDistLabel, config.baseVisionDistance);
        LoadRange(baseVisionAngleSlider, baseVisionAngleLabel, config.baseVisionAngle);
        LoadRange(baseRepCooldownSlider, baseRepCooldownLabel, config.baseMaxReproductionCooldown);
        LoadRange(baseRepCostSlider, baseRepCostLabel, config.baseReproductionEnergyCost);
        LoadRange(baseDietSlider, baseDietLabel, config.baseDietPreference);
        LoadRange(baseAttackSlider, baseAttackLabel, config.baseAttackDamage);
        LoadRange(baseMutChanceSlider, baseMutChanceLabel, config.baseMutationChanceMod);
        LoadRange(baseMutMagnitudeSlider, baseMutMagnitudeLabel, config.baseMutationMagnitudeMod);

        // Initial Agent Ranges
        if (sizeRange != null) sizeRange.SetValues(config.initialSizeMin, config.initialSizeMax);
        if (speedRange != null) speedRange.SetValues(config.initialSpeedMin, config.initialSpeedMax);
        if (visionDistRange != null) visionDistRange.SetValues(config.initialVisionDistanceMin, config.initialVisionDistanceMax);
        if (visionAngleRange != null) visionAngleRange.SetValues(config.initialVisionAngleMin, config.initialVisionAngleMax);
        if (mutChanceRange != null) mutChanceRange.SetValues(config.initialMutationChanceModMin, config.initialMutationChanceModMax);
        if (mutMagnitudeRange != null) mutMagnitudeRange.SetValues(config.initialMutationMagnitudeModMin, config.initialMutationMagnitudeModMax);
        if (dietRange != null) dietRange.SetValues(config.initialDietPreferenceMin, config.initialDietPreferenceMax);
        if (attackRange != null) attackRange.SetValues(config.initialAttackDamageMin, config.initialAttackDamageMax);
        if (repCooldownRange != null) repCooldownRange.SetValues(config.initialMaxRepCooldownMin, config.initialMaxRepCooldownMax);
        if (repCostRange != null) repCostRange.SetValues(config.initialRepEnergyCostMin, config.initialRepEnergyCostMax);
    }

    private void LoadRange(Slider slider, TMP_Text label, float value)
    {
        if (slider != null) slider.value = value;
        UpdateLabel(label, value);
    }

    private void WireCallbacks()
    {
        // World
        seedField.onEndEdit.AddListener(val =>
        {
            if (int.TryParse(val, out int seed))
                config.seed = seed;
        });
        randomiseSeedButton.onClick.AddListener(() =>
        {
            config.seed = -1;
            seedField.text = "-1";
        });
        worldSizeSlider.onValueChanged.AddListener(val =>
        {
            config.worldSize = Mathf.RoundToInt(val);
            UpdateLabel(worldSizeLabel, config.worldSize);
            RefreshPreview();
        });

        // Temperature
        temperatureScaleSlider.onValueChanged.AddListener(val =>
        {
            config.temperatureScale = val;
            UpdateLabel(temperatureScaleLabel, val);
            RefreshPreview();
        });
        coldSkewSlider.onValueChanged.AddListener(val =>
        {
            config.coldSkewPower = val;
            UpdateLabel(coldSkewLabel, val);
            RefreshPreview();
        });

        // Population
        initialAgentsSlider.onValueChanged.AddListener(val =>
        {
            config.initialAgentCount = Mathf.RoundToInt(val);
            UpdateLabel(initialAgentsLabel, config.initialAgentCount);
        });
        maxPopulationSlider.onValueChanged.AddListener(val =>
        {
            config.maxPopulation = Mathf.RoundToInt(val);
            UpdateLabel(maxPopulationLabel, config.maxPopulation);
        });
        spawnPatternDropdown.onValueChanged.AddListener(val =>
        {
            config.spawnPattern = val;
        });
        uniformStartToggle.onValueChanged.AddListener(val =>
        {
            config.uniformStart = val;
            SetAttributeRowVisibility(val);
        });

        // Uniform Start Values
        WireRange(baseSizeSlider, baseSizeLabel, v => config.baseSize = v);
        WireRange(baseSpeedSlider, baseSpeedLabel, v => config.baseSpeed = v);
        WireRange(baseVisionDistSlider, baseVisionDistLabel, v => config.baseVisionDistance = v);
        WireRange(baseVisionAngleSlider, baseVisionAngleLabel, v => config.baseVisionAngle = v);
        WireRange(baseRepCooldownSlider, baseRepCooldownLabel, v => config.baseMaxReproductionCooldown = v);
        WireRange(baseRepCostSlider, baseRepCostLabel, v => config.baseReproductionEnergyCost = v);
        WireRange(baseDietSlider, baseDietLabel, v => config.baseDietPreference = v);
        WireRange(baseAttackSlider, baseAttackLabel, v => config.baseAttackDamage = v);
        WireRange(baseMutChanceSlider, baseMutChanceLabel, v => config.baseMutationChanceMod = v);
        WireRange(baseMutMagnitudeSlider, baseMutMagnitudeLabel, v => config.baseMutationMagnitudeMod = v);

        // Food
        initialFoodSlider.onValueChanged.AddListener(val =>
        {
            config.initialFoodCount = Mathf.RoundToInt(val);
            UpdateLabel(initialFoodLabel, config.initialFoodCount);
        });
        maxFoodSlider.onValueChanged.AddListener(val =>
        {
            config.maxFoodCount = Mathf.RoundToInt(val);
            UpdateLabel(maxFoodLabel, config.maxFoodCount);
        });

        // Predation
        enablePredationToggle.onValueChanged.AddListener(val =>
        {
            config.enablePredation = val;
        });

        // Initial Agent Ranges (RangeSliders)
        WireRangeSlider(sizeRange, (min, max) => { config.initialSizeMin = min; config.initialSizeMax = max; });
        WireRangeSlider(speedRange, (min, max) => { config.initialSpeedMin = min; config.initialSpeedMax = max; });
        WireRangeSlider(visionDistRange, (min, max) => { config.initialVisionDistanceMin = min; config.initialVisionDistanceMax = max; });
        WireRangeSlider(visionAngleRange, (min, max) => { config.initialVisionAngleMin = min; config.initialVisionAngleMax = max; });
        WireRangeSlider(mutChanceRange, (min, max) => { config.initialMutationChanceModMin = min; config.initialMutationChanceModMax = max; });
        WireRangeSlider(mutMagnitudeRange, (min, max) => { config.initialMutationMagnitudeModMin = min; config.initialMutationMagnitudeModMax = max; });
        WireRangeSlider(dietRange, (min, max) => { config.initialDietPreferenceMin = min; config.initialDietPreferenceMax = max; });
        WireRangeSlider(attackRange, (min, max) => { config.initialAttackDamageMin = min; config.initialAttackDamageMax = max; });
        WireRangeSlider(repCooldownRange, (min, max) => { config.initialMaxRepCooldownMin = min; config.initialMaxRepCooldownMax = max; });
        WireRangeSlider(repCostRange, (min, max) => { config.initialRepEnergyCostMin = min; config.initialRepEnergyCostMax = max; });

        // Start
        startButton.onClick.AddListener(StartSimulation);
    }

    private void WireRange(Slider slider, TMP_Text label, System.Action<float> setter)
    {
        if (slider == null) return;
        slider.onValueChanged.AddListener(val =>
        {
            setter(val);
            UpdateLabel(label, val);
        });
    }

    private void WireRangeSlider(RangeSlider rangeSlider, System.Action<float, float> setter)
    {
        if (rangeSlider == null) return;
        rangeSlider.OnValueChanged += (min, max) => setter(min, max);
    }

    private void SetAttributeRowVisibility(bool isUniform)
    {
        foreach (var row in uniformRows) if (row) row.SetActive(isUniform);
        foreach (var row in rangeRows) if (row) row.SetActive(!isUniform);
    }

    private void RefreshPreview()
    {
        if (mapPreview != null)
            mapPreview.Regenerate(config.worldSize, config.temperatureScale, config.coldSkewPower);
    }

    private void StartSimulation()
    {
        SceneManager.LoadScene("Main");
    }

    private void UpdateLabel(TMP_Text label, float value)
    {
        if (label != null)
            label.text = value.ToString("F1");
    }

    private void UpdateLabel(TMP_Text label, int value)
    {
        if (label != null)
            label.text = value.ToString();
    }
}

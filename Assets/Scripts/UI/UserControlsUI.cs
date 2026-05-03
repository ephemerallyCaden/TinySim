using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserControlsUI : MonoBehaviour
{
    public TMP_Text simulationSpeedText;
    public Slider simulationSpeedSlider;

    public void UpdateSimulationSpeed()
    {
        //Updates simulation speed on slider value change
        simulationSpeedText.text = $"Simulation Speed: {simulationSpeedSlider.value}x";
        SimulationManager.instance.simulationSpeed = simulationSpeedSlider.value;
    }

    private void Start()
    {
        // Bind slider event in code so it works even if Inspector binding was lost
        if (simulationSpeedSlider != null)
            simulationSpeedSlider.onValueChanged.AddListener(delegate { UpdateSimulationSpeed(); });
    }

    private void Update()
    {
        // Keyboard shortcuts for simulation speed
        // ] to speed up, [ to slow down, P to pause
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            SimulationManager.instance.simulationSpeed = Mathf.Min(SimulationManager.instance.simulationSpeed + 1f, 20f);
            UpdateSpeedDisplay();
        }
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            SimulationManager.instance.simulationSpeed = Mathf.Max(SimulationManager.instance.simulationSpeed - 1f, 1f);
            UpdateSpeedDisplay();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SimulationManager.instance.TogglePause();
        }
    }

    private void UpdateSpeedDisplay()
    {
        float speed = SimulationManager.instance.simulationSpeed;
        if (simulationSpeedText != null)
            simulationSpeedText.text = $"Simulation Speed: {speed}x";
        if (simulationSpeedSlider != null)
            simulationSpeedSlider.value = speed;
    }
}

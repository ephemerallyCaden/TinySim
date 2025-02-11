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
        simulationSpeedText.text = $"Simulation Speed: {simulationSpeedSlider.value}x";
        SimulationManager.instance.simulationSpeed = simulationSpeedSlider.value;
    }
}

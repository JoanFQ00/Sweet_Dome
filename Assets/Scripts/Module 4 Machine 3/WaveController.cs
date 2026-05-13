using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WaveController : MonoBehaviour
{
    public WaveRenderer WaveRenderer;

    public Slider amplitudeSlider;
    public Slider frequencySlider;
    public Slider phaseSlider;

    void Start()
    {
        if (amplitudeSlider != null) 
        {
            amplitudeSlider.onValueChanged.AddListener(OnAmplitudedChanged);
            frequencySlider.onValueChanged.AddListener(OnFrequencyChanged);
            phaseSlider.onValueChanged.AddListener(OnPhaseChanged);
        }
    }

    void OnAmplitudedChanged(float value) 
    {
        WaveRenderer.amplitude = value;
    }

    void OnFrequencyChanged(float value) 
    {
        WaveRenderer.frequency = value;
    }

    void OnPhaseChanged(float value) 
    {
        WaveRenderer.phase = value;
    }
}
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
        amplitudeSlider.value = WaveRenderer.amplitude;
        frequencySlider.value = WaveRenderer.frequency;
        phaseSlider.value = WaveRenderer.phase;
    }

    void Update()
    {
        amplitudeSlider.value = WaveRenderer.amplitude;
        frequencySlider.value = WaveRenderer.frequency;
        phaseSlider.value = WaveRenderer.phase;
    }
}
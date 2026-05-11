using UnityEngine;
using TMPro;

public class TemperatureSistem : MonoBehaviour
{
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI farengheitText;

    public float temperatureCelsius = 0f;
    float temperatureFarenheit;

    void Start()
    {
        Conversor();
    }

    public void TemperatureUp()
    {
        temperatureCelsius += 1f;
        Conversor();
    }

    public void TemperatureDown()
    {
        temperatureCelsius -= 1f;
        Conversor();
    }

    void Conversor()
    {
        temperatureFarenheit = (temperatureCelsius * 9f / 5f) + 32f;
        temperatureText.text = temperatureCelsius + "ºC" ;
        farengheitText.text = temperatureFarenheit + "ºF";
    }
}

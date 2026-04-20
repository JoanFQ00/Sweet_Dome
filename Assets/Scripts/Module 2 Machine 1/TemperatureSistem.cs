using UnityEngine;
using TMPro;

public class TemperatureSistem : MonoBehaviour
{
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI stateText;

    public float temperatureCelsius = 0f;
    float temperatureFarenheit;

    public bool hasCharge = true;

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

        temperatureText.text = temperatureCelsius + "ºC / " + temperatureFarenheit + "ºF";
        if (!hasCharge)
        {
            stateText.text = "State = Null";
        }
        else if (temperatureCelsius < 0)
        {
            stateText.text = "State = Solid";
        }
        else if (temperatureCelsius >= 0 && temperatureCelsius <= 100)
        {
            stateText.text = "State = Liquid";
        }
        else
        {
            stateText.text = "State = Gas";
        }
    }
}

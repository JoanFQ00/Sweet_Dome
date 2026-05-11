using UnityEngine;
using TMPro;

public class TemperatureSistem : MonoBehaviour
{
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI farengheitText;

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
            farengheitText.text = "State = Null";
        }
        else if (temperatureCelsius < 0)
        {
            farengheitText.text = "State = Solid";
        }
        else if (temperatureCelsius >= 0 && temperatureCelsius <= 100)
        {
            farengheitText.text = "State = Liquid";
        }
        else
        {
            farengheitText.text = "State = Gas";
        }
    }
}

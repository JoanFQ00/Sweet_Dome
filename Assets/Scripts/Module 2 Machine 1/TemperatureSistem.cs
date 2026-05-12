using UnityEngine;
using TMPro;

public class TemperatureSistem : MonoBehaviour
{
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI farengheitText;
    public AutoTemperature autoSystem;

    public float currentTemperature = 0f;
    public float targetTemperature = 0f;

    public float speed = 2f;

    public float tolerance = 1f;

    float temperatureFarenheit;

    void Update()
    {
        currentTemperature = Mathf.MoveTowards(currentTemperature,targetTemperature,speed * Time.deltaTime);
        UpdateUI();
    }

    public void TemperatureUp()
    {
        targetTemperature += 1f;
    }

    public void TemperatureDown()
    {
        targetTemperature -= 1f;
    }

    void UpdateUI()
    {
        temperatureFarenheit = (currentTemperature * 9f / 5f) + 32f;

        temperatureText.text = Mathf.RoundToInt(currentTemperature) + "ºC";
        farengheitText.text = Mathf.RoundToInt(temperatureFarenheit) + "ºF";
    }
}
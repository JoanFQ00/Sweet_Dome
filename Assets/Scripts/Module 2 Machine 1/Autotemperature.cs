using UnityEngine;
using TMPro;

public class AutoTemperature : MonoBehaviour
{
    public TextMeshProUGUI targetTempText;

    public float targetTemperature;
    public float minTemp = -10f;
    public float maxTemp = 50f;

    public float changeInterval = 5f;
    private float timer;

    void Start()
    {
        GenerateNewTemperature();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeInterval)
        {
            timer = 0f;
            GenerateNewTemperature();
        }

        UpdateUI();
    }

    void GenerateNewTemperature()
    {
        targetTemperature = Random.Range(minTemp, maxTemp);
    }

    void UpdateUI()
    {
        if (targetTempText != null) 
        {
            targetTempText.text = Mathf.RoundToInt(targetTemperature) + "ºC";
        }
    }
}
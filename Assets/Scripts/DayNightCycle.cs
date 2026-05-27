using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Sun Settings")]
    public Light sun;

    public float sunriseAngle = 90f;
    public float sunsetAngle = -90f;

    private void OnEnable()
    {
        DayTime.OnTimeChanged += UpdateSunRotation;
    }

    private void OnDisable()
    {
        DayTime.OnTimeChanged -= UpdateSunRotation;
    }

    private void Start()
    {
        UpdateSunRotation(DayTime.Instance.GetCurrentHour(),DayTime.Instance.GetCurrentMinutes());
    }

    void UpdateSunRotation(int hour, int minutes)
    {
        float currentTime = hour + (minutes / 60f);
        float dayStart = DayTime.Instance.startHourDay;
        float dayEnd = DayTime.Instance.endHour;
        float normalizedTime = Mathf.InverseLerp(dayStart, dayEnd, currentTime);
        float sunRotation = Mathf.Lerp(sunriseAngle, sunsetAngle, normalizedTime);

        transform.rotation = Quaternion.Euler(-sunRotation, 170f, 0f);

        if (sun != null)
        {
            sun.intensity = Mathf.Lerp(0.2f,1.2f,Mathf.Sin(normalizedTime * Mathf.PI));
        }

        RenderSettings.ambientLight = Color.Lerp(Color.black,Color.white,Mathf.Sin(normalizedTime * Mathf.PI));
    }
}
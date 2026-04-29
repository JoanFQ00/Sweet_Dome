using System;
using TMPro;
using UnityEngine;

public class DayTime : MonoBehaviour
{
    public static DayTime Instance;

    [Header("Time")]
    public int startHourDay = 9;
    public int endHour = 21;
    public int currentHour;

    private int currentMinutes;

    private bool timeStopped = false;

    [Header("Speed")]
    public float secondsPerPhase = 10f;
    public int minutesPassed = 10;
    private float counter;

    [Header("UI")]
    public TextMeshProUGUI timerText;

    private bool canSleepChange = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHour = startHourDay;
        currentMinutes = 0;
        UpdateUI();
    }

    void Update()
    {
        if (timeStopped) return;

        counter += Time.deltaTime;

        if (counter >= secondsPerPhase)
        {
            counter = 0f;
            AddMinutes(minutesPassed); 
        }
    }

    void AddMinutes(int amountTime)
    {
        currentMinutes += amountTime;

        if (currentMinutes >= 60)
        {
            currentMinutes = 0;
            currentHour++;
        }

        if (currentHour >= endHour)
        {
            currentHour = endHour;
            currentMinutes = 0;
            canSleepChange = true;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        timerText.text = currentHour.ToString("00") + ":" + currentMinutes.ToString("00");
    }

    public bool CanSleepInBed()
    {
        return canSleepChange;
    }

    public void ResetDay()
    {
        currentHour = startHourDay;
        currentMinutes = 0;
        canSleepChange = false;
        UpdateUI();
    }

    public int GetCurrentHour() 
    {
        return currentHour;
    }

    public void StopTimeCount(bool stop) 
    {
        timeStopped = stop;
    }
}

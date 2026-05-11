using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DayTime : MonoBehaviour
{
    public static DayTime Instance;

    [Header("Days")]
    public List<GameObject> minigames;
    public int maxDays = 2;
    private int currentDay;

    [Header("Time")]
    public int startHourDay = 9;
    public int endHour = 21;
    public int currentHour;
    private int currentMinutes;

    [Header("UI")]
    public TextMeshProUGUI timerText;


    public static Action<int> OnTimeSkiped;

    private bool canSleepChange = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GameManager.OnChangeDay += LoadNextDay;
    }

    private void OnDisable()
    {
        GameManager.OnChangeDay -= LoadNextDay;
    }

    void Start()
    {
        currentDay = PlayerPrefs.GetInt("lastDay", 0);

        ResetDayTime();

        LoadMinigames();
    }

    // =======================
    // DAY LOGIC
    // =======================

    void LoadNextDay()
    {
        currentDay++;

        if (currentDay > maxDays)
            currentDay = 0;

        GameManager.Instance.decManager.ResetDayActions();
        
        SaveLastDay(currentDay);

        LoadMinigames();
        ResetDayTime(); 
    }

    void LoadMinigames()
    {
        for (int i = 0; i < minigames.Count; i++)
        {
            minigames[i].SetActive(i == currentDay);
        }
    }

    void SaveLastDay(int lastDay)
    {
        PlayerPrefs.SetInt("lastDay", lastDay);
    }

    void UpdateUI()
    {
        if (timerText != null)
            timerText.text = currentHour.ToString("00") + ":" + currentMinutes.ToString("00");
    }

    public void ResetDayTime()
    {
        currentHour = startHourDay;
        currentMinutes = 0;
        canSleepChange = false;
        UpdateUI();
    }

    public bool CanSleepInBed()
    {
        return true;
    }

    public int GetCurrentHour()
    {
        return currentHour;
    }

    public void SetCurrentTime(int _hour, int _minute) 
    {
        currentHour = _hour;
        currentMinutes = _minute;
        UpdateUI();
    }

    public void SkipTime() 
    {
        if (currentHour <= 13) 
        {
            currentHour += 2;
            OnTimeSkiped?.Invoke(currentHour);
            UpdateUI();
        }
    }
}
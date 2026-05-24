using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private bool canSleep = false;

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
        LevelManager.OnLevelLoaded += ResetDayTime;
    }

    private void OnDisable()
    {
        GameManager.OnChangeDay -= LoadNextDay;
        LevelManager.OnLevelLoaded -= ResetDayTime;
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


    void LoadNextDay(Day c_Day)
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

    public void UpdateUI()
    {
        if (timerText != null)
            timerText.text = currentHour.ToString("00") + ":" + currentMinutes.ToString("00");
    }

    public void ResetDayTime()
    {
        SetCurrentTime(startHourDay, 0);
        UpdateUI();
    }

    public bool CanSleepInBed()
    {
        return canSleep;
    }

    public int GetCurrentHour()
    {
        return currentHour;
    }

    public void SetCurrentTime(int _hour, int _minute)
    {
        currentHour = _hour;
        currentMinutes = _minute;

        canSleep = currentHour >= 21;

        UpdateUI();
    }

    public void SkipTime() 
    {
        currentHour += 2;
        if (currentHour > 15) currentHour = 15;

        OnTimeSkiped?.Invoke(currentHour);
        UpdateUI();
    }
}
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

    // REMOVIDO: Se elimina private int currentDay para evitar desincronizaciones con el GameManager

    [Header("Time")]
    public int startHourDay = 9;
    public int endHour = 21;
    public int currentHour;
    private int currentMinutes;

    [Header("Lerp")]
    private bool isLerpingTime = false;
    private float startMinutes;
    private float targetMinutes;
    private float lerpDuration;
    private float lerpElapsed;

    public static Action<int, int> OnTimeChanged;
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
        ResetDayTime();
        LoadMinigames();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SetCurrentTime(21, 00);
        }

        UpdateTimeLerp();
    }

    void LoadNextDay(Day c_Day)
    {
        int dayIndex = (int)c_Day;

        GameManager.Instance.decManager.ResetDayActions();

        LoadMinigames();
        ResetDayTime();
    }

    public void LoadMinigames()
    {
        if (GameManager.Instance == null) return;

        int gameDayIndex = (int)GameManager.Instance.currentDay;

        for (int i = 0; i < minigames.Count; i++)
        {
            minigames[i].SetActive(i <= gameDayIndex);
        }
    }

    public bool CanSleepInBed()
    {
        return canSleep;
    }

    public int GetCurrentHour()
    {
        return currentHour;
    }

    public int GetCurrentMinutes()
    {
        return currentMinutes;
    }

    public void ResetDayTime()
    {
        SetCurrentTime(startHourDay, 0);
    }

    public void AddMinutes(int minutes)
    {
        currentMinutes += minutes;

        if (currentMinutes >= 60)
        {
            currentHour++;
            currentMinutes = 0;
        }

        NotifyTimeChanged();
    }

    public void SetCurrentTime(int _hour, int _minute)
    {
        currentHour = _hour;
        currentMinutes = _minute;

        canSleep = currentHour >= endHour;

        NotifyTimeChanged();
    }

    public void SkipTime()
    {
        currentHour += 2;

        if (currentHour > 15)
            currentHour = 15;

        OnTimeSkiped?.Invoke(currentHour);

        NotifyTimeChanged();
    }

    private void NotifyTimeChanged()
    {
        OnTimeChanged?.Invoke(currentHour, currentMinutes);
    }

    public void LerpToTime(int targetHour, int targetMinute, float duration)
    {
        startMinutes = currentHour * 60 + currentMinutes;
        targetMinutes = targetHour * 60 + targetMinute;

        lerpDuration = Mathf.Max(0.01f, duration);
        lerpElapsed = 0f;

        isLerpingTime = true;
    }

    private void UpdateTimeLerp()
    {
        if (!isLerpingTime) return;

        lerpElapsed += Time.deltaTime;

        float t = lerpElapsed / lerpDuration;

        float current = Mathf.Lerp(startMinutes, targetMinutes, t);

        currentHour = Mathf.FloorToInt(current / 60f);
        currentMinutes = Mathf.FloorToInt(current % 60f);

        OnTimeChanged?.Invoke(currentHour, currentMinutes);

        if (t >= 1f)
        {
            currentHour = Mathf.FloorToInt(targetMinutes / 60f);
            currentMinutes = Mathf.FloorToInt(targetMinutes % 60f);

            OnTimeChanged?.Invoke(currentHour, currentMinutes);

            isLerpingTime = false;
        }
    }
}
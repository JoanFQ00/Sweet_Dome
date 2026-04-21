using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DayLoop : MonoBehaviour
{
    public List<GameObject> minigames;

    public int maxDays = 2; 
    private int currentDay;


    private void OnEnable()
    {
        GameManager.OnChangeDay += LoadNextDay;
    }

    private void OnDisable()
    {
        GameManager.OnChangeDay -= LoadNextDay;
    }

    void LoadNextDay() 
    {
        currentDay += 1;
        Debug.Log("Today is: " + currentDay);
        LoadMinigames();
    }

    void Start()
    {
        currentDay = PlayerPrefs.GetInt("lastDay");
    }

    void LoadMinigames() 
    {
        for (int i = 0; i < minigames.Count; i++) 
        {
            if (i == currentDay)
            { 
                minigames[i].SetActive(true);
            }
        }
    }

    void SaveLastDay(int lastDay) 
    {
        PlayerPrefs.SetInt("lastDay", lastDay);
    }
}

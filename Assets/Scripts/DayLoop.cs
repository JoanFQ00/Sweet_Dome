using System.Collections.Generic;
using UnityEngine;

public class DayLoop : MonoBehaviour
{
    public static DayLoop Instance;

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

    void Start()
    {
        currentDay = PlayerPrefs.GetInt("lastDay");
    }

    void LoadNextDay() 
    {
        currentDay += 1;
        Debug.Log("Today is: " + currentDay);
        SaveLastDay(currentDay);
        LoadMinigames();
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

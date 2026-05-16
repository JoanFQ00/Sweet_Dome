using UnityEngine;

public class MachineActivator : MonoBehaviour
{
    [Header("Dias activos")]
    public bool day1 = true;
    public bool day2 = true;
    public bool day3 = true;
    public bool day4 = true;
    public bool day5 = true;

    [Header("Scripts to activate")]
    public MonoBehaviour[] scriptsToEnable;

    private void Start()
    {
        GameManager.NewDay += CheckDay;
    }

    private void OnDestroy()
    {
        GameManager.NewDay -= CheckDay;
    }

    void CheckDay()
    {
        bool activeDay = SwitchDayActivator();
        foreach (MonoBehaviour script in scriptsToEnable)
        {
            if(script != null)
                { script.enabled = true; }
        }
    }

    bool SwitchDayActivator()
    {
        switch (GameManager.Instance.currentDay)
        {
            case Day.Day1:
                return day1;
            case Day.Day2:
                return day2;
            case Day.Day3:
                return day3;
            case Day.Day4:
                return day4;
            case Day.Day5:
                return day5;
        }

        return false;
    }
}

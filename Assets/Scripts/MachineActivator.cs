using UnityEngine;

public class MachineActivator : MonoBehaviour
{
    [Header("Dias activos")]
    public bool day1 = true;
    public bool day2 = true;
    public bool day3 = true;
    public bool day4 = true;
    public bool day5 = true;

    [Header("Gameobjects DAY 1")]
    public GameObject[] machineEnabled;

    private void Start()
    {
        foreach (GameObject machine in machineEnabled)
            machine.SetActive(false);

        GameManager.NewDay += SwitchDayActivator;

        SwitchDayActivator();
    }

    private void OnDisable()
    {
        GameManager.NewDay -= SwitchDayActivator;
    }

    void SwitchDayActivator()
    {
        switch (GameManager.Instance.currentDay)
        {
            case Day.Day1:
                EnableMachineModules(1);
                break;

            case Day.Day2:
                EnableMachineModules(2);
                break;

            case Day.Day3:
                EnableMachineModules(3);
               break;

            case Day.Day4:
                EnableMachineModules(4);
                break;

            case Day.Day5:
                EnableMachineModules(5);
                break;

            default:
                break;
        }
    }

    void EnableMachineModules(int day)
    {
        for (int i = 0; i < machineEnabled.Length; i++)
            if (i + 1 == day)
                machineEnabled[i].SetActive(true);
    }
}
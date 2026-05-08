using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class DayManager : MonoBehaviour
{
    public Dec_Con HasEated;
    public Dec_Con HasNotEated;
    public Dec_Con HasWatchedTv;
    public Dec_Con HasRead;

    private void Update()
    {
        switch (GameManager.Instance.currentDay) 
        {
            case Day.Day1: 
                
                break;
        }
    }

    private void OnEnable()
    {
        SkipTimeButt.OnFirstDayButt += HourSkiped;
    }

    private void OnDisable()
    {
        SkipTimeButt.OnFirstDayButt -= HourSkiped;
    }

    void HourSkiped(bool daySkiped) 
    {
        if (GameManager.Instance.currentDay == Day.Day1) 
        {
            if (!HasEated)
            {
                ChangeRobotDialogue((int)DialoguesRobo.HasNotEated);
                Debug.Log(GameManager.Instance.robotBehaviour.dialogueSys.currentDialogue);
            }
            else if (!HasRead) 
            {
                ChangeRobotDialogue((int)DialoguesRobo.HasNotReaded);
            }
            else if (!HasWatchedTv) 
            {
                ChangeRobotDialogue((int)DialoguesRobo.HasNotWatchedTv);
            }
        }
    }

    void ChangeRobotDialogue(int id) 
    {
        GameManager.Instance.robotBehaviour.dialogueSys.ChargeDialogues(id);
    }

    public void StartDay() 
    {
        PresidentSpeaks();
    }

   void PresidentSpeaks() 
    {
        GameManager.Instance.presiSpeaker.PresiSpeaks();
    }
}

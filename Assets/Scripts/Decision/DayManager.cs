using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;

public class DayManager : MonoBehaviour
{
    public Dec_Con HasEated;
    public Dec_Con HasRead;
    public Dec_Con HasWatchedTv;

    bool watchTvTriggered = false;

    private void Update()
    {
        switch (GameManager.Instance.currentDay) 
        {
            case Day.Day1:
                if (HasWatchedTv.IsMet(GameManager.Instance.decManager) && !watchTvTriggered) 
                {
                    GameManager.Instance.presiSpeaker.ChangeDialogueID((int)PresiDialogue.GoToWorkDay1);
                    watchTvTriggered = true;
                    PresidentSpeaks();
                }
                break;
        }
    }

    private void OnEnable()
    {
        SkipTimeButt.OnFirstDayButt += HourSkiped;
        GameManager.OnChangeDay += RestartTriggers;
    }

    private void OnDisable()
    {
        SkipTimeButt.OnFirstDayButt -= HourSkiped;
        GameManager.OnChangeDay -= RestartTriggers;
    }

    void HourSkiped(bool daySkiped) 
    {
        if (GameManager.Instance.currentDay == Day.Day1) 
        {
            if (!HasEated.IsMet(GameManager.Instance.decManager))
            {
                ChangeRobotDialogue((int)DialoguesRobo.HasNotEated);
            }
            else if (!HasRead.IsMet(GameManager.Instance.decManager)) 
            {
                ChangeRobotDialogue((int)DialoguesRobo.HasNotReaded);
            }
            else if (!HasWatchedTv.IsMet(GameManager.Instance.decManager)) 
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

    public void RestartTriggers() 
    {
        watchTvTriggered = false;
    }
}

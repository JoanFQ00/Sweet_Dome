using UnityEngine;

public class DayManager : MonoBehaviour
{
    public Dec_Con hasEated;
    public Dec_Con hasRead;
    public Dec_Con hasWatchedTv;

    public bool watchTvTriggered = false;

    private void Update()
    {
        switch (GameManager.Instance.currentDay)
        {
            case Day.Day1:
                if (hasWatchedTv != null) 
                {
                    if (hasWatchedTv.IsMet(GameManager.Instance.decManager) && !watchTvTriggered)
                    {
                        GameManager.Instance.presiSpeaker.ChangeDialogueID((int)PresiDialogue.GoToWorkDay1);
                        watchTvTriggered = true;
                        PresidentSpeaks();
                    }
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
            if (!hasEated.IsMet(GameManager.Instance.decManager))
            {
                ChangeRobotDialogue((int)DialoguesRobo.HasNotEated);
            }
            else if (!hasRead.IsMet(GameManager.Instance.decManager)) 
            {
                ChangeRobotDialogue((int)DialoguesRobo.HasNotReaded);
            }
            else if (!hasWatchedTv.IsMet(GameManager.Instance.decManager)) 
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

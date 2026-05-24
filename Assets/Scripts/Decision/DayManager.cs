using UnityEngine;

public class DayManager : MonoBehaviour
{
    [Header("Day1Con")]
    public Dec_Con hasEated;
    public Dec_Con hasRead;
    public Dec_Con hasWatchedTv;
    public Dec_Con machineCompletedDay1;

    [Header("Day2Con")]
    public Dec_Con dorothyBook1Readed;
    public Dec_Con dorothyAngried;


    [Header("SafeChecks")]
    public bool hasEatedDay2 = false;
    public bool watchTvTriggered = false;
    public bool machineCompletedToday = false;
    public bool youReadDorothyBook = false;
    public bool dorothyAngry = false;

    [Header("Interactables")]
    public GameObject[] dayInt;

    private void OnEnable()
    {
        SkipTimeButt.OnFirstDayButt += HourSkiped;
        GameManager.OnChangeDay += RestartTriggers;
        GameManager.OnChangeDay += LoadInteractObjects;
    }

    private void OnDisable()
    {
        SkipTimeButt.OnFirstDayButt -= HourSkiped;
        GameManager.OnChangeDay -= RestartTriggers;
        GameManager.OnChangeDay -= LoadInteractObjects;
    }

    private void Update()
    {
        switch (GameManager.Instance.currentDay)
        {
            case Day.Day1:
                UpdateDay1();
            break;

            case Day.Day2:
                UpdateDay1();
            break;
        }
    }

    void UpdateDay1() 
    {
        if (hasWatchedTv != null)
        {
            if (hasWatchedTv.IsMet(GameManager.Instance.decManager) && !watchTvTriggered)
            {
                GameManager.Instance.presiSpeaker.ChangeDialogueID((int)PresiDialogue.GoToWorkDay1);
                watchTvTriggered = true;
                PresidentSpeaks();
            }

            if (machineCompletedDay1.IsMet(GameManager.Instance.decManager) && !machineCompletedToday)
            {
                GameManager.Instance.presiSpeaker.ChangeDialogueID((int)PresiDialogue.MachineCompleted);
                machineCompletedToday = true;
                PresidentSpeaks();
            }
        }
    }

    void UpdateDay2() 
    {
        if (hasEated.IsMet(GameManager.Instance.decManager) && !hasEatedDay2)
        {
            ChangeRobotDialogue((int)DialoguesRobo.DorothyRecomendsBook1);
            GameManager.Instance.robotBehaviour.testGoToPlayer = true;
            hasEatedDay2 = true;
        }

        //if (dorothyBook1Readed.IsMet(GameManager.Instance.decManager) && !youReadDorothyBook)
        //{
        //    ChangeRobotDialogue((int)DialoguesRobo.YouReadDorothyBook);
        //    GameManager.Instance.robotBehaviour.testGoToPlayer = true;
        //    youReadDorothyBook = true;
        //}

        //if (hasRead.IsMet(GameManager.Instance.decManager) && !dorothyAngry)
        //{
        //    ChangeRobotDialogue((int)DialoguesRobo.DorothyAngryDay2);
        //    GameManager.Instance.robotBehaviour.testGoToPlayer = true;
        //    dorothyAngry = true; 
        //}
    }

    void HourSkiped(bool daySkiped) 
    {
        if (GameManager.Instance.currentDay == Day.Day1) 
        {
            Day1HourSkip();
        }
        if (GameManager.Instance.currentDay == Day.Day2) 
        {
            Day1HourSkip();
        }
    }

    void Day1HourSkip() 
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
    void Day2HourSkip() 
    {
        if (!hasEated.IsMet(GameManager.Instance.decManager) && !hasEatedDay2)
        {
            ChangeRobotDialogue((int)DialoguesRobo.HasNotEatedDay2);
            GameManager.Instance.robotBehaviour.testGoToPlayer = true;
            hasEatedDay2 = true;
        }
        if (!hasRead.IsMet(GameManager.Instance.decManager) && !dorothyAngry)
        {
            ChangeRobotDialogue((int)DialoguesRobo.DorothyAngryDay2);
            GameManager.Instance.robotBehaviour.testGoToPlayer = true;
            dorothyAngry = true;
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

    public void RestartTriggers(Day currentDay) 
    {
        watchTvTriggered = false;
        machineCompletedToday = false;
        youReadDorothyBook = false;
        dorothyAngry = false;
    }

    private void LoadInteractObjects(Day currentDay) 
    {
        foreach (GameObject var in dayInt) 
        {
            var.SetActive(false);
        }

        dayInt[(int) currentDay].SetActive(true);
    }
}

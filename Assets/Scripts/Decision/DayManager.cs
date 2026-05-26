using System.Linq;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [Header("Day1Con")]
    public Dec_Con hasEated;
    public Dec_Con hasRead;
    public Dec_Con hasWatchedTv;
    public Dec_Con machineCompletedDay1;

    public float day1FinalTimer;
    private float currentDay1Time;

    [Header("Day2Con")]
    public Dec_Con dorothyBook1Readed;
    public Dec_Con dorothyAngried;
    public Dec_Con noteatedDay2;

    public float day2DorothyTimer = 5f;
    private float day2DorothyCurrentTime;


    [Header("SafeChecks")]
    public bool hasEated_SF = false;
    public bool watchTvTriggered = false;
    public bool machineCompletedToday = false;
    public bool youReadDorothyBook = false;
    public bool dorothyAngry = false;
    public bool dorothyExplainsButton = false;

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
                UpdateDay2();
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
        day2DorothyCurrentTime += Time.deltaTime;

        if (day2DorothyCurrentTime >= day2DorothyTimer && !dorothyExplainsButton) 
        {
            dorothyExplainsButton = true;
            ChangeRobotDialogue((int)DialoguesRobo.DorothyExplainsButton);
            GameManager.Instance.robotBehaviour.testGoToPlayer = true;
        }

        if (hasEated.IsMet(GameManager.Instance.decManager) && !hasEated_SF) 
        {
            hasEated_SF = true;
            ChangeRobotDialogue((int)DialoguesRobo.DorothyRecomendsBook1);
            GameManager.Instance.robotBehaviour.testGoToPlayer = true;
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
            Day2HourSkip();
        }
    }

    void Day1HourSkip() 
    {
        if (!hasEated.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.HasNotEated);
            GameManager.Instance.robotBehaviour.testGoToPlayer = true;
        }
        else if (!hasRead.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.HasNotReaded);
            GameManager.Instance.robotBehaviour.testGoToPlayer = true;
        }
        else if (!hasWatchedTv.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.HasNotWatchedTv);
            GameManager.Instance.robotBehaviour.testGoToPlayer = true;
        }
    }
    void Day2HourSkip() 
    {
        if (!hasEated.IsMet(GameManager.Instance.decManager) && !hasEated_SF)
        {
            ChangeRobotDialogue((int)DialoguesRobo.DorothyCongratsotNotEat);
            GameManager.Instance.robotBehaviour.testGoToPlayer = true;
            hasEated_SF = true;
        }
        else if (!hasRead.IsMet(GameManager.Instance.decManager) && !dorothyAngry)
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
        dorothyExplainsButton = false;
    }

    public void LoadInteractObjects(Day currentDay)
    {
        foreach (GameObject obj in dayInt)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        int index = (int)currentDay;

        if (index >= 0 && index < dayInt.Length && dayInt[index] != null)
        {
            dayInt[index].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"No interact objects configured for {currentDay}");
        }
    }

    public void FindInteractables()
    {
        dayInt = GameObject.FindGameObjectsWithTag("H_Interactables")
            .OrderBy(obj => obj.name)
            .ToArray();
    }
}

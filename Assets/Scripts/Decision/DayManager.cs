using NUnit.Framework;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

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
    public float day2DorothyTimer = 5f;
    private float day2DorothyCurrentTime;

    public Dec_Con dorothyBook1Readed;
    public Dec_Con dorothyAngriedDay2;


    [Header("Day3Con")] //Podria reutilizar el timer IKIK
    public float day3DorothyTimer = 5f;
    private float day3DorothyCurrentTime;

    public Dec_Con dorothyLaunchEated;

    [Header("ActionsToRemember")]
    public NDec_Act angriedDay2;
    public NDec_Act angriedDay3;

    [Header("SafeChecks")]
    public bool hasEated_SF = false;
    public bool watchTvTriggered = false;
    public bool machineCompletedToday = false;
    public bool youReadDorothyBook = false;
    public bool dorothyAngry = false;
    public bool dorothyExplainsButton = false;
    public bool afterTvDay2 = false;
    public bool dorothyTalksDay3 = false;
    public bool finishedEatin2 = false;

    [Header("Interactables")]
    public GameObject[] dayInt;

    [Header("DorothyBooks")]
    public List<GameObject> dorothyBooks;

    private void Start()
    {
        dorothyBooks = GameObject.FindGameObjectsWithTag("DorothyBooks").ToList();
        foreach (GameObject b in dorothyBooks) 
        {
            b.SetActive(false);
        }
    }


    private void OnEnable()
    {
        SkipTimeButt.OnFirstDayButt += HourSkiped;
        GameManager.OnChangeDay += RestartTriggers;
        GameManager.OnChangeDay += LoadInteractObjects;
        LevelManager.OnLevelLoaded += SceneLoaded;
    }

    private void OnDisable()
    {
        SkipTimeButt.OnFirstDayButt -= HourSkiped;
        GameManager.OnChangeDay -= RestartTriggers;
        GameManager.OnChangeDay -= LoadInteractObjects;
        LevelManager.OnLevelLoaded -= SceneLoaded;
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

            case Day.Day3:
                UpdateDay3();
            break;
        }
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

        if (GameManager.Instance.currentDay == Day.Day3) 
        {
            Day3HourSkip();
        }
    }

    void SceneLoaded() 
    {
        //dorothyBooks = GameObject.FindGameObjectsWithTag("DorothyBooks").ToList();

        //foreach (GameObject b in dorothyBooks)
        //{
        //    b.SetActive(false);
        //}
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
            ChangeRobotDialogue((int)DialoguesRobo.DorothyExplainsButton, true);
        }

        if (hasEated.IsMet(GameManager.Instance.decManager) && !hasEated_SF) 
        {
            hasEated_SF = true;
            ChangeRobotDialogue((int)DialoguesRobo.DorothyRecomendsBook1, true);

            dorothyBooks[dorothyBooks.Count() - 1].SetActive(true);
        }

        if (dorothyBook1Readed.IsMet(GameManager.Instance.decManager) && !youReadDorothyBook)
        {
            ChangeRobotDialogue((int)DialoguesRobo.YouReadDorothyBook, true);
            youReadDorothyBook = true;
        }
        else if (hasRead.IsMet(GameManager.Instance.decManager) && !dorothyAngry)
        {
            ChangeRobotDialogue((int)DialoguesRobo.DorothyAngryDay2, true);
            dorothyAngry = true;
            angriedDay2.ActionExecuted();
        }

        if ((afterTvDay2) && ((dorothyBook1Readed.IsMet(GameManager.Instance.decManager) && !watchTvTriggered) || 
            (dorothyAngriedDay2.IsMet(GameManager.Instance.decManager) && !watchTvTriggered)))
        {
            ChangeRobotDialogue((int)DialoguesRobo.DontWatchThatProgram, true);
            watchTvTriggered = true;
        }
    }

    void UpdateDay3() 
    {
        day3DorothyCurrentTime += Time.deltaTime;

        if (day3DorothyCurrentTime >= day3DorothyTimer && !dorothyTalksDay3)  
        {
            dorothyTalksDay3 = true;
            ChangeRobotDialogue((int)DialoguesRobo.DorothyLaunch, true);
        }

        if (dorothyLaunchEated.IsMet(GameManager.Instance.decManager) && !hasEated_SF)
        {
            hasEated_SF = true;
            ChangeRobotDialogue((int)DialoguesRobo.DorothyHappyLaunch, true);
        }
        else if (hasEated.IsMet(GameManager.Instance.decManager) && !hasEated_SF) 
        {
            hasEated_SF = true;
            ChangeRobotDialogue((int)DialoguesRobo.DorothyAngryDay3 , true);
        }

        if ((hasEated_SF && dorothyLaunchEated.IsMet(GameManager.Instance.decManager)) 
            && !finishedEatin2) 
        {
            finishedEatin2 = true;
            ChangeRobotDialogue((int)DialoguesRobo.RecomendsBook2, true);
        }
    }

    void Day1HourSkip() 
    {
        if (!hasEated.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.HasNotEated, true);
        }
        else if (!hasRead.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.HasNotReaded, true);
        }
        else if (!hasWatchedTv.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.HasNotWatchedTv, true);
        }
    }
    void Day2HourSkip() 
    {
        if (!hasEated.IsMet(GameManager.Instance.decManager) && !hasEated_SF)
        {
            ChangeRobotDialogue((int)DialoguesRobo.DorothyCongratsotNotEat, true);
            hasEated_SF = true;

            dorothyBooks[dorothyBooks.Count() - 1].SetActive(true);
        }
        else if (!hasRead.IsMet(GameManager.Instance.decManager) && !dorothyAngry)
        {
            ChangeRobotDialogue((int)DialoguesRobo.DorothyAngryDay2, true);
            dorothyAngry = true;
            angriedDay2.ActionExecuted();

        }
        else if (!hasWatchedTv.IsMet(GameManager.Instance.decManager) 
            && dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.DontWatchThatProgram, true);
            watchTvTriggered = true;
        }
        else if (!hasWatchedTv.IsMet(GameManager.Instance.decManager) 
            && dorothyAngriedDay2.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.MattewSturdy, true);
            watchTvTriggered = true;
        }
    }

    void Day3HourSkip() 
    {
        if (!hasEated.IsMet(GameManager.Instance.decManager)
            && dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
        {

        }
        else if (!hasEated.IsMet(GameManager.Instance.decManager)
            && !dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
        {

        }
    }

    void ChangeRobotDialogue(int id, bool goToPlayer) 
    {
        GameManager.Instance.robotBehaviour.dialogueSys.ChargeDialogues(id);
        GameManager.Instance.robotBehaviour.testGoToPlayer = goToPlayer;
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
        afterTvDay2 = false;   
        finishedEatin2 = false;
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

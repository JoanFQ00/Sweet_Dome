using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    private bool playedSitAnim = false;
    private bool playedTvAnim = false;

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

    public float timerToBeBop = 1f;
    float currentBeBopTimer;
    bool activatedBBTimer = false;

    public Dec_Con dorothyLaunchEated;
    public Dec_Con dorothyAngriedDay3;
    public Dec_Con dorothyBook2Readed;

    [Header("ActionsToRemember")]
    public NDec_Act angriedDay2;
    public NDec_Act angriedDay3;
    public NDec_Act dorothySaysWatchTv;

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
    public bool triedToSkipMachine = false;

    [Header("Day3 SafeChecks")]
    public bool checkedEatDay3 = false;
    public bool checkedReadDay3 = false;
    public bool checkedTvDay3 = false;

    [Header("Interactables")]
    public Action_Interactable[] dayInt;
    public GameObject dorothyLaunch;

    [Header("DorothyBooks")]
    private GameObject dorothyBook;

    private IEnumerator Start()
    {
        yield return null;

        AssignReferences();
        FindInteractables();
        LoadInteractObjects(GameManager.Instance.currentDay);
    }


    void AssignReferences()
    {
        if (SceneManager.GetActiveScene().name != "Gameplay")
            return;

        GameObject[] objs = GameObject.FindGameObjectsWithTag("DorothyBooks");

        foreach (GameObject obj in objs)
        {
            if (obj.CompareTag("DorothyLaunch"))
            {
                dorothyLaunch = obj;
            }

            if (obj.CompareTag("DorothyBooks"))
            {
                dorothyBook = obj;
                dorothyBook.SetActive(false);
            }
        }
    }


    private void OnEnable()
    {
        SkipTimeButt.OnFirstDayButt += HourSkiped;
        GameManager.OnChangeDay += RestartTriggers;
        GameManager.OnChangeDay += LoadInteractObjects;
        LevelManager.OnLevelLoaded += SceneLoaded;
        PresiSys.OnDialogueTerminated += CheckPresiTerminatedDialogue;
    }

    private void OnDisable()
    {
        SkipTimeButt.OnFirstDayButt -= HourSkiped;
        GameManager.OnChangeDay -= RestartTriggers;
        GameManager.OnChangeDay -= LoadInteractObjects;
        LevelManager.OnLevelLoaded -= SceneLoaded;
        PresiSys.OnDialogueTerminated -= CheckPresiTerminatedDialogue;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "Gameplay")
            return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            GameManager.Instance.dayTime.SetCurrentTime(15, 0);
        }

        Timers();

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

            case Day.Day4:
                break;

            case Day.Day5:
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
        AssignReferences();
        FindInteractables();
        LoadInteractObjects(GameManager.Instance.currentDay);
    }

    void Timers()
    {
        if (activatedBBTimer)
        {
            currentBeBopTimer += Time.deltaTime;

            if (currentBeBopTimer >= timerToBeBop)
            {
                PresidentSpeaks(PresiDialogue.DorothySights);
                activatedBBTimer = false;
            }
        }
    }

    void UpdateDay1()
    {
        if (hasWatchedTv != null)
        {
            if (hasWatchedTv.IsMet(GameManager.Instance.decManager) && !watchTvTriggered)
            {
                PresidentSpeaks(PresiDialogue.GoToWorkDay1);
                watchTvTriggered = true;
            }

            if (machineCompletedDay1.IsMet(GameManager.Instance.decManager) && !machineCompletedToday)
            {
                PresidentSpeaks(PresiDialogue.MachineCompleted);
                machineCompletedToday = true;
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

            dorothyBook.SetActive(true);
            // IMPORTANT: Has de pensar que en realitat li has de pasar un INT (pero per com funciona el codi de canvi de dia)
            // rep lenum, igualment els enums son ints so...
            dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day1);
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
            watchTvTriggered = true;
            activatedBBTimer = true;
            ChangeRobotDialogue((int)DialoguesRobo.DontWatchThatProgram, true);
        }

        if (hasWatchedTv.IsMet(GameManager.Instance.decManager) && !watchTvTriggered)
        {
            watchTvTriggered = true;
            activatedBBTimer = true;
            PresidentSpeaks(PresiDialogue.GoToWorkDay2);
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

        if (!hasEated_SF && hasEated.IsMet(GameManager.Instance.decManager)
            && !dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
        {
            hasEated_SF = true;
            ChangeRobotDialogue((int)DialoguesRobo.Angry3AndNoBook, true);
            angriedDay3.ActionExecuted();
        }
        else if (!hasEated_SF && hasEated.IsMet(GameManager.Instance.decManager) && !hasEated_SF)
        {
            hasEated_SF = true;
            ChangeRobotDialogue((int)DialoguesRobo.DorothyAngryDay3, true);
            dorothyBook.SetActive(true);
            dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day2);
        }
        else if (!hasEated_SF && dorothyLaunchEated.IsMet(GameManager.Instance.decManager) && !hasEated_SF)
        {
            hasEated_SF = true;
            ChangeRobotDialogue((int)DialoguesRobo.DorothyHappyLaunch, true);
            dorothyBook.SetActive(true);
            dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day2);
        }

        if (hasRead.IsMet(GameManager.Instance.decManager) && dorothyLaunchEated.IsMet(GameManager.Instance.decManager))
        {
            // Dorothy Se sienta y mira a matthew
            GameManager.Instance.robotBehaviour.TriggerSitAnimation();
        }

        if (hasWatchedTv.IsMet(GameManager.Instance.decManager) &&
            dorothyBook2Readed.IsMet(GameManager.Instance.decManager))
        {
            // Dorothy Desconecta la television
            GameManager.Instance.robotBehaviour.TriggerTurnOffTvAnimation();
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

        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager) && triedToSkipMachine)
        {
            // Numero de final i temps fins que s'executa per si has de ficar algun dialeg
            GameManager.Instance.ExecuteFinishAnimation(0, 2);
        }
        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.TryToSkipMachineDay1, true);
            triedToSkipMachine = true;
        }
    }
    void Day2HourSkip()
    {
        if (!hasEated.IsMet(GameManager.Instance.decManager) && !hasEated_SF)
        {
            ChangeRobotDialogue((int)DialoguesRobo.DorothyCongratsotNotEat, true);
            hasEated_SF = true;

            dorothyBook.SetActive(true);
            dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day1);
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
            activatedBBTimer = true;
        }
        else if (!hasWatchedTv.IsMet(GameManager.Instance.decManager)
            && dorothyAngriedDay2.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.MattewSturdy, true);
            watchTvTriggered = true;
            activatedBBTimer = true;
        }
        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager) && triedToSkipMachine)
        {
            PresidentSpeaks(PresiDialogue.MattewTraitorDay1);
            GameManager.Instance.ExecuteFinishAnimation(0, 6);
        }
        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.TryToSkipMachineDay1, true);
            triedToSkipMachine = true;
        }
    }

    void Day3HourSkip()
    {
        if (!checkedEatDay3)
        {
            checkedEatDay3 = true;

            if (!hasEated.IsMet(GameManager.Instance.decManager)
                && dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
            {
                hasEated_SF = true;
                ChangeRobotDialogue((int)DialoguesRobo.DorothyAngryDay3, true);
                dorothyBook.SetActive(true);
                dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day2);
                return;
            }
            else if (!hasEated.IsMet(GameManager.Instance.decManager)
                && !dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
            {
                hasEated_SF = true;
                ChangeRobotDialogue((int)DialoguesRobo.Angry3AndNoBook, true);
                angriedDay3.ActionExecuted();
                return;
            }
        }

        if (!checkedReadDay3)
        {
            checkedReadDay3 = true;

            if (!hasRead.IsMet(GameManager.Instance.decManager)
                && dorothyBook.activeSelf)
            {
                ChangeRobotDialogue((int)DialoguesRobo.HasNotReaded, true);
                return;
            }
            else if (!hasRead.IsMet(GameManager.Instance.decManager)
                && dorothyAngriedDay2.IsMet(GameManager.Instance.decManager)
                && dorothyAngriedDay3.IsMet(GameManager.Instance.decManager))
            {
                ChangeRobotDialogue((int)DialoguesRobo.HasNotReaded, true);
            }
        }

        if (!checkedTvDay3)
        {
            checkedTvDay3 = true;

            if (!hasWatchedTv.IsMet(GameManager.Instance.decManager)
                && dorothyAngriedDay2.IsMet(GameManager.Instance.decManager)
                && dorothyAngriedDay3.IsMet(GameManager.Instance.decManager))
            {
                dorothySaysWatchTv.ActionExecuted();
                return;
            }
        }

        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager) && triedToSkipMachine)
        {
            PresidentSpeaks(PresiDialogue.MattewTraitorDay1);
            if (!playedSitAnim)
            {
                GameManager.Instance.robotBehaviour.TriggerSitAnimation();
                playedSitAnim = true;
            }
        }
        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager))
        {
            ChangeRobotDialogue((int)DialoguesRobo.TryToSkipMachineDay1, true);
            triedToSkipMachine = true;
            if (!playedTvAnim)
            {
                GameManager.Instance.robotBehaviour.TriggerTurnOffTvAnimation();
                playedTvAnim = true;
            }
        }
    }

    void CheckPresiTerminatedDialogue()
    {
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.DorothySights)
        {
            ChangeRobotDialogue((int)DialoguesRobo.DorothyMissesBeBop, false);
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

    void PresidentSpeaks(PresiDialogue dialogue)
    {
        GameManager.Instance.presiSpeaker.PresiSpeaks((int)dialogue);
    }

    public void RestartTriggers(Day currentDay)
    {
        watchTvTriggered = false;
        machineCompletedToday = false;
        youReadDorothyBook = false;
        dorothyAngry = false;
        dorothyExplainsButton = false;
        hasEated_SF = false;
        afterTvDay2 = false;
        finishedEatin2 = false;
        triedToSkipMachine = false;
    }

    public void LoadInteractObjects(Day currentDay)
    {
        foreach (Action_Interactable obj in dayInt)
        {
            if (!obj.keepBetweenDays)
            {
                bool shouldBeActive = (obj.DayLoad == currentDay);
                obj.gameObject.SetActive(shouldBeActive);
            }
        }

        int index = (int)currentDay;

        if (index >= 0 && index < dayInt.Length && dayInt[index] != null)
        {
            dayInt[index].gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"No interact objects configured for {currentDay}");
        }
    }

    public void FindInteractables()
    {
        dayInt = FindObjectsByType<Action_Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }
}

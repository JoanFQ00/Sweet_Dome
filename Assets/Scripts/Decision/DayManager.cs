using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    [Header("Day1Con")]
    public Dec_Con hasEated;
    public Dec_Con hasRead;
    public Dec_Con hasWatchedTv;
    public Dec_Con machineCompletedDay1;

    public float day1FinalTimer = 60f;
    private float currentDay1Time;
    bool moveTimerCheck = true;

    [Header("Day2Con")]
    public float day2DorothyTimer = 5f;
    private float day2DorothyCurrentTime;

    public Dec_Con dorothyBook1Readed;
    public Dec_Con dorothyAngriedDay2;

    [Header("Day3Con")]
    public float dayLaunchDorothyTimer = 5f;
    private float dayLaunchDorothyCurrentTime;

    public float timerToBeBop = 1f;
    float currentBeBopTimer;
    bool activatedBBTimer = false;

    public Dec_Con dorothyLaunchEated;
    public Dec_Con dorothyAngriedDay3;
    public Dec_Con dorothyBook2Readed;

    [Header("Day4Con")]
    public Dec_Con dorothyLaunch2;
    public Dec_Con hasReadDorothyBook3;

    [Header("Day5Con")]
    public Dec_Con hasReadDorothyBook4;

    [Header("ActionsToRemember")]
    public NDec_Act angriedDay2;
    public NDec_Act angriedDay3;
    public NDec_Act dorothySaysWatchTv;
    public NDec_Act disconectTV;

    [Header("Day Actions")]
    public NDec_Act eatAction;
    public NDec_Act readAction;
    public NDec_Act watchTVAction;

    [Header("Branches")]
    public bool D11 = false;
    public bool D12 = false;
    public bool D21 = false;
    public bool D22 = false;
    public bool D31 = false;
    public bool notLaunchDay4 = false;
    public bool D32 = false;
    public bool D41 = false;
    public bool D42 = false;
    public bool D51 = false;
    public bool D52 = false;
    public bool D53 = false;
    public bool D61 = false;
    public bool D62 = false;
    public bool D71 = false;
    public bool D72 = false;
    public bool D81 = false;
    public bool D82 = false;
    public bool D83 = false;
    public bool D84 = false;
    public bool dorothyWaiting = false;

    [Header("SafeChecks")]
    public bool hasEated_SC = false;
    public bool watchTvTriggered = false;
    public bool machineCompletedToday = false;
    public bool youReadDorothyBook = false;
    public bool dorothyAngry = false;
    public bool dorothyExplainsButton = false;
    public bool afterTvDay2 = false;
    public bool dorothyTalksAboutLaunch = false;
    public bool finishedEatin2 = false;
    public bool triedToSkipMachine = false;

    [Header("Day3 SafeChecks")]
    public bool checkedEatDay3 = false;
    public bool checkedReadDay3 = false;
    public bool checkedTvDay3 = false;
    public bool hasTriedToSkipTVDay3;

    [Header("Day4 SafeChecks")]
    public bool checkedReadD4 = false;
    public bool trySkipEatingD4 = false;

    [Header("Day5 SafeChecks")]
    public bool checkReadDay5;
    public bool makeDorothySad = false;
    public bool dorothyWaitsForInteraction = false;

    [Header("Animation SafeCheck")]
    private bool watchWindowCheck = false;

    [Header("Interactables")]
    public Action_Interactable[] dayInt;
    public GameObject dorothyLaunch;

    [Header("DorothyBooks")]
    public GameObject dorothyBook;

    public static event Action<string> OnRequestCinematic;
    public static Action OnTurnOffTV;

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
        GameManager.OnChangeDay += RestartTriggersAndTimers;
        GameManager.OnChangeDay += LoadInteractObjects;
        GameManager.NewDay += GoodMorningPresi;
        LevelManager.OnLevelLoaded += SceneLoaded;
        PresiSys.OnDialogueTerminated += CheckPresiTerminatedDialogue;
        GameManager.NewDay += BookLaunchActChange;
        PlayerController.OnMovedForFirstTime += StopTimer;
        EndingController.BackToMenu += HardReset;
    }

    private void OnDisable()
    {
        SkipTimeButt.OnFirstDayButt -= HourSkiped;
        GameManager.OnChangeDay -= RestartTriggersAndTimers;
        GameManager.OnChangeDay -= LoadInteractObjects;
        GameManager.NewDay -= GoodMorningPresi;
        LevelManager.OnLevelLoaded -= SceneLoaded;
        PresiSys.OnDialogueTerminated -= CheckPresiTerminatedDialogue;
        GameManager.NewDay -= BookLaunchActChange;
        PlayerController.OnMovedForFirstTime -= StopTimer;
        EndingController.BackToMenu -= HardReset;
    }

    void StopTimer()
    {
        moveTimerCheck = false;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "Gameplay")
            return;

        //if (Input.GetKeyDown(KeyCode.P)) 
        //{
        //    D32 = true;
        //}

        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    GameManager.Instance.dayTime.SetCurrentTime(15, 0);
        //}

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
                UpdateDay4();
                break;

            case Day.Day5:
                UpdateDay5();
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
        if (GameManager.Instance.currentDay == Day.Day1 &&
            moveTimerCheck) 
        {
            currentDay1Time += Time.deltaTime;
            if (currentDay1Time >= day1FinalTimer) 
            {
                moveTimerCheck = false;
                currentDay1Time = 0;
                GameManager.Instance.ExecuteFinishAnimation(1, 3);
            }
        }

        if (activatedBBTimer)
        {
            currentBeBopTimer += Time.deltaTime;

            if (currentBeBopTimer >= timerToBeBop)
            {
                PresidentSpeaks(PresiDialogue.DorothySights);
                OnRequestCinematic?.Invoke("Day5WatchWindow");
                ChangeDorothyDialogue((int)DialoguesRobo.DorothyMissesBeBop, false);
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
                GameManager.Instance.dayTime.LerpToTime(15, 0, 3);
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
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyExplainsButton, true);
        }

        if (hasEated.IsMet(GameManager.Instance.decManager) && !hasEated_SC)
        {
            hasEated_SC = true;
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyRecomendsBook1, true);

            dorothyBook.SetActive(true);
            dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day1);
        }

        if (dorothyBook1Readed.IsMet(GameManager.Instance.decManager) && !youReadDorothyBook)
        {
            ChangeDorothyDialogue((int)DialoguesRobo.YouReadDorothyBook, true);
            youReadDorothyBook = true;

            D11 = true;
        }
        else if (hasRead.IsMet(GameManager.Instance.decManager) && !dorothyAngry)
        {
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyAngryDay2, true);
            dorothyAngry = true;
            angriedDay2.ActionExecuted();

            D12 = true;
        }

        if (afterTvDay2 && ((dorothyBook1Readed.IsMet(GameManager.Instance.decManager) && !watchTvTriggered) ||
            (dorothyAngriedDay2.IsMet(GameManager.Instance.decManager) && !watchTvTriggered)))
        {
            OnRequestCinematic?.Invoke("Day5WatchWindow");
            watchTvTriggered = true;
            ChangeDorothyDialogue((int)DialoguesRobo.DontWatchThatProgram, false);
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
        dayLaunchDorothyCurrentTime += Time.deltaTime;

        if (dayLaunchDorothyCurrentTime >= dayLaunchDorothyTimer && !dorothyTalksAboutLaunch)
        {
            dorothyTalksAboutLaunch = true;
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyLaunch, true);
        }

        if (!hasEated_SC && hasEated.IsMet(GameManager.Instance.decManager)
            && !dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
        {
            hasEated_SC = true;
            ChangeDorothyDialogue((int)DialoguesRobo.Angry3AndNoBook, true);
            angriedDay3.ActionExecuted();

            checkedEatDay3 = true;
            D22 = true;
        }
        else if (!hasEated_SC && hasEated.IsMet(GameManager.Instance.decManager) && !hasEated_SC)
        {
            hasEated_SC = true;
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyAngryDay3, true);
            dorothyBook.SetActive(true);
            dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day2);

            checkedEatDay3 = true;
            D22 = true;
        }
        else if (!hasEated_SC && dorothyLaunchEated.IsMet(GameManager.Instance.decManager))
        {
            hasEated_SC = true;
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyHappyLaunch, true);
            dorothyBook.SetActive(true);
            dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day2);

            checkedEatDay3 = true;
            D21 = true;
        }

        if ((hasRead.IsMet(GameManager.Instance.decManager) || dorothyBook2Readed.IsMet(GameManager.Instance.decManager))&& 
            dorothyLaunchEated.IsMet(GameManager.Instance.decManager))
        {
            if (!checkedEatDay3)
            {
                checkedEatDay3 = true;
                OnRequestCinematic?.Invoke("Day3SitAndWatch");
                OnTurnOffTV?.Invoke();

                D31 = true;
            }
        }
    }

    void UpdateDay4() 
    {
        dayLaunchDorothyCurrentTime += Time.deltaTime;

        if (dayLaunchDorothyCurrentTime >= dayLaunchDorothyTimer && !dorothyTalksAboutLaunch)
        {
            dorothyTalksAboutLaunch = true;
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyLaunch, true);
        }

        if (D31)
        {
            if (!hasEated_SC && dorothyLaunch2.IsMet(GameManager.Instance.decManager))
            {
                ChangeDorothyDialogue((int)DialoguesRobo.DorothyHappyLaunch, true);
                hasEated_SC = true;
                dorothyBook.SetActive(true);
                dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day3);

                OnRequestCinematic("Day4TurnOffTv");
                OnTurnOffTV?.Invoke();
            }

            if (hasRead.IsMet(GameManager.Instance.decManager)
               && !checkedReadD4 && notLaunchDay4)
            {
                checkedReadD4 = true;
                ChangeDorothyDialogue((int)DialoguesRobo.MadDoro, true);
                GameManager.Instance.dayTime.SetCurrentTime(15, 0);
                D52 = true;
            }

            else if ((hasRead.IsMet(GameManager.Instance.decManager) || hasReadDorothyBook3.IsMet(GameManager.Instance.decManager))
                && !checkedReadD4)
            {
                checkedReadD4 = true;
                OnRequestCinematic?.Invoke("Day4Hug");
                D51 = true;
            }


        }
        else if (D32)
        {
            if (!hasEated_SC && dorothyLaunchEated.IsMet(GameManager.Instance.decManager))
            {
                hasEated_SC = true;
                OnRequestCinematic?.Invoke("Day4TurnOffTv");
                OnTurnOffTV?.Invoke();
                D31 = true;
            }
        }
    }

    void UpdateDay5() 
    {
        if (D71)
        {
            if (D53)
            {
                D72 = true;
                D71 = false;
            }
            else 
            {
                if (D61)
                {
                    dayLaunchDorothyCurrentTime += Time.deltaTime;

                    if (dayLaunchDorothyCurrentTime >= dayLaunchDorothyTimer && !dorothyTalksAboutLaunch)
                    {
                        dorothyTalksAboutLaunch = true;
                        ChangeDorothyDialogue((int)DialoguesRobo.DorothyLaunch, true);
                    }

                    if (!hasEated_SC && dorothyLaunch2.IsMet(GameManager.Instance.decManager))
                    {
                        ChangeDorothyDialogue((int)DialoguesRobo.LastBookRec, true);
                        hasEated_SC = true;
                        dorothyBook.SetActive(true);
                        dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day4);
                    }

                    if (!checkReadDay5 && hasReadDorothyBook4.IsMet(GameManager.Instance.decManager)) 
                    {
                        OnRequestCinematic?.Invoke("Day5LongHug");
                        dorothyWaitsForInteraction = true;

                        checkReadDay5 = true;
                        ChangeDorothyDialogue((int)DialoguesRobo.FinalRevelation, false);
                        dorothyWaiting = true;
                    }
                }
                if (D62) 
                {
                    dayLaunchDorothyCurrentTime += Time.deltaTime;

                    if (dayLaunchDorothyCurrentTime >= dayLaunchDorothyTimer && !dorothyTalksAboutLaunch)
                    {
                        dorothyTalksAboutLaunch = true;
                        ChangeDorothyDialogue((int)DialoguesRobo.AngriedAndNoLaunch, true);
                        dorothyLaunch.SetActive(false);
                    }

                    if (!checkReadDay5 && hasRead.IsMet(GameManager.Instance.decManager)) 
                    {
                        checkReadDay5 = true;

                        ChangeDorothyDialogue((int)DialoguesRobo.AngriedAndNoLaunch, true);
                        D83 = true;
                    }
                }
            }
            
        }
        else if (D72)
        {
            if (!hasEated_SC && hasEated.IsMet(GameManager.Instance.decManager)) 
            {
                if (!D53) 
                {
                    D71 = true;
                }
            }
        }
    }

    void Day1HourSkip()
    {
        if (!hasEated.IsMet(GameManager.Instance.decManager))
        {
            ChangeDorothyDialogue((int)DialoguesRobo.HasNotEated, true);
        }
        else if (!hasRead.IsMet(GameManager.Instance.decManager))
        {
            ChangeDorothyDialogue((int)DialoguesRobo.HasNotReaded, true);
        }
        else if (!hasWatchedTv.IsMet(GameManager.Instance.decManager))
        {
            ChangeDorothyDialogue((int)DialoguesRobo.HasNotWatchedTv, true);
        }

        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager) && triedToSkipMachine)
        {
            GameManager.Instance.ExecuteFinishAnimation(0, 2);
        }
        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager))
        {
            ChangeDorothyDialogue((int)DialoguesRobo.TryToSkipMachineDay1, true);
            triedToSkipMachine = true;
        }
    }
    void Day2HourSkip()
    {
        if (!hasEated.IsMet(GameManager.Instance.decManager) && !hasEated_SC)
        {
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyCongratsotNotEat, true);
            hasEated_SC = true;
            eatAction.ActionExecuted();

            dorothyBook.SetActive(true);
            dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day1);
        }
        else if ((!hasRead.IsMet(GameManager.Instance.decManager) && !dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
            && !dorothyAngry)
        {
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyAngryDay2, true);
            readAction.ActionExecuted();

            dorothyAngry = true;
            angriedDay2.ActionExecuted();

            D12 = true;
        }
        else if (!watchTvTriggered && !hasWatchedTv.IsMet(GameManager.Instance.decManager)
            && dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
        {
            ChangeDorothyDialogue((int)DialoguesRobo.DontWatchThatProgram, false);
            OnRequestCinematic?.Invoke("Day5WatchWindow");
            watchTVAction.ActionExecuted();

            watchTvTriggered = true;
            activatedBBTimer = true;
        }
        else if (!watchTvTriggered && !hasWatchedTv.IsMet(GameManager.Instance.decManager)
            && dorothyAngriedDay2.IsMet(GameManager.Instance.decManager))
        {
            ChangeDorothyDialogue((int)DialoguesRobo.MattewSturdy, false);
            OnRequestCinematic?.Invoke("Day5WatchWindow");
            watchTVAction.ActionExecuted();

            watchTvTriggered = true;
            activatedBBTimer = true;
        }
        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager) && triedToSkipMachine)
        {
            GameManager.Instance.ExecuteFinishAnimation(0, 2);
        }
        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager))
        {
            ChangeDorothyDialogue((int)DialoguesRobo.TryToSkipMachineDay1, true);
            triedToSkipMachine = true;
        }
    }

    void Day3HourSkip()
    {
        Debug.Log("Here");
        if (!checkedEatDay3)
        {
            Debug.Log("Here2");
            checkedEatDay3 = true;

            if (!hasEated.IsMet(GameManager.Instance.decManager)
                && dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
            {
                hasEated_SC = true;
                ChangeDorothyDialogue((int)DialoguesRobo.DorothyAngryDay3, true);
                dorothyBook.SetActive(true);
                dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day2);
                return;
            }
            else if (!hasEated.IsMet(GameManager.Instance.decManager)
                && !dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
            {
                hasEated_SC = true;
                ChangeDorothyDialogue((int)DialoguesRobo.Angry3AndNoBook, true);
                angriedDay3.ActionExecuted();
                return;
            }
        }

        if (!checkedReadDay3)
        {
            checkedReadDay3 = true;
            Debug.Log("Here3");

            if (!hasRead.IsMet(GameManager.Instance.decManager)
              && dorothyAngriedDay2.IsMet(GameManager.Instance.decManager)
              && dorothyAngriedDay3.IsMet(GameManager.Instance.decManager))
            {
                ChangeDorothyDialogue((int)DialoguesRobo.HasNotReaded, true);
            }
            else if (!hasRead.IsMet(GameManager.Instance.decManager)
                && dorothyBook.activeSelf)
            {
                ChangeDorothyDialogue((int)DialoguesRobo.HasNotReaded, true);
                OnRequestCinematic("Day4TurnOffTv");
                OnTurnOffTV?.Invoke();
                return;
            }
          
        }

        if (!checkedTvDay3)
        {
            checkedTvDay3 = true;

            if (!hasTriedToSkipTVDay3)
            {
                hasTriedToSkipTVDay3 = true;

                if (!hasWatchedTv.IsMet(GameManager.Instance.decManager)
                    && dorothyAngriedDay2.IsMet(GameManager.Instance.decManager)
                    && dorothyAngriedDay3.IsMet(GameManager.Instance.decManager))
                {
                    ChangeDorothyDialogue((int)DialoguesRobo.EncourageTV, true);
                    dorothySaysWatchTv.ActionExecuted();
                    return;
                }
            }
            else 
            {
                GameManager.Instance.dayTime.LerpToTime(15, 0);
                GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Completa la máquina");
                GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.GoToWorkDay2);
            }
        }

        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager) && triedToSkipMachine)
        {
            PresidentSpeaks(PresiDialogue.MattewTraitorDay1);
            GameManager.Instance.ExecuteFinishAnimation(0, 6);
        }
        else if (hasWatchedTv.IsMet(GameManager.Instance.decManager))
        {
            ChangeDorothyDialogue((int)DialoguesRobo.TryToSkipMachineDay1, true);
            triedToSkipMachine = true;
        }

    }

    void CheckPresiTerminatedDialogue()
    {
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.DorothySights)
        {
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyMissesBeBop, false);
        }
    }

    public void ChangeDorothyDialogue(int id, bool goToPlayer)
    {
        GameManager.Instance.robotBehaviour.dialogueSys.ChargeDialogues(id);
        GameManager.Instance.robotBehaviour.testGoToPlayer = goToPlayer;
    }

    public void ChargeFinalAnimation() 
    {
        OnRequestCinematic("FinalCinematic");
    }

    public void StartDay()
    {
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Gameplay"))
            return;

        PresidentSpeaks();
    }

    void PresidentSpeaks()
    {
        GameManager.Instance.presiSpeaker.PresiSpeaks();
    }

    public void PresidentSpeaks(PresiDialogue dialogue)
    {
        GameManager.Instance.presiSpeaker.PresiSpeaks((int)dialogue);
    }

    public void RestartTriggersAndTimers(Day currentDay)
    {
        watchTvTriggered = false;
        machineCompletedToday = false;
        youReadDorothyBook = false;
        dorothyAngry = false;
        dorothyExplainsButton = false;
        hasEated_SC = false;
        afterTvDay2 = false;
        finishedEatin2 = false;
        triedToSkipMachine = false;
        dorothyTalksAboutLaunch = false;

        dayLaunchDorothyCurrentTime = 0;
    }

    void HardReset() 
    {
        D11 = false;
        D12 = false;
        D21 = false;
        D22 = false;
        D31 = false;
        notLaunchDay4 = false;
        D32 = false;
        D41 = false;
        D42 = false;
        D51 = false;
        D52 = false;
        D53 = false;
        D61 = false;
        D62 = false;
        D71 = false;
        D72 = false;
        D81 = false;
        D82 = false;
        D83 = false;
        D84 = false;
        dorothyWaiting = false;

       hasEated_SC = false;
       watchTvTriggered = false;
       machineCompletedToday = false;
       youReadDorothyBook = false;
       dorothyAngry = false;
       dorothyExplainsButton = false;
       afterTvDay2 = false;
       dorothyTalksAboutLaunch = false;
       finishedEatin2 = false;
       triedToSkipMachine = false;

       checkedEatDay3 = false;
       checkedReadDay3 = false;
       checkedTvDay3 = false;
       hasTriedToSkipTVDay3 = false;

       checkedReadD4 = false;
       trySkipEatingD4 = false;

       checkReadDay5 = false;
       makeDorothySad = false;
       dorothyWaitsForInteraction = false;

       watchWindowCheck = false;
}

    public void LoadInteractObjects(Day currentDay)
    {
        // Convertimos el enum del día actual a su valor numérico entero (ej: Day1 = 0, Day2 = 1...)
        int currentDayIndex = (int)currentDay;

        foreach (Action_Interactable obj in dayInt)
        {
            if (obj == null) continue;

            if (!obj.keepBetweenDays)
            {
                // El objeto se activa si su día asignado es exactamente el día actual
                bool shouldBeActive = (obj.DayLoad == currentDay);
                obj.gameObject.SetActive(shouldBeActive);
            }
            else
            {
                // Convertimos el día del objeto a número. Si es menor o igual al día actual,
                // significa que pertenece a un día pasado o al presente y se queda encendido.
                int objectDayIndex = (int)obj.DayLoad;
                obj.gameObject.SetActive(objectDayIndex <= currentDayIndex);
            }
        }

        if (dorothyLaunch != null)
        {
            if (GameManager.Instance.currentDay == Day.Day3 ||
                GameManager.Instance.currentDay == Day.Day4)
            {
                dorothyLaunch.SetActive(true);
            }
            else
                dorothyLaunch.SetActive(false);
        }
    }

    public void FindInteractables()
    {
        dayInt = FindObjectsByType<Action_Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    void GoodMorningPresi()
    {
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Gameplay"))
            return;

        switch (GameManager.Instance.currentDay) 
        {
            //case Day.Day2:
            //    PresidentSpeaks(PresiDialogue.GoodMorningDay1);
            //break;

            //case Day.Day3:
            //    PresidentSpeaks(PresiDialogue.GoodMorningDay1);
            //break;
            
            case Day.Day1:
                PresidentSpeaks(PresiDialogue.GoodMorningDay001);
            break;
            
            case Day.Day2:
                PresidentSpeaks(PresiDialogue.GoodMorningDay002);
            break;

            case Day.Day3:
                PresidentSpeaks(PresiDialogue.GoodMorningDay003);
            break;

            case Day.Day4:
                if (D41)
                {
                    PresidentSpeaks(PresiDialogue.GoodMornDayHappy);
                }
                else 
                {
                    PresidentSpeaks(PresiDialogue.GoodMorNoHappy);
                }
                break;

            case Day.Day5:
                if (D71)
                {
                    PresidentSpeaks(PresiDialogue.GoodMorDay5);
                }
                else 
                {
                    PresidentSpeaks(PresiDialogue.BadMorDay5);
                }
                break;
        }
    }

    void BookLaunchActChange()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Gameplay"))
        {
            Action_Reader readerCode = dorothyBook.GetComponent<Action_Reader>();
            Action_Eat eatCode = dorothyLaunch.GetComponent<Action_Eat>();

            switch (GameManager.Instance.currentDay)
            {
                case Day.Day3:
                    NDec_Act newAct = Resources.Load<NDec_Act>("Actions/DorothyBook2");
                    readerCode.act = newAct;
                break;

                case Day.Day4:
                    NDec_Act newAct2 = Resources.Load<NDec_Act>("Actions/DorothyBook3");
                    NDec_Act eatAct2 = Resources.Load<NDec_Act>("Actions/DorothyLaunchD4");
                    readerCode.act = newAct2;
                break;

                case Day.Day5:
                    NDec_Act newAct3 = Resources.Load<NDec_Act>("Actions/DorothyBook4");

                break;
            }
        }
    }
}
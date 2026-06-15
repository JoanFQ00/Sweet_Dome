using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

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

    [Header("Day3Con")] //Podria reutilizar el timer IKIK
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

    [Header("Branches")]
    public bool D11 = false;
    public bool D12 = false;
    public bool D21 = false;
    public bool D22 = false;
    public bool D31 = false;
    public bool notLaunchDay4;
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

    [Header("Interactables")]
    public Action_Interactable[] dayInt;
    public GameObject dorothyLaunch;

    [Header("DorothyBooks")]
    public GameObject dorothyBook;

    [Header("Cinematics Settings (Day 3)")]
    public PlayableDirector day3Timeline;
    private bool cinematicStarted = false;
    private bool cinematicEndedTriggered = false;

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
        PlayerController.OnMovedForFirstTime += () => moveTimerCheck = false;
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
        PlayerController.OnMovedForFirstTime -= () => moveTimerCheck = false;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "Gameplay")
            return;

        if (Input.GetKeyDown(KeyCode.P)) 
        {
            D32 = true;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            GameManager.Instance.dayTime.SetCurrentTime(15, 0);
        }

        Timers();

        if (cinematicStarted && !cinematicEndedTriggered && day3Timeline != null)
        {
            if (day3Timeline.state != PlayState.Playing || day3Timeline.time >= day3Timeline.duration)
            {
                cinematicEndedTriggered = true;
                TerminarCinematicaYVolverAlJuego();
            }
        }

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
            Debug.Log(currentDay1Time);
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

                // - Cinematica Dorothy va a la ventana 
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

        if ((afterTvDay2) && ((dorothyBook1Readed.IsMet(GameManager.Instance.decManager) && !watchTvTriggered) ||
            (dorothyAngriedDay2.IsMet(GameManager.Instance.decManager) && !watchTvTriggered)))
        {
            watchTvTriggered = true;
            activatedBBTimer = true;
            ChangeDorothyDialogue((int)DialoguesRobo.DontWatchThatProgram, true);
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

            D22 = true;
        }
        else if (!hasEated_SC && hasEated.IsMet(GameManager.Instance.decManager) && !hasEated_SC)
        {
            hasEated_SC = true;
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyAngryDay3, true);
            dorothyBook.SetActive(true);
            dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day2);

            D22 = true;
        }
        else if (!hasEated_SC && dorothyLaunchEated.IsMet(GameManager.Instance.decManager))
        {
            hasEated_SC = true;
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyHappyLaunch, true);
            dorothyBook.SetActive(true);
            dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day2);

            D21 = true;
        }

        if ((hasRead.IsMet(GameManager.Instance.decManager) || dorothyBook2Readed.IsMet(GameManager.Instance.decManager))&& 
            dorothyLaunchEated.IsMet(GameManager.Instance.decManager))
        {
            if (!checkedEatDay3)
            {
                checkedEatDay3 = true;
                IniciarSecuenciaCinematica(); // -Cinematica Despres d'acabar aqueta ha de fer la cinematica d'apagar la tele
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
                //Play Cinematic "DOROTHY SE SIENTA CON MATTHEW Y LE ABRAZA
                D51 = true;
            }


        }
        else if (D32)
        {
            if (!hasEated_SC && dorothyLaunchEated.IsMet(GameManager.Instance.decManager))
            {
                // - Cinematica Apagar Tele
                hasEated_SC = true;
                D31 = true;
                Debug.Log("Dorothy Apaga la tele");
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
                        // - Cinematica Abrazo Largo y le quiere mucho + Come To Window
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
                        //Desactivar Menjar Normal
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
            // Numero de final i temps fins que s'executa per si has de ficar algun dialeg
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

            dorothyBook.SetActive(true);
            dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day1);
        }
        else if (!hasRead.IsMet(GameManager.Instance.decManager) && !dorothyAngry)
        {
            ChangeDorothyDialogue((int)DialoguesRobo.DorothyAngryDay2, true);
            dorothyAngry = true;
            angriedDay2.ActionExecuted();

            D12 = true;
        }
        else if (!hasWatchedTv.IsMet(GameManager.Instance.decManager)
            && dorothyBook1Readed.IsMet(GameManager.Instance.decManager))
        {
            ChangeDorothyDialogue((int)DialoguesRobo.DontWatchThatProgram, true);
            watchTvTriggered = true;
            activatedBBTimer = true;
        }
        else if (!hasWatchedTv.IsMet(GameManager.Instance.decManager)
            && dorothyAngriedDay2.IsMet(GameManager.Instance.decManager))
        {
            ChangeDorothyDialogue((int)DialoguesRobo.MattewSturdy, true);
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
            ChangeDorothyDialogue((int)DialoguesRobo.TryToSkipMachineDay1, true);
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

            if (!hasRead.IsMet(GameManager.Instance.decManager)
                && dorothyBook.activeSelf)
            {
                ChangeDorothyDialogue((int)DialoguesRobo.HasNotReaded, true);
                return;
            }
            else if (!hasRead.IsMet(GameManager.Instance.decManager)
                && dorothyAngriedDay2.IsMet(GameManager.Instance.decManager)
                && dorothyAngriedDay3.IsMet(GameManager.Instance.decManager))
            {
                ChangeDorothyDialogue((int)DialoguesRobo.HasNotReaded, true);
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

    public void StartDay()
    {
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

        //Timers
        dayLaunchDorothyCurrentTime = 0;
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

        int index = (int)currentDay;

        if (index >= 0 && index < dayInt.Length && dayInt[index] != null)
        {
            dayInt[index].gameObject.SetActive(true);
        }
    }

    public void FindInteractables()
    {
        dayInt = FindObjectsByType<Action_Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    void GoodMorningPresi()
    {
        switch (GameManager.Instance.currentDay) 
        {
            case Day.Day2:
                PresidentSpeaks(PresiDialogue.GoodMorningDay1);
            break;

            case Day.Day3:
                PresidentSpeaks(PresiDialogue.GoodMorningDay1);
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

    void IniciarSecuenciaCinematica()
    {
        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);

        if (CinematicFader.Instance != null)
        {
            CinematicFader.Instance.FundidoANegro();
        }

        if (day3Timeline != null)
        {
            day3Timeline.Play();
            cinematicStarted = true;
        }
    }

    void TerminarCinematicaYVolverAlJuego()
    {
        if (CinematicFader.Instance != null)
        {
            CinematicFader.Instance.FundidoATransparente();
        }

        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            GameManager.Instance.playerController.LockPlayer(false);
            GameManager.Instance.playerController.LockCamera(false);
        }

        cinematicStarted = false;
        cinematicEndedTriggered = false;
    }
}

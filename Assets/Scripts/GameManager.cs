using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Day { Day1 = 0, Day2, Day3, Day4, Day5, Day6 }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public GameObject UI;
    public PlayerController playerController;
    public PresiSpeaker presiSpeaker;
    public RobotBehaviour robotBehaviour;
    public DayManager dayManager;
    public DecManager decManager;
    public DayTime dayTime;
    public LevelManager levelManager;
    public GameObject cinDialogueVisual;

    public Day currentDay;

    [Header("StartPosition")]
    public Transform playerStart;

    public static event Action<Day> OnChangeDay;
    public static event Action NewDay;

    public static event Action<int> ActivateFinalAnimation;
    public static event Action FinalFade;
    public int pendingFinalId;

    public enum GameState { Gameplay, Interacting, Minigame, Paused }
    public GameState currentState;

    private List<IResetable> resetableObjects = new List<IResetable>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentDay = Day.Day1;
            decManager = GetComponent<DecManager>();
            dayManager = GetComponent<DayManager>();

            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        LevelManager.OnLevelLoaded += LevelLoaded;
        BedInt.OnInteract += ChangeDay;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelLoaded -= LevelLoaded;
        BedInt.OnInteract -= ChangeDay;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    dayTime.SetCurrentTime(21, 0);
        //}
        
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    dayTime.SetCurrentTime(15, 0);
        //}
    }

    private void LevelLoaded()
    {
        if (SceneManager.GetActiveScene().name == "Final")
            return;

        currentState = GameState.Gameplay;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (dayTime != null)
        {
            dayTime.InitMinigamesInGameplay();
            dayTime.ResetDayTime();
        }

        playerController = FindFirstObjectByType<PlayerController>();
        presiSpeaker = FindFirstObjectByType<PresiSpeaker>();
        robotBehaviour = FindFirstObjectByType<RobotBehaviour>();
        UI = GameObject.FindGameObjectWithTag("UI");

        if (dayTime != null)
            dayTime.ResetDayTime();

        GameObject startPointObj = GameObject.FindWithTag("PlayerStart");
        if (startPointObj != null)
        {
            playerStart = startPointObj.transform;

            if (playerController != null)
            {
                if (playerController.characterController != null)
                    playerController.characterController.enabled = false;

                playerController.transform.position = playerStart.position;

                if (playerController.characterController != null)
                    playerController.characterController.enabled = true;
            }
        }

        if (decManager != null)
            decManager.ActionsChanged();

        if (dayManager != null)
        {
            dayManager.FindInteractables();
            dayManager.LoadInteractObjects(currentDay);
            dayManager.StartDay();
        }

        if (presiSpeaker != null)
            presiSpeaker.DialogueIni();

        StartCoroutine(DelayedTransitionSequence());
    }

    private IEnumerator DelayedTransitionSequence()
    {
        yield return null;
        ExecuteDayTransitionSequence();
    }

    public void SetPlayer(PlayerController controller)
    {
        playerController = controller;
    }

    public void ChangeDay()
    {
        if (currentDay < Day.Day5)
        {
            currentDay = (Day)((int)currentDay + 1);

            OnChangeDay?.Invoke(currentDay);

            ExecuteDayTransitionSequence();
        }
    }

    public void RegisterResetable(IResetable resetable)
    {
        if (resetable != null && !resetableObjects.Contains(resetable))
        {
            resetableObjects.Add(resetable);
        }
    }

    public void UnregisterResetable(IResetable resetable)
    {
        if (resetableObjects.Contains(resetable))
        {
            resetableObjects.Remove(resetable);
        }
    }

    private void ExecuteDayTransitionSequence()
    {
        resetableObjects.RemoveAll(item => item == null || (item is MonoBehaviour mono && mono == null));

        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < resetableObjects.Count; i++)
        {
            if (resetableObjects[i] is MonoBehaviour mono && mono.GetComponent<CapabilityUI>() == null)
            {
                resetableObjects[i].ResetValues();
            }
        }

        NewDay?.Invoke();

        for (int i = 0; i < resetableObjects.Count; i++)
        {
            if (resetableObjects[i] is MonoBehaviour mono && mono.GetComponent<CapabilityUI>() != null)
            {
                resetableObjects[i].ResetValues();
            }
        }
    }

    public void ExecuteFinishAnimation(int id, int waitTime)
    {
        pendingFinalId = id;
        FinalFade?.Invoke();
        StartCoroutine(LoadFinalDelayed(waitTime));
    }

    private IEnumerator LoadFinalDelayed(int waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        levelManager.LoadFinal();
    }
}
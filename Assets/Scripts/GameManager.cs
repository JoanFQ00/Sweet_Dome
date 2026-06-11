using System;
using System.Collections;
using System.Diagnostics;
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
    public Day currentDay;

    [Header("StartPosition")]
    public Transform playerStart;

    public static event Action<Day> OnChangeDay;
    public static event Action NewDay;

    public static event Action<int> ActivateFinalAnimation;
    public static event Action FinalFade;
    public int pendingFinalId;

    public enum GameState
    {
        Gameplay,
        Interacting,
        Minigame,
        Paused
    }

    public GameState currentState;

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

    protected virtual void Start()
    {
        StartCoroutine(DelayedStartDay());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y)) 
        {
            ExecuteFinishAnimation(0, 3);
        }
    }

    private IEnumerator DelayedStartDay()
    {
        yield return null;
        NewDay?.Invoke();
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

    private void LevelLoaded()
    {
        if (SceneManager.GetActiveScene().name == "Final")
            return;

        currentState = GameState.Gameplay;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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

            if (presiSpeaker != null)
            {
                presiSpeaker.ChangeDialogueID(1); 
                presiSpeaker.PresiSpeaks();
            }

            NewDay?.Invoke();
        }
    }

    // Numero de final i temps fins que s'executa per si has de ficar algun dialeg
    public void ExecuteFinishAnimation(int id, int waitTime)
    {
        pendingFinalId = id;
        FinalFade?.Invoke();
        StartCoroutine(LoadFinalDelayed(waitTime));
    }

    IEnumerator LoadFinalDelayed(int waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        levelManager.LoadFinal();
    }
}
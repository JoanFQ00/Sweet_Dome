using System;
using System.Collections;
using UnityEngine;
public enum Day { Day1 = 0, Day2, Day3, Day4, Day5 }

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
    public Day currentDay;

    [Header("StartPosition")]
    public Transform playerStart;

    public static Action<Day> OnChangeDay;
    public static Action NewDay;
   
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
        playerController = FindFirstObjectByType<PlayerController>();
        presiSpeaker = FindFirstObjectByType<PresiSpeaker>();
        robotBehaviour = FindFirstObjectByType<RobotBehaviour>();
        UI = GameObject.FindGameObjectWithTag("UI");
        dayTime.timerText = UI.GetComponent<UIManager>().timerText;
        dayTime.ResetDayTime();

        playerStart = GameObject.FindWithTag("PlayerStart").transform;
        playerController.transform.position = playerStart.position;
        decManager.ActionsChanged();
        dayManager.FindInteractables();
        dayManager.LoadInteractObjects(currentDay);

        if (presiSpeaker != null)
            presiSpeaker.DialogueIni();

        if (dayManager != null)
            dayManager.StartDay();

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

            presiSpeaker.ChangeDialogueID((int)PresiDialogue.GoodMorningDay1);
            presiSpeaker.PresiSpeaks();

            NewDay?.Invoke();
        }
    }
}

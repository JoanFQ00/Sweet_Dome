using System;
using System.Collections;
using UnityEngine;
public enum Day { Day1, Day2, Day3, Day4, Day5 }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject UI;

    public PlayerController playerController;
    public PresiSpeaker presiSpeaker;
    public RobotBehaviour robotBehaviour;
    public DayManager dayManager;
    public DecManager decManager;
    public DayTime dayTime;
    public Day currentDay;

    public static Action OnChangeDay;
    public static Action NewDay;
    //public static Action OnSpawnObj;
   
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
        OnChangeDay?.Invoke();

        if (currentDay < Day.Day5)
        {
            currentDay = (Day)((int)currentDay + 1);
            NewDay?.Invoke();
        }
    }
}

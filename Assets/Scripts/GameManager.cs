using System;
using UnityEngine;
public enum Day { Day1, Day2, Day3, Day4, Day5 }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerController playerController;
    public PresiSpeaker presiSpeaker;
    public RobotBehaviour robotBehaviour;
    public DayManager dayManager;
    public DecManager decManager;
    public Day currentDay;

    public static Action OnChangeDay;
    public static Action OnSpawnObj;
   
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

    private void Start()
    {
        robotBehaviour.dialogueSys.SysIni();
        presiSpeaker.DialogueIni(); 
        dayManager.StartDay();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) 
        {
            SpawnRadarObjective();
        }
    }

    private void SpawnRadarObjective() 
    {
        OnSpawnObj?.Invoke();
    }

    private void OnEnable()
    {
        BedInt.OnInteract += ChangeDay;
    }

    private void OnDisable()
    {
        BedInt.OnInteract -= ChangeDay;
    }

    public void SetPlayer(PlayerController controller) 
    {
        playerController = controller;
    }

    public void ChangeDay() 
    {
        OnChangeDay?.Invoke();

        if (currentDay < Day.Day2)
        {
            currentDay = (Day)((int)currentDay + 1);
        }
    }
}

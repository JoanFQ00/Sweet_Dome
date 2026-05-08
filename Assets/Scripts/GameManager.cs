using System;
using Unity.VisualScripting;
using UnityEngine;
public enum Day { Day1, Day2 }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerController playerController;
    public PresiSpeaker presiSpeaker;
    public RobotBehaviour robotBehaviour;
    public DayManager dayManager;
    public Day currentDay;

    public static Action OnChangeDay;
    public static Action OnSpawnObj;
   
    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            currentDay = Day.Day1;
            DontDestroyOnLoad(gameObject);
        }
        else 
            Destroy(gameObject);
    }

    private void Start()
    {
        dayManager = GetComponent<DayManager>();

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
        currentDay += 1;
    }
}

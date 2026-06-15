using UnityEngine;
using UnityEngine.Playables;
using System;

public class CinematicManager : MonoBehaviour
{
    public static CinematicManager Instance { get; private set; }

    [Header("Timelines")]
    public PlayableDirector wakeUpTimeline;
    public PlayableDirector day3TurnOffTvTimeline;
    public PlayableDirector day3SitAndWatchTimeline;
    public PlayableDirector day4SitAndHugTimeline;
    public PlayableDirector day4TurnOffTvTimeline;
    public PlayableDirector day5LongHugTimeline;
    public PlayableDirector day5WatchWindowTimeline;
    public PlayableDirector goToSleepTimeline;
    public PlayableDirector finalCinematic;

    public PlayableDirector activeTimeline;
    private bool cinematicStarted = false;
    private bool cinematicEndedTriggered = false;

    public GameObject cinematicDialogueUI;

    public static event Action OnCinematicEnded;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        cinematicDialogueUI.SetActive(false);
    }

    private void OnEnable()
    {
        DayManager.OnRequestCinematic += StartCinematicSequence;
        MachineManager.OnFinishedDay += StartCinematicSequence;
        GameManager.NewDay += StartDayCinematic;
    }

    private void OnDisable()
    {
        DayManager.OnRequestCinematic -= StartCinematicSequence;
        MachineManager.OnFinishedDay -= StartCinematicSequence;
        GameManager.NewDay -= StartDayCinematic;
    }

    private void Update()
    {
        if (cinematicStarted && !cinematicEndedTriggered && activeTimeline != null)
        {
            if (activeTimeline.state != PlayState.Playing || activeTimeline.time >= activeTimeline.duration)
            {
                cinematicEndedTriggered = true;
                EndCinematicAndReturnToGameplay();
            }
        }
    }

    void StartDayCinematic() 
    {
        StartCinematicSequence("WakeUpCinematic");
    }

    void StartCinematicSequence(string cinematicName)
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            GameManager.Instance.playerController.LockPlayer(true);
            GameManager.Instance.playerController.LockCamera(true);
        }

        switch (cinematicName)
        {
            case "Day3SitAndWatch":
                activeTimeline = day3SitAndWatchTimeline;
                break;
            case "Day3TurnOffTv":
                activeTimeline = day3TurnOffTvTimeline;
                break;
            case "Day4Hug":
                activeTimeline = day4SitAndHugTimeline;
                break;
            case "Day4TurnOffTv":
                activeTimeline = day4TurnOffTvTimeline;
                break;
            case "Day5LongHug":
                activeTimeline = day5LongHugTimeline;
                break;
            case "Day5WatchWindow":
                activeTimeline = day5WatchWindowTimeline;
                break;
            case "GoToSleep":
                activeTimeline = goToSleepTimeline;
                break;
            case "FinalCinematic":
                activeTimeline = finalCinematic;
                break;
            case "WakeUpCinematic":
                activeTimeline = wakeUpTimeline;
                break;
            default:
                activeTimeline = null;
                break;
        }

        if (activeTimeline != null)
        {
            activeTimeline.Play();
            cinematicStarted = true;
            cinematicEndedTriggered = false;
        }
        else
        {
            EndCinematicAndReturnToGameplay();
        }
    }

    void EndCinematicAndReturnToGameplay()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            GameManager.Instance.playerController.LockPlayer(false);
            GameManager.Instance.playerController.LockCamera(false);
        }

        if (activeTimeline.name == "Cin (2)")
        {
            StartCinematicSequence("Day3TurnOffTv");
            return;
        }
        else if (activeTimeline.name == "Cin (5)")
        {
            StartCinematicSequence("Day5WatchWindow");
        }
        else
            activeTimeline = null;

        cinematicStarted = false;
        cinematicEndedTriggered = false;
        activeTimeline = null;

        OnCinematicEnded?.Invoke();
    }
}
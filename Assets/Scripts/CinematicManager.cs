using UnityEngine;
using UnityEngine.Playables;
using System;

public class CinematicManager : MonoBehaviour
{
    public static CinematicManager Instance { get; private set; }

    [Header("Keybindings")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

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
            if (Input.GetKeyDown(skipKey))
            {
                SkipCurrentCinematic();
                return;
            }

            if (activeTimeline.state != PlayState.Playing || activeTimeline.time >= activeTimeline.duration)
            {
                cinematicEndedTriggered = true;
                EndCinematicAndReturnToGameplay();
            }
        }
    }

    private void SkipCurrentCinematic()
    {
        if (activeTimeline == null) return;

        string timelineName = activeTimeline.name;

        activeTimeline.extrapolationMode = DirectorWrapMode.Hold;

        activeTimeline.time = activeTimeline.duration;

        activeTimeline.Evaluate();

        cinematicEndedTriggered = true;

        EndCinematicAndReturnToGameplay(timelineName);
    }

    void StartDayCinematic() 
    {
        StartCinematicSequence("WakeUpCinematic");
    }

    void StartCinematicSequence(string cinematicName)
    {
        if (cinematicStarted) return;

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
            activeTimeline.time = 0;

            activeTimeline.Evaluate();

            activeTimeline.Play();

            cinematicStarted = true;
            cinematicEndedTriggered = false;
        }
        else
        {
            EndCinematicAndReturnToGameplay();
        }
    }

    void EndCinematicAndReturnToGameplay(string finishedTimelineName = "")
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            GameManager.Instance.playerController.LockPlayer(false);
            GameManager.Instance.playerController.LockCamera(false);
        }

        if (string.IsNullOrEmpty(finishedTimelineName) && activeTimeline != null)
        {
            finishedTimelineName = activeTimeline.name;
        }

        cinematicStarted = false;
        cinematicEndedTriggered = false;
        activeTimeline = null;

        if (!string.IsNullOrEmpty(finishedTimelineName))
        {
            if (finishedTimelineName == "Cin (4)")
            {
                StartCinematicSequence("Day3TurnOffTv");
                return;
            }
            else if (finishedTimelineName == "Cin (6)")
            {
                StartCinematicSequence("Day5WatchWindow");
                return;
            }
        }

        OnCinematicEnded?.Invoke();
    }
}
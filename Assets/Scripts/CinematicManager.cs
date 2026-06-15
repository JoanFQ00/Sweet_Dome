using UnityEngine;
using UnityEngine.Playables;
using System;

public class CinematicManager : MonoBehaviour
{
    public static CinematicManager Instance { get; private set; }

    [Header("Timelines")]
    public PlayableDirector day3Timeline;
    public PlayableDirector day3TurnOffTvTimeline;
    public PlayableDirector day4HugTimeline;
    public PlayableDirector day4TurnOffTvTimeline;

    private PlayableDirector activeTimeline;
    private bool cinematicStarted = false;
    private bool cinematicEndedTriggered = false;

    public static event Action OnCinematicEnded;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        DayManager.OnRequestCinematic += StartCinematicSequence;
    }

    private void OnDisable()
    {
        DayManager.OnRequestCinematic -= StartCinematicSequence;
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

    void StartCinematicSequence(string cinematicName)
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            GameManager.Instance.playerController.LockPlayer(true);
            GameManager.Instance.playerController.LockCamera(true);
        }

        switch (cinematicName)
        {
            case "Day3":
                activeTimeline = day3Timeline;
                break;
            case "Day3TurnOffTv":
                activeTimeline = day3TurnOffTvTimeline;
                break;
            case "Day4Hug":
                activeTimeline = day4HugTimeline;
                break;
            case "Day4TurnOffTv":
                activeTimeline = day4TurnOffTvTimeline;
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

        cinematicStarted = false;
        cinematicEndedTriggered = false;
        activeTimeline = null;

        OnCinematicEnded?.Invoke();
    }
}
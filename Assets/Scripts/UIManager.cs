using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Menus")]
    public GameObject pauseMenu;
    public GameObject optionsMenu;
    public Button menuButton;

    [Header("Interact")]
    public GameObject interactTxt;
    public GameObject stopInteractTxt;

    [Header("Time")]
    public TextMeshProUGUI timerText, dayText;
    public TextMeshProUGUI timeWalls;

    [Header("Objectives")]
    public List<TextMeshProUGUI> objectivseTMP;

    void Start()
    {
        interactTxt.SetActive(false);
        stopInteractTxt.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);

        menuButton.onClick.AddListener(OnMenuButtonClicked);
    }

    private void OnEnable()
    {
        GameManager.NewDay += OnDayChanged;
        Interact.CanInteract += ChangeInteractVisibility;
        DecManager.OnActionsChanged += ChangeHudObjectives;
        LevelManager.OnLevelLoaded += OnSceneLoaded;

        DayTime.OnTimeChanged += UpdateTimeUI;
    }

    private void OnDisable()
    {
        GameManager.NewDay -= OnDayChanged;
        Interact.CanInteract -= ChangeInteractVisibility;
        DecManager.OnActionsChanged -= ChangeHudObjectives;
        LevelManager.OnLevelLoaded -= OnSceneLoaded;

        DayTime.OnTimeChanged -= UpdateTimeUI;
    }

    private void Update()
    {
        if (GameManager.Instance.currentState != GameState.Gameplay)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangePauseMenuState(!pauseMenu.activeSelf);
        }
    }
    void ChangeInteractVisibility(bool visibility, bool interacting) 
    {
        if (interacting)
        {
            interactTxt.SetActive(false);
            stopInteractTxt.SetActive(true);
        }
        else 
        {
            interactTxt.SetActive(visibility);
            stopInteractTxt.SetActive(false);
        }
    }

    void OnDayChanged()
    {
        dayText.text = $"{GameManager.Instance.currentDay.ToString().ToUpper()}";
    }

    private void UpdateTimeUI(int hour, int minutes)
    {
        timerText.text = $"{hour:00} {minutes:00}";

        if (timeWalls != null)
            timeWalls.text = $"{hour:00}  {minutes:00}";
    }

    void ChangeHudObjectives(List<NDec_Act> actionList)
    {
        for (int i = 0; i < objectivseTMP.Count; i++)
        {
            if (i >= actionList.Count || actionList[i] == null)
            {
                objectivseTMP[i].gameObject.SetActive(false);
            }
            else
            {
                objectivseTMP[i].gameObject.SetActive(true);
                objectivseTMP[i].text = actionList[i].actionName;
            }
        }
    }

    public void ChangePauseMenuState(bool state)
    {
        pauseMenu.SetActive(state);
        optionsMenu.SetActive(false);

        if (state)
        {
            GameManager.Instance.currentState = GameState.Paused;

            GameManager.Instance.playerController.LockPlayer(true);
            GameManager.Instance.playerController.LockCamera(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            GameManager.Instance.currentState = GameState.Gameplay;

            GameManager.Instance.playerController.LockPlayer(false);
            GameManager.Instance.playerController.LockCamera(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ChangeOptionsMenu(bool state)
    {
        optionsMenu.SetActive(state);
        pauseMenu.SetActive(!state);
    }

    private void OnSceneLoaded()
    {
        interactTxt.SetActive(false);
        stopInteractTxt.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);

        menuButton.onClick.AddListener(OnMenuButtonClicked);
    }

    private void OnMenuButtonClicked()
    {
        GameManager.Instance.levelManager.LoadMenu();
    }
}
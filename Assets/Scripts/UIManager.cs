using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

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
    }

    private void OnDisable()
    {
        GameManager.NewDay -= OnDayChanged;
        Interact.CanInteract -= ChangeInteractVisibility;
        DecManager.OnActionsChanged -= ChangeHudObjectives;
        LevelManager.OnLevelLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
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
        if (state && !optionsMenu.activeSelf 
            && !GameManager.Instance.playerController.playerLocked)
        {
            pauseMenu.SetActive(true);
            GameManager.Instance.playerController.LockPlayer(true);
            GameManager.Instance.playerController.LockCamera(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else 
        {
            pauseMenu.SetActive(false);
            optionsMenu.SetActive(false);
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
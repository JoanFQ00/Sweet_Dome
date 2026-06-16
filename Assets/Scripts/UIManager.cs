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

    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button quitButton;

    [Header("Options")]
    [SerializeField] private Slider yawSlider;
    [SerializeField] private Slider pitchSlider;

    [SerializeField] private TMP_Text yawValueText;
    [SerializeField] private TMP_Text pitchValueText;

    [Header("Interact")]
    public GameObject interactTxt;
    public GameObject stopInteractTxt;

    [Header("ZoomIm")]
    public GameObject zoomIm;

    [Header("Time")]
    public TextMeshProUGUI timeHour, timeMinute;
    public TextMeshProUGUI timeHourMachine, timeMinuteMachine;

    [Header("Objectives")]
    public List<TextMeshProUGUI> objectivseTMP;

    [Header("Center and Cursors")]
    public GameObject center;

    void Start()
    {
        interactTxt.SetActive(false);
        stopInteractTxt.SetActive(false);
        zoomIm.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        SetupPauseButtons();
    }

    private void OnEnable()
    {
        Interact.CanInteract += ChangeInteractVisibility;
        DecManager.OnActionsChanged += ChangeHudObjectives;
        LevelManager.OnLevelLoaded += OnSceneLoaded;
        MiniGameTest.OnMachineEntered += MachineEntered;
        PlayerController.OnZoomActivated += ShowHideZoom;
        DayTime.OnTimeChanged += UpdateTimeUI;
    }

    private void OnDisable()
    {
        Interact.CanInteract -= ChangeInteractVisibility;
        DecManager.OnActionsChanged -= ChangeHudObjectives;
        LevelManager.OnLevelLoaded -= OnSceneLoaded;
        MiniGameTest.OnMachineEntered -= MachineEntered;
        PlayerController.OnZoomActivated -= ShowHideZoom;
        DayTime.OnTimeChanged -= UpdateTimeUI;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.currentState == GameState.Gameplay)
            {
                ChangePauseMenuState(true);
            }
            else if (GameManager.Instance.currentState == GameState.Paused)
            {
                ChangePauseMenuState(false);
            }
        }
    }

    private void SetupPauseButtons()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeButtonClicked);

        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    public void OnResumeButtonClicked()
    {
        Time.timeScale = 1f;
        ChangePauseMenuState(false);
    }

    public void ShowHideZoom(bool show) 
    {
        zoomIm.SetActive(show);
    }

    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    void ChangeInteractVisibility(bool visibility, bool interacting) 
    {
        if (interacting)
        {
        }
        else
        {
        }
    }

    void MachineEntered(bool entered)
    {
        if (entered)
            center.SetActive(false);
        else 
            center.SetActive(true);
    }

    private void UpdateTimeUI(int hour, int minutes)
    {
        if (timeHour != null) 
        {
            timeHour.text = $"{hour}";
            timeHourMachine.text = $"{hour}";

            if (minutes == 0)
            {
                timeMinute.text = $"{minutes}0";
                timeMinuteMachine.text = $"{minutes}0";
            }
            else 
            {
                 timeMinute.text = $"{minutes}";
                 timeMinuteMachine.text = $"{minutes}";
            }
        }
    }

    public void ForceChangeObjectives(string newObj) 
    {
        objectivseTMP[0].text = newObj;
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
            Time.timeScale = 0f;

            GameManager.Instance.currentState = GameState.Paused;

            GameManager.Instance.playerController.LockPlayer(true);
            GameManager.Instance.playerController.LockCamera(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;

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

        if(state)
            InitializeOptions();
    }

    private void OnSceneLoaded()
    {
        Time.timeScale = 1f;

        interactTxt.SetActive(false);
        stopInteractTxt.SetActive(false);
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
    }

    private const float MIN_LOOK_SPEED = 20f;
    private const float MAX_LOOK_SPEED = 300f;

    private void InitializeOptions()
    {
        PlayerController player = GameManager.Instance.playerController;

        yawSlider.minValue = 5;
        yawSlider.maxValue = 20;
        yawSlider.wholeNumbers = true;

        pitchSlider.minValue = 5;
        pitchSlider.maxValue = 20;
        pitchSlider.wholeNumbers = true;

        yawSlider.value = ConvertSpeedToSlider(player.yawSpeed);
        pitchSlider.value = ConvertSpeedToSlider(player.pitchSpeed);

        yawValueText.text = yawSlider.value.ToString("F0");
        pitchValueText.text = pitchSlider.value.ToString("F0");

        yawSlider.onValueChanged.RemoveAllListeners();
        pitchSlider.onValueChanged.RemoveAllListeners();

        yawSlider.onValueChanged.AddListener(SetYawSpeed);
        pitchSlider.onValueChanged.AddListener(SetPitchSpeed);
    }

    private float ConvertSliderToSpeed(float sliderValue)
    {
        return Mathf.Lerp(MIN_LOOK_SPEED,MAX_LOOK_SPEED,(sliderValue - 1f) / 99f);
    }

    private float ConvertSpeedToSlider(float speed)
    {
        return Mathf.Lerp(1f,100f,Mathf.InverseLerp(MIN_LOOK_SPEED, MAX_LOOK_SPEED, speed));
    }

    private void SetYawSpeed(float sliderValue)
    {
        float speed = ConvertSliderToSpeed(sliderValue);

        GameManager.Instance.playerController.yawSpeed = speed;

        yawValueText.text = sliderValue.ToString("F0");

        PlayerPrefs.SetFloat("YawSpeed", speed);
        PlayerPrefs.Save();
    }

    private void SetPitchSpeed(float sliderValue)
    {
        float speed = ConvertSliderToSpeed(sliderValue);

        GameManager.Instance.playerController.pitchSpeed = speed;

        pitchValueText.text = sliderValue.ToString("F0");

        PlayerPrefs.SetFloat("PitchSpeed", speed);
        PlayerPrefs.Save();
    }
}
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

    [Header("Options")]
    [SerializeField] private Slider yawSlider;
    [SerializeField] private Slider pitchSlider;

    [SerializeField] private TMP_Text yawValueText;
    [SerializeField] private TMP_Text pitchValueText;

    [Header("Interact")]
    public GameObject interactTxt;
    public GameObject stopInteractTxt;

    [Header("Time")]
    public TextMeshProUGUI timerText, dayText;
    public TextMeshProUGUI timeWalls;

    [Header("Objectives")]
    public List<TextMeshProUGUI> objectivseTMP;

    [Header("Center and Cursors")]
    public GameObject center;

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
        MiniGameTest.OnMachineEntered += MachineEntered;

        DayTime.OnTimeChanged += UpdateTimeUI;
    }

    private void OnDisable()
    {
        GameManager.NewDay -= OnDayChanged;
        Interact.CanInteract -= ChangeInteractVisibility;
        DecManager.OnActionsChanged -= ChangeHudObjectives;
        LevelManager.OnLevelLoaded -= OnSceneLoaded;
        MiniGameTest.OnMachineEntered -= MachineEntered;

        DayTime.OnTimeChanged -= UpdateTimeUI;
    }

    private void Update()
    {
        if (GameManager.Instance.currentState != GameState.Gameplay)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("AAA");
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

    void MachineEntered(bool entered)
    {
        if (entered)
            center.SetActive(false);
        else 
            center.SetActive(true);
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

        if(state)
            InitializeOptions();
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
        return Mathf.Lerp(
            MIN_LOOK_SPEED,
            MAX_LOOK_SPEED,
            (sliderValue - 1f) / 99f
        );
    }

    private float ConvertSpeedToSlider(float speed)
    {
        return Mathf.Lerp(
            1f,
            100f,
            Mathf.InverseLerp(MIN_LOOK_SPEED, MAX_LOOK_SPEED, speed)
        );
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
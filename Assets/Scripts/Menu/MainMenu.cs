using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 2f;

    private bool fadingOut = false;
    private bool loadScene = false;

    [Header("Panels")]
    public GameObject controlPanel;
    public GameObject optionsPanel;
    public GameObject menuPanel;

    [Header("Settings Sensibility")]
    public Slider yawSlider;
    public Slider pitchSlider;
    public TMP_Text yawValueText;
    public TMP_Text pitchValueText;
    public float sliderMinimo = 1f;
    public float sliderMaximo = 100f;

    private const float MIN_LOOK_SPEED = 5f;
    private const float MAX_LOOK_SPEED = 20;

    private const float DEFAULT_SLIDER_VAL = 50f;

    void Start()
    {
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        controlPanel.SetActive(false);
    }

    void Update()
    {
        if (fadingOut)
        {
            Color c = fadeImage.color;

            c.a += fadeSpeed * Time.unscaledDeltaTime;
            fadeImage.color = c;

            if (c.a >= 1f && !loadScene)
            {
                loadScene = true;
                if (SoundManagerOwn.Instance != null)
                {
                    SoundManagerOwn.Instance.PlayGameplayMusic();
                }
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }

    public void PlayGame()
    {
        fadingOut = true;
    }

    public void OptionsMenu()
    {
        optionsPanel.SetActive(true);
        menuPanel.SetActive(false);
        InitializeSliders();
    }
    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void ShowControls() 
    {
        controlPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void HideControls() 
    {
        controlPanel.SetActive(false);
        menuPanel.SetActive(true); 
    }

    private void InitializeSliders()
    {
        yawSlider.minValue = sliderMinimo;
        yawSlider.maxValue = sliderMaximo;
        yawSlider.wholeNumbers = true;

        pitchSlider.minValue = sliderMinimo;
        pitchSlider.maxValue = sliderMaximo;
        pitchSlider.wholeNumbers = true;

        float defaultSpeed = ConvertSliderToSpeed(DEFAULT_SLIDER_VAL);
        float savedYawSpeed = PlayerPrefs.GetFloat("YawSpeed", defaultSpeed);
        float savedPitchSpeed = PlayerPrefs.GetFloat("PitchSpeed", defaultSpeed);

        yawSlider.value = ConvertSpeedToSlider(savedYawSpeed);
        pitchSlider.value = ConvertSpeedToSlider(savedPitchSpeed);

        yawValueText.text = yawSlider.value.ToString("F0");
        pitchValueText.text = pitchSlider.value.ToString("F0");

        yawSlider.onValueChanged.RemoveAllListeners();
        pitchSlider.onValueChanged.RemoveAllListeners();
        yawSlider.onValueChanged.AddListener(SetYawSpeed);
        pitchSlider.onValueChanged.AddListener(SetPitchSpeed);
    }

    private float ConvertSliderToSpeed(float sliderValue)
    {
        return Mathf.Lerp(MIN_LOOK_SPEED, MAX_LOOK_SPEED, (sliderValue - 1f) / 99f);
    }

    private float ConvertSpeedToSlider(float speed)
    {
        return Mathf.Lerp(1f, 100f, Mathf.InverseLerp(MIN_LOOK_SPEED, MAX_LOOK_SPEED, speed));
    }

    private void SetYawSpeed(float sliderValue)
    {
        float speed = ConvertSliderToSpeed(sliderValue);
        yawValueText.text = sliderValue.ToString("F0");
        PlayerPrefs.SetFloat("YawSpeed", speed);
        PlayerPrefs.Save();
    }

    private void SetPitchSpeed(float sliderValue)
    {
        float speed = ConvertSliderToSpeed(sliderValue);
        pitchValueText.text = sliderValue.ToString("F0");
        PlayerPrefs.SetFloat("PitchSpeed", speed);
        PlayerPrefs.Save();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
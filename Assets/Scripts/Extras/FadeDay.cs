using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeDay : MonoBehaviour
{
    public Image fadeImage;
    public TextMeshProUGUI dayTMP;

    [Header("Fade Settings")]
    public float fadeInTime = 2.5f;
    public float fadeOutTime = 2.0f;
    public bool lockPlayer;

    private enum FadeState
    {
        None,
        FadingOut,
        FadingIn,
        FinalFade
    }

    private FadeState fadeState;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Color imgColor = fadeImage.color;
        imgColor.a = 1f;
        fadeImage.color = imgColor;

        Color textColor = dayTMP.color;
        textColor.a = 1f;
        dayTMP.color = textColor;

        StartFade();
    }

    private void OnEnable()
    {
        GameManager.NewDay += StartFade;
        LevelManager.OnLevelLoaded += StartFadeIn;
        GameManager.FinalFade += StartFinalFade;
    }

    private void OnDisable()
    {
        GameManager.NewDay -= StartFade;
        LevelManager.OnLevelLoaded -= StartFadeIn;
        GameManager.FinalFade -= StartFinalFade;
    }

    void StartFinalFade() 
    {
        fadeImage.gameObject.SetActive(true);
        dayTMP.gameObject.SetActive(false);

        fadeState = FadeState.FinalFade;
    }

    public void StartFade()
    {
        fadeImage.gameObject.SetActive(true);
        dayTMP.gameObject.SetActive(true);

        switch (GameManager.Instance.currentDay) 
        {
            case Day.Day1:
                dayTMP.text = "Day 1";
                break;
            case Day.Day2:
                dayTMP.text = "Day 5";
                break;
            case Day.Day3:
                dayTMP.text = "Day 13";
                break;
            case Day.Day4:
                dayTMP.text = "Day 25";
                break;
            case Day.Day5:
                dayTMP.text = "Day 67";
                break;
        }

        if (lockPlayer) 
        {
            GameManager.Instance.playerController.LockPlayer(true);
            GameManager.Instance.playerController.LockCamera(true);
        }

        fadeState = FadeState.FadingOut;
    }

    public void StartFadeIn() 
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Gameplay"))
            dayTMP.gameObject.SetActive(true);

        fadeImage.gameObject.SetActive(true);
        dayTMP.text = GameManager.Instance.currentDay.ToString();

        if (lockPlayer)
        {
            GameManager.Instance.playerController.LockPlayer(true);
            GameManager.Instance.playerController.LockCamera(true);
        }

        fadeState = FadeState.FadingIn;
    }

    private void Update()
    {
        Color imgColor = fadeImage.color;
        Color textColor = dayTMP.color;

        switch (fadeState)
        {
            case FadeState.FadingOut:

                imgColor.a += Time.unscaledDeltaTime / fadeOutTime;
                textColor.a = imgColor.a;

                fadeImage.color = imgColor;
                dayTMP.color = textColor;

                if (imgColor.a >= 1f)
                {
                    fadeState = FadeState.FadingIn;
                }

            break;

            case FadeState.FadingIn:

                imgColor.a -= Time.unscaledDeltaTime / fadeInTime;
                textColor.a = imgColor.a;

                fadeImage.color = imgColor;
                dayTMP.color = textColor;

                if (imgColor.a <= 0f)
                {
                    fadeState = FadeState.None;

                    fadeImage.gameObject.SetActive(false);
                    dayTMP.gameObject.SetActive(false);

                    GameManager.Instance.playerController.LockPlayer(false);
                    GameManager.Instance.playerController.LockCamera(false);
                }
            break;
            case FadeState.FinalFade:
                imgColor.a += Time.unscaledDeltaTime / fadeOutTime;
                textColor.a = 0f; 

                fadeImage.color = imgColor;
                dayTMP.color = textColor;

                if (imgColor.a >= 1f)
                {
                    imgColor.a = 1f;
                    fadeImage.color = imgColor;

                    fadeState = FadeState.None;
                }
                break;
        }
    }
}
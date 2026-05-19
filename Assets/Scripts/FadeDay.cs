using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeDay : MonoBehaviour
{
    public Image fadeImage;
    public TextMeshProUGUI dayTMP;

    [Header("Fade Settings")]
    public float fadeInTime = 2.5f;
    public float fadeOutTime = 2.0f;

    private enum FadeState
    {
        None,
        FadingOut,
        FadingIn
    }

    private FadeState fadeState;

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
    }

    private void OnDisable()
    {
        GameManager.NewDay -= StartFade;
        LevelManager.OnLevelLoaded -= StartFade;
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
                dayTMP.text = "Day 11";
                break;
            case Day.Day4:
                dayTMP.text = "Day 26";
                break;
            case Day.Day5:
                dayTMP.text = "Day 67";
                break;
        }

        fadeState = FadeState.FadingOut;
    }

    public void StartFadeIn() 
    {
        fadeImage.gameObject.SetActive(true);
        dayTMP.gameObject.SetActive(true);
        dayTMP.text = GameManager.Instance.currentDay.ToString();

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
                }
            break;
        }
    }
}
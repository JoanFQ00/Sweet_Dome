using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeDay : MonoBehaviour
{
    public Image fadeImage;
    public TextMeshProUGUI dayTMP;

    [Header("Fade Settings")]
    public float fadeTime = 2.5f;

    private enum FadeState
    {
        None,
        FadingOut,
        FadingIn
    }

    private FadeState fadeState;

    private void Start()
    {
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        StartFade();
    }

    private void OnEnable()
    {
        GameManager.NewDay += StartFade;
        LevelManager.OnLevelLoaded += StartFadeIn;
    }

    private void OnDisable()
    {
        GameManager.NewDay += StartFade;
        LevelManager.OnLevelLoaded -= StartFadeIn;
    }

    public void StartFade()
    {
        fadeImage.gameObject.SetActive(true);
        dayTMP.gameObject.SetActive(true);
        dayTMP.text = GameManager.Instance.currentDay.ToString();

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
        Color c = fadeImage.color;

        switch (fadeState)
        {
            case FadeState.FadingOut:

                c.a += Time.unscaledDeltaTime / fadeTime;

                fadeImage.color = c;

                if (c.a >= 1f)
                {
                    // CAMBIAR DIA / CARGAR ESCENA
                    // GameManager.Instance.ChangeDay();

                    fadeState = FadeState.FadingIn;
                }

                break;

            case FadeState.FadingIn:

                c.a -= Time.unscaledDeltaTime / fadeTime;

                fadeImage.color = c;

                if (c.a <= 0f)
                {
                    fadeState = FadeState.None;

                    fadeImage.gameObject.SetActive(false);
                    dayTMP.gameObject.SetActive(false);
                }

                break;
        }
    }
}
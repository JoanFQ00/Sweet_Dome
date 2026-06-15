using System.Collections;
using TMPro;
using UnityEngine;

public class FadeDay : MonoBehaviour
{
    public TextMeshProUGUI dayTMP;

    [Header("Day Text Settings")]
    public float delayBeforeShow = 15f;
    public float textFadeInTime = 0.5f;
    public float visibleTime = 2f;
    public float textFadeOutTime = 1f;

    public static FadeDay Instance;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        dayTMP.gameObject.SetActive(false);

        Color textColor = dayTMP.color;
        textColor.a = 0f;
        dayTMP.color = textColor;

        StartFade();
    }

    private void OnEnable()
    {
        GameManager.NewDay += StartFade;
        LevelManager.OnLevelLoaded += StartFade;
    }

    private void OnDisable()
    {
        GameManager.NewDay -= StartFade;
        LevelManager.OnLevelLoaded -= StartFade;
    }

    public void StartFade()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowDayTextRoutine());
    }

    private IEnumerator ShowDayTextRoutine()
    {
        yield return new WaitForSeconds(delayBeforeShow);

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

            default:
                dayTMP.text = GameManager.Instance.currentDay.ToString();
                break;
        }

        dayTMP.gameObject.SetActive(true);

        Color textColor = dayTMP.color;

        float t = 0f;
        textColor.a = 0f;
        dayTMP.color = textColor;

        while (t < textFadeInTime)
        {
            t += Time.deltaTime;

            textColor.a = Mathf.Lerp(0f, 1f, t / textFadeInTime);
            dayTMP.color = textColor;

            yield return null;
        }

        textColor.a = 1f;
        dayTMP.color = textColor;

        yield return new WaitForSeconds(visibleTime);

        t = 0f;

        while (t < textFadeOutTime)
        {
            t += Time.deltaTime;

            textColor.a = Mathf.Lerp(1f, 0f, t / textFadeOutTime);
            dayTMP.color = textColor;

            yield return null;
        }

        textColor.a = 0f;
        dayTMP.color = textColor;

        dayTMP.gameObject.SetActive(false);
        currentRoutine = null;
    }
}
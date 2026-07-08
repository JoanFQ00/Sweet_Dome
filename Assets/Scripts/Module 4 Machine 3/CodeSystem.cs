using UnityEngine;
using TMPro;
using System.Collections;

public class CodeSystem : MonoBehaviour
{
    [Header("3D Canvas Input")]
    public GameObject inputCanvas;
    public TextMeshProUGUI inputDisplay;

    [Header("Normal UI Canvas")]
    public GameObject generatedCodeCanvas;
    public TextMeshProUGUI generatedCodeText;

    [Header("Config")]
    public int codeLength = 4;
    public float codeVisibleTime = 3f;

    [SerializeField] private FunnelBase rawFunnel;

    private string generatedCode = "";
    private string currentInput = "";

    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public Color defaultColor = Color.white;

    private bool showingCode = false;
    private float codeTimer = 0f;
    private bool inputLocked = false;
    private bool codeSystemActive = false;

    private WaveWinner currentWaveManager;

    [Header("Special Day 5")]
    public bool isSecretCodeCorrectDay5 = false;
    private readonly string secretCodeDay5 = "3729";
    private Coroutine day5HardcodedTimerCoroutine;

    void Start()
    {
        if (generatedCodeCanvas != null) generatedCodeCanvas.SetActive(false);
        if (inputDisplay != null) inputDisplay.text = "";
        codeSystemActive = false;
        showingCode = false;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            showingCode = false;
            return;
        }

        if (showingCode)
        {
            codeTimer += Time.deltaTime;
            if (codeTimer >= codeVisibleTime)
            {
                HideGeneratedCode();
            }
        }
    }

    public void GenerateCode()
    {
        generatedCode = "";
        currentInput = "";
        inputLocked = false;

        for (int i = 0; i < codeLength; i++)
        {
            generatedCode += Random.Range(0, 10).ToString();
        }

        ShowGeneratedCode();
    }

    void ShowGeneratedCode()
    {
        if (generatedCodeCanvas != null) generatedCodeCanvas.SetActive(true);

        generatedCodeText.color = defaultColor;
        generatedCodeText.text = generatedCode;

        showingCode = true;
        codeTimer = 0f;
    }

    void HideGeneratedCode()
    {
        showingCode = false;
        generatedCodeText.text = "";
        if (generatedCodeCanvas != null) generatedCodeCanvas.SetActive(false);
    }

    public void AddNumber(int number)
    {
        if (!codeSystemActive || inputLocked) return;
        if (currentInput.Length >= codeLength) return;

        currentInput += number.ToString();
        inputDisplay.text = currentInput;

        if (currentInput.Length == codeLength)
        {
            CheckCode();
        }
    }

    public void DeleteLastNumber()
    {
        if (inputLocked) return;
        if (currentInput.Length <= 0) return;

        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        inputDisplay.text = currentInput;
    }

    void CheckCode()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            if (currentInput == secretCodeDay5)
            {
                inputDisplay.color = correctColor;
                inputLocked = true;
                isSecretCodeCorrectDay5 = true;

                if (day5HardcodedTimerCoroutine != null)
                {
                    StopCoroutine(day5HardcodedTimerCoroutine);
                    day5HardcodedTimerCoroutine = null;
                }
            }
            else
            {
                inputLocked = true;
                inputDisplay.color = wrongColor;
                inputDisplay.text = "ERROR";
                StartCoroutine(ResetKeyboardForRetryRoutine());
            }
            return;
        }

        if (currentInput == generatedCode)
        {
            inputDisplay.color = correctColor;
            inputLocked = true;

            if (rawFunnel != null) rawFunnel.AddLiquid(20f);
            Invoke(nameof(EndEventSuccess), 1.5f);
        }
        else
        {
            inputLocked = true;
            inputDisplay.color = wrongColor;
            inputDisplay.text = "ERROR";
            Invoke(nameof(EndEventFail), 1.5f);
        }
    }

    private IEnumerator ResetKeyboardForRetryRoutine()
    {
        yield return new WaitForSeconds(1.2f);
        currentInput = "";
        if (inputDisplay != null)
        {
            inputDisplay.text = "";
            inputDisplay.color = defaultColor;
        }
        inputLocked = false;
    }

    public void ForceTimeoutFail()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5 && day5HardcodedTimerCoroutine != null)
        {
            return;
        }

        if (!codeSystemActive) return;

        inputLocked = true;
        showingCode = false;
        HideGeneratedCode();

        if (inputDisplay != null)
        {
            inputDisplay.color = wrongColor;
            inputDisplay.text = "ERROR";
        }

        Invoke(nameof(EndEventFail), 1.5f);
    }

    private void EndEventSuccess()
    {
        codeSystemActive = false;
        ResetInput();
        if (currentWaveManager != null) currentWaveManager.NotifyEventResult(true);
    }

    private void EndEventFail()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5 && day5HardcodedTimerCoroutine != null)
        {
            return;
        }

        codeSystemActive = false;
        ResetInput();

        if (currentWaveManager != null)
        {
            currentWaveManager.NotifyEventResult(false);
        }
        else
        {
            WaveWinner.OnEventFinished?.Invoke(false);
        }
    }

    public void EnableCodeSystem(WaveWinner manager)
    {
        currentWaveManager = manager;
        codeSystemActive = true;
        showingCode = false;

        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            currentInput = "";
            inputLocked = false;
            isSecretCodeCorrectDay5 = false;

            if (inputDisplay != null)
            {
                inputDisplay.text = "";
                inputDisplay.color = defaultColor;
            }

            if (day5HardcodedTimerCoroutine != null) StopCoroutine(day5HardcodedTimerCoroutine);
            day5HardcodedTimerCoroutine = StartCoroutine(Day5HardcodedTimerSequence());

            return;
        }

        GenerateCode();
    }

    private IEnumerator Day5HardcodedTimerSequence()
    {
        yield return new WaitForSeconds(30f);

        day5HardcodedTimerCoroutine = null;

        inputLocked = true;
        if (inputDisplay != null)
        {
            inputDisplay.color = wrongColor;
            inputDisplay.text = "ERROR";
        }

        codeSystemActive = false;
        ResetInput();
        WaveWinner.OnEventFinished?.Invoke(false);
    }

    public void ForceDisable()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5 && !isSecretCodeCorrectDay5)
        {
            if (day5HardcodedTimerCoroutine != null) return;
        }

        if (day5HardcodedTimerCoroutine != null)
        {
            StopCoroutine(day5HardcodedTimerCoroutine);
            day5HardcodedTimerCoroutine = null;
        }

        codeSystemActive = false;
        showingCode = false;
        HideGeneratedCode();
        ResetInput();
        isSecretCodeCorrectDay5 = false;
    }

    void ResetInput()
    {
        currentInput = "";
        inputDisplay.text = "";
        inputDisplay.color = defaultColor;
        inputLocked = false;
    }

    public bool IsCodeSystemActive() => codeSystemActive;
}
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

    void Start()
    {
        if (generatedCodeCanvas != null) generatedCodeCanvas.SetActive(false);
        if (inputDisplay != null) inputDisplay.text = "";
        codeSystemActive = false;
    }

    void Update()
    {
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

        //Debug.Log("<color=#FFFF00><b>[CODE SYSTEM]</b></color> Codigo Aleatorio Generado: " + generatedCode);
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

        //Debug.Log("<color=#80CAFF><b>[CODE INPUT]</b></color> Digito introducido: " + number + " | Entrada actual: " + currentInput);

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

        //Debug.Log("<color=#80CAFF><b>[CODE INPUT]</b></color> Borrado ultimo digito. Entrada actual: " + currentInput);
    }

    void CheckCode()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            //Debug.Log("<color=#FF80FF><b>[CODE VALIDATION]</b></color> Validando Dia 5. Entrada: " + currentInput + " | Requerido: " + secretCodeDay5);

            if (currentInput == secretCodeDay5)
            {
                //Debug.Log("<color=#00FF00><b>[CODE SUCCESS]</b></color> ¡Codigo Secreto Dia 5 Correcto!");
                inputDisplay.color = correctColor;
                inputLocked = true;
                isSecretCodeCorrectDay5 = true;
            }
            else
            {
                //Debug.Log("<color=#FF0000><b>[CODE ERROR]</b></color> Codigo Secreto Dia 5 Incorrecto.");
                inputLocked = true;
                inputDisplay.color = wrongColor;
                inputDisplay.text = "ERROR";
                StartCoroutine(ResetKeyboardForRetryRoutine());
            }
            return;
        }

        //Debug.Log("<color=#FF80FF><b>[CODE VALIDATION]</b></color> Validando Dia Normal. Entrada: " + currentInput + " | Requerido: " + generatedCode);

        if (currentInput == generatedCode)
        {
            //Debug.Log("<color=#00FF00><b>[CODE SUCCESS]</b></color> ¡Codigo Normal Correcto!");
            inputDisplay.color = correctColor;
            inputLocked = true;

            if (rawFunnel != null) rawFunnel.AddLiquid(20f);
            Invoke(nameof(EndEventSuccess), 1.5f);
        }
        else
        {
            //Debug.Log("<color=#FF0000><b>[CODE ERROR]</b></color> Codigo Normal Incorrecto.");
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
        //Debug.Log("<color=#80CAFF><b>[CODE SYSTEM]</b></color> Teclado restablecido para reintento.");
    }

    public void ForceTimeoutFail()
    {
        if (!codeSystemActive) return;

        inputLocked = true;
        showingCode = false;
        HideGeneratedCode();

        if (inputDisplay != null)
        {
            inputDisplay.color = wrongColor;
            inputDisplay.text = "ERROR";
        }

        //Debug.Log("<color=#FF0000><b>[CODE TIMEOUT]</b></color> Tiempo agotado. Forzando fallo.");
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
        codeSystemActive = false;
        ResetInput();
        if (currentWaveManager != null) currentWaveManager.NotifyEventResult(false);
    }

    public void EnableCodeSystem(WaveWinner manager)
    {
        currentWaveManager = manager;
        codeSystemActive = true;

        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            //Debug.Log("<color=#00FFFF><b>[CODE SYSTEM]</b></color> Teclado Inicializado en Dia 5. Esperando codigo secreto: " + secretCodeDay5);
            currentInput = "";
            inputLocked = false;
            if (inputDisplay != null)
            {
                inputDisplay.text = "";
                inputDisplay.color = defaultColor;
            }
            return;
        }

        //Debug.Log("<color=#00FFFF><b>[CODE SYSTEM]</b></color> Teclado Inicializado en Dia Normal. Generando clave aleatoria.");
        GenerateCode();
    }

    public void ForceDisable()
    {
        //Debug.Log("<color=#FF8000><b>[CODE SYSTEM]</b></color> Forzando apagado y desactivacion del teclado.");
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
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

    [Header("Configuración Especial Día 5")]
    // Flag que el MachineManager leerá en su IF para saber si el código 3729 se puso bien
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

        Debug.Log("CODIGO: " + generatedCode);
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
                isSecretCodeCorrectDay5 = true; // El MachineManager detectará esto en su IF

                Debug.Log("<color=#00FF00><b>[CÓDIGO SECRETO]</b></color> ¡Código 3729 correcto!");
            }
            else
            {
                inputLocked = true;
                inputDisplay.color = wrongColor;
                inputDisplay.text = "ERROR";

                Debug.Log("<color=#FF0000><b>[CÓDIGO SECRETO]</b></color> Código incorrecto. Reiniciando teclado...");

                // Iniciamos la rutina para limpiar el error y permitir reintentar sin romper el bucle
                StartCoroutine(ResetKeyboardForRetryRoutine());
            }
            return;
        }

        // --- COMPORTAMIENTO HABITUAL (Días 1 a 4) ---
        if (currentInput == generatedCode)
        {
            inputDisplay.color = correctColor;
            inputLocked = true;

            if (rawFunnel != null) rawFunnel.AddLiquid(20f);
            Debug.Log("CÓDIGO CORRECTO");

            Invoke(nameof(EndEventSuccess), 1.5f);
        }
        else
        {
            inputLocked = true;
            inputDisplay.color = wrongColor;
            inputDisplay.text = "ERROR";
            Debug.Log("CÓDIGO INCORRECTO");

            Invoke(nameof(EndEventFail), 1.5f);
        }
    }

    /// <summary>
    /// limpia la pantalla y vuelve a dejar el teclado desbloqueado para reintentar.
    /// </summary>
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
        if (!codeSystemActive) return;

        inputLocked = true;
        showingCode = false;
        HideGeneratedCode();

        if (inputDisplay != null)
        {
            inputDisplay.color = wrongColor;
            inputDisplay.text = "ERROR";
        }

        Debug.Log("TIEMPO AGOTADO EN EL TECLADO NUMÉRICO");
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

        // Si es el día 5, no generamos código aleatorio ni mostramos el canvas de ayuda
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            currentInput = "";
            inputLocked = false;
            if (inputDisplay != null)
            {
                inputDisplay.text = "";
                inputDisplay.color = defaultColor;
            }
            return;
        }

        GenerateCode();
    }

    public void ForceDisable()
    {
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
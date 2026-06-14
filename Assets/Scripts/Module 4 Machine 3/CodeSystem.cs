using UnityEngine;
using TMPro;

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
        if (currentInput == generatedCode)
        {
            inputDisplay.color = correctColor;
            inputLocked = true;

            if (rawFunnel != null) rawFunnel.AddLiquid(20f);

            Debug.Log("CÓDIGO CORRECTO");

            // [Parpadeo ÉXITO - Opcional]

            Invoke(nameof(EndEventSuccess), 1.5f);
        }
        else
        {
            inputLocked = true;
            inputDisplay.color = wrongColor;
            inputDisplay.text = "ERROR";

            Debug.Log("CÓDIGO INCORRECTO");

            // [Parpadeo ERROR - Opcional]

            Invoke(nameof(EndEventFail), 1.5f);
        }
    }

    /// <summary>
    /// ¡NUEVA ORDEN DE INYECCIÓN!: Si el reloj global de la alarma llega a cero antes de meter
    /// el código, forzamos la animación de fallo en rojo y congelamos la pantalla.
    /// </summary>
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

        // Lanzamos el fin del evento con retraso para ver el cartel rojo y reproducir la animación
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
        GenerateCode();
    }

    public void ForceDisable()
    {
        codeSystemActive = false;
        showingCode = false;
        HideGeneratedCode();
        ResetInput();
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
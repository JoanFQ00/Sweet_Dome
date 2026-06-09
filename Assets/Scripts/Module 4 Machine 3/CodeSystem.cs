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

    void Start()
    {
        if (generatedCodeCanvas != null)
            generatedCodeCanvas.SetActive(false);

        if (inputDisplay != null)
            inputDisplay.text = "";

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
        if (generatedCodeCanvas != null)
            generatedCodeCanvas.SetActive(true);

        generatedCodeText.color = defaultColor;
        generatedCodeText.text = generatedCode;

        showingCode = true;
        codeTimer = 0f;
    }

    void HideGeneratedCode()
    {
        showingCode = false;

        generatedCodeText.text = "";

        if (generatedCodeCanvas != null)
            generatedCodeCanvas.SetActive(false);
    }

    public void AddNumber(int number)
    {
        if (!codeSystemActive || inputLocked) return;
        if (currentInput.Length >= codeLength)
            return;

        currentInput += number.ToString();

        inputDisplay.text = currentInput;

        if (currentInput.Length == codeLength)
        {
            CheckCode();
        }
    }
    public void DeleteLastNumber()
    {
        if (inputLocked)
            return;

        if (currentInput.Length <= 0)
            return;

        currentInput = currentInput.Substring(0, currentInput.Length - 1);
        inputDisplay.text = currentInput;
    }

    void CheckCode()
    {
        if (currentInput == generatedCode)
        {
            inputDisplay.color = correctColor;

            if (rawFunnel != null)
                rawFunnel.AddLiquid(20f);

            Debug.Log("CORRECTO");
            codeSystemActive = false;
        }
        else
        {
            inputLocked = true;
            inputDisplay.color = wrongColor;
            inputDisplay.text = "ERROR";

            Debug.Log("INCORRECTO");

            Invoke(nameof(ResetInput), 1.5f);
        }
    }
    public void EnableCodeSystem()
    {
        codeSystemActive = true;
        GenerateCode();
    }

    void ResetInput()
    {
        currentInput = "";

        inputDisplay.text = "";
        inputDisplay.color = defaultColor;
    }
}
using UnityEngine;
using TMPro;

public class CodeSystem : MonoBehaviour
{
    [Header("UI")]
    public GameObject codeCanvas;
    public TextMeshProUGUI codeDisplay;

    [Header("Config")]
    public int codeLength = 4;
    [SerializeField] private FunnelBase rawFunnel;

    private string generatedCode = "";
    private string currentInput = "";

    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public Color defaultColor = Color.white;

    void Start()
    {
        if (codeCanvas != null)
            codeCanvas.SetActive(false);
    }

    public void GenerateCode()
    {
        generatedCode = "";
        currentInput = "";

        for (int i = 0; i < codeLength; i++)
        {
            generatedCode += Random.Range(0, 10).ToString();
        }

        Debug.Log("CODIGO: " + generatedCode);

        if (codeCanvas != null)
            codeCanvas.SetActive(true);

        UpdateDisplay();
    }

    public void AddNumber(int number)
    {
        if (currentInput.Length >= codeLength)
            return;

        currentInput += number.ToString();
        UpdateDisplay();

        if (currentInput.Length == codeLength)
        {
            CheckCode();
        }
    }

    void UpdateDisplay()
    {
        codeDisplay.text = currentInput;
        codeDisplay.color = defaultColor;
    }

    void CheckCode()
    {
        if (currentInput == generatedCode)
        {
            codeDisplay.color = correctColor;
            rawFunnel.AddLiquid(20f);
            Debug.Log("CORRECTO");
        }
        else
        {
            codeDisplay.color = wrongColor;
            Debug.Log("INCORRECTO");

            Invoke(nameof(ResetInput), 1.5f);
        }
    }

    void ResetInput()
    {
        currentInput = "";
        UpdateDisplay();
    }
}
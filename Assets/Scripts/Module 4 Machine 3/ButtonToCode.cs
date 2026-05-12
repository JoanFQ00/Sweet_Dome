using UnityEngine;

public class ButtonToCode : MonoBehaviour
{
    public MainButt mainButt;
    public NumberButton numberSrc;
    public CodeSystem codeSystem; 

    void Awake()
    {
        if (mainButt != null)
        {
            mainButt.onPress.AddListener(SendNumber);
        }
    }

    void OnDestroy()
    {
        if (mainButt != null)
        {
            mainButt.onPress.RemoveListener(SendNumber);
        }
    }

    void SendNumber()
    {
        if (codeSystem == null || numberSrc == null) return;

        codeSystem.AddNumber(numberSrc.number);
    }
}
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

        if (codeSystem.IsCodeSystemActive())
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.frequencyNumberButton, gameObject);
            }
        }

        codeSystem.AddNumber(numberSrc.number);
    }
}
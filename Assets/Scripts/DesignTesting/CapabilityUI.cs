using TMPro;
using UnityEngine;

public class CapabilityUI : MonoBehaviour
{
    [SerializeField] private CapabilityBase capabilityBase;
    [SerializeField] private TextMeshProUGUI text;

    void Start()
    {
        if (capabilityBase != null)
            capabilityBase.OnCapacityUpdated += UpdateUI;
    }

    private void OnDisable()
    {
        if (capabilityBase != null)
            capabilityBase.OnCapacityUpdated -= UpdateUI;
    }

    void UpdateUI(float value)
    {
        text.text = Mathf.RoundToInt(value).ToString();
    }
}
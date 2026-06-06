using TMPro;
using UnityEngine;

public class CapabilityUI : MonoBehaviour
{
    public enum CapabilityType
    {
        NORMAL = 0,
        CITY,
        BIG_CITY
    }

    [SerializeField] private CapabilityType capabilityType;

    [SerializeField] private CapabilityBase capabilityBase;
    [SerializeField] private TextMeshProUGUI capabilityText, nameText;

    private void Start()
    {
        StartCoroutine(DelayedInit());
    }

    private System.Collections.IEnumerator DelayedInit()
    {
        yield return null;

        UpdateCapabilityUI(capabilityBase.CurrentCapacity);

        if (capabilityType == CapabilityType.BIG_CITY)
        {
            CityFunnel cityFunnel = capabilityBase.GetComponent<CityFunnel>();
            if (cityFunnel != null)
            {
                UpdateCityNameUI(cityFunnel.GetInitialName());
            }
        }
    }

    private void OnEnable()
    {
        if (capabilityBase == null) return;

        capabilityBase.OnCapacityUpdated += UpdateCapabilityUI;
        if (capabilityType == CapabilityType.BIG_CITY)
            capabilityBase.OnNameUpdated += UpdateCityNameUI;

        UpdateCapabilityUI(capabilityBase.CurrentCapacity);

        if (capabilityType == CapabilityType.BIG_CITY)
        {
            CityFunnel cityFunnel = capabilityBase.GetComponent<CityFunnel>();
            if (cityFunnel != null)
            {
                UpdateCityNameUI(cityFunnel.GetInitialName());
            }
        }
    }

    private void OnDisable()
    {
        if (capabilityBase == null)
            return;

        capabilityBase.OnCapacityUpdated -= UpdateCapabilityUI;

        if (capabilityType == CapabilityType.BIG_CITY)
            capabilityBase.OnNameUpdated -= UpdateCityNameUI;
    }

    private void UpdateCapabilityUI(float value)
    {
        capabilityText.text =
            Mathf.RoundToInt(value).ToString();
    }

    private void UpdateCityNameUI(string name)
    {
        if (nameText != null)
            nameText.text = name;
    }
}
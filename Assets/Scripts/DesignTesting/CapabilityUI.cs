using TMPro;
using UnityEngine;

public class CapabilityUI : MonoBehaviour, IResetable
{
    public enum CapabilityType { NORMAL = 0, CITY, BIG_CITY }

    [SerializeField] private CapabilityType capabilityType;
    [SerializeField] private CapabilityBase capabilityBase;
    [SerializeField] private TextMeshProUGUI capabilityText, nameText;

    private void Start()
    {
        // REGISTRO DE UI: Las pantallas se apuntan al manager síncronamente
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    private void OnEnable()
    {
        if (capabilityBase == null) return;

        capabilityBase.OnCapacityUpdated -= UpdateCapabilityUI;
        capabilityBase.OnCapacityUpdated += UpdateCapabilityUI;

        if (capabilityType == CapabilityType.BIG_CITY)
        {
            capabilityBase.OnNameUpdated -= UpdateCityNameUI;
            capabilityBase.OnNameUpdated += UpdateCityNameUI;
        }

        ResetValues();
    }

    private void OnDisable()
    {
        if (capabilityBase == null) return;

        capabilityBase.OnCapacityUpdated -= UpdateCapabilityUI;

        if (capabilityType == CapabilityType.BIG_CITY)
            capabilityBase.OnNameUpdated -= UpdateCityNameUI;
    }

    public void ResetValues()
    {
        if (capabilityBase == null) return;

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

    private void UpdateCapabilityUI(float value)
    {
        if (capabilityText != null)
            capabilityText.text = Mathf.RoundToInt(value).ToString();
    }

    private void UpdateCityNameUI(string name)
    {
        if (nameText != null)
            nameText.text = name;
    }
}
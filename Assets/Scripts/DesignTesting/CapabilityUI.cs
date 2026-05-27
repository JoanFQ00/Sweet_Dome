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

    void Start()
    {
        if (capabilityBase != null)
        {
            capabilityBase.OnCapacityUpdated += UpdateCapabilityUI;
            //UpdateCapabilityUI(capabilityBase.CurrentCapacity);

            if (capabilityType != CapabilityType.NORMAL)
            {
                if (capabilityType == CapabilityType.CITY)
                    UpdateCityNameUI(capabilityBase.GetComponent<CityLabel>().label);
                else
                {
                    capabilityBase.OnNameUpdated += UpdateCityNameUI;
                    UpdateCapabilityUI(capabilityBase.gameObject.GetComponent<CityFunnel>().GetInitialValue());
                    UpdateCityNameUI(capabilityBase.gameObject.GetComponent<CityFunnel>().GetInitialName());
                }
            }
        }
    }

    private void OnDisable()
    {
        if (capabilityBase != null)
        {
            capabilityBase.OnCapacityUpdated -= UpdateCapabilityUI;

            if (capabilityType != CapabilityType.NORMAL)
            {
                if (capabilityType == CapabilityType.CITY)
                    UpdateCityNameUI(capabilityBase.GetComponent<CityLabel>().label);
                else
                {
                    capabilityBase.OnNameUpdated -= UpdateCityNameUI;
                    UpdateCapabilityUI(capabilityBase.gameObject.GetComponent<CityFunnel>().GetInitialValue());
                    UpdateCityNameUI(capabilityBase.gameObject.GetComponent<CityFunnel>().GetInitialName());
                }
            }
        }
    }

    void UpdateCapabilityUI(float value)
    {
        capabilityText.text = Mathf.RoundToInt(value).ToString();
    }

    void UpdateCityNameUI(string name)
    {
        nameText.text = name;
    }
}
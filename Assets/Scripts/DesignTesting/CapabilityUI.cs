using TMPro;
using UnityEngine;

public class CapabilityUI : MonoBehaviour, IResetable
{
    public enum CapabilityType { NORMAL = 0, CITY, BIG_CITY }

    [SerializeField] private CapabilityType capabilityType;
    [SerializeField] private CapabilityBase capabilityBase;
    [SerializeField] private TextMeshProUGUI capabilityText, nameText;

    [Header("Polish Counter Settings")]
    [SerializeField] private float numberLerpSpeed = 5f; // Velocidad de la transición del número

    private float targetCapacity;
    private float currentVisualCapacity;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }
    }

    private void Update()
    {
        // Si el número visual actual no coincide con el objetivo real, interpolamos
        if (Mathf.Abs(currentVisualCapacity - targetCapacity) > 0.01f)
        {
            currentVisualCapacity = Mathf.Lerp(currentVisualCapacity, targetCapacity, numberLerpSpeed * Time.deltaTime);
            SetTextValue(currentVisualCapacity);
        }
        else if (currentVisualCapacity != targetCapacity)
        {
            currentVisualCapacity = targetCapacity;
            SetTextValue(currentVisualCapacity);
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

        targetCapacity = capabilityBase.CurrentCapacity;
        currentVisualCapacity = capabilityBase.CurrentCapacity;
        SetTextValue(currentVisualCapacity);

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
        // Al recibir el evento, actualizamos el objetivo matemático para que el Update empiece el lerp
        targetCapacity = value;
    }

    private void SetTextValue(float value)
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
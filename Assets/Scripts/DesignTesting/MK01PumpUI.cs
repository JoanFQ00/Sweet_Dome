using UnityEngine;
using TMPro;

public class MK01PumpUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Machine01 machineSystem;

    [Header("UI Elements (Text)")]
    [SerializeField] private TextMeshProUGUI textDisplay;

    [Header("UI GameObjects (Toggles)")]
    [SerializeField] private GameObject purifiedPumpImageGO;
    [SerializeField] private GameObject rawPumpImageGO;

    [Header("Visual Interpolation (Lerp)")]
    [SerializeField] private float lerpSpeed = 15f;

    private LiquidPump currentTrackedPump;
    private float visualCapacityValue;
    private bool shouldLerp = false;

    private void OnEnable()
    {
        if (machineSystem != null)
        {
            machineSystem.OnPumpSwapped += HandlePumpSwapped;
        }
    }

    private void OnDisable()
    {
        if (machineSystem != null)
        {
            machineSystem.OnPumpSwapped -= HandlePumpSwapped;
        }

        if (currentTrackedPump != null)
        {
            currentTrackedPump.OnCapacityUpdated -= HandleCapacityChanged;
        }
    }

    private void Update()
    {
        if (shouldLerp && currentTrackedPump != null)
        {
            visualCapacityValue = Mathf.MoveTowards(visualCapacityValue, currentTrackedPump.CurrentCapacity, lerpSpeed * 10f * Time.deltaTime);
            textDisplay.text = Mathf.RoundToInt(visualCapacityValue).ToString();

            if (Mathf.Abs(visualCapacityValue - currentTrackedPump.CurrentCapacity) < 0.5f)
            {
                visualCapacityValue = currentTrackedPump.CurrentCapacity;
                textDisplay.text = Mathf.RoundToInt(visualCapacityValue).ToString();
                shouldLerp = false;
            }
        }
    }

    private void HandlePumpSwapped(LiquidPump newActivePump)
    {
        if (currentTrackedPump != null)
        {
            currentTrackedPump.OnCapacityUpdated -= HandleCapacityChanged;
        }

        currentTrackedPump = newActivePump;

        if (currentTrackedPump != null)
        {
            currentTrackedPump.OnCapacityUpdated += HandleCapacityChanged;
            shouldLerp = true;

            TogglePumpImages(newActivePump.gameObject.name);
        }
    }

    private void HandleCapacityChanged(float currentRealCapacity)
    {
        shouldLerp = true;
    }

    private void TogglePumpImages(string pumpName)
    {
        if (purifiedPumpImageGO == null || rawPumpImageGO == null) return;

        if (pumpName.ToLower().Contains("purified"))
        {
            purifiedPumpImageGO.SetActive(true);
            rawPumpImageGO.SetActive(false);
        }
        else
        {
            purifiedPumpImageGO.SetActive(false);
            rawPumpImageGO.SetActive(true);
        }
    }
}
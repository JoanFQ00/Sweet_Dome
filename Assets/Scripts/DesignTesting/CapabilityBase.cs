using System;
using UnityEngine;

public abstract class CapabilityBase : MonoBehaviour
{
    [Header("Capability")]
    [SerializeField] protected float maxCapacity = 100f;
    [SerializeField] protected float minCapacity = 0f;
    [SerializeField] protected float currentCapacity;

    public Action<float> OnCapacityUpdated;
    public Action<string> OnNameUpdated;

    protected virtual void Start()
    {
        StartDay();
    }

    public void StartDay()
    {
        currentCapacity = GetInitialCapacity(GameManager.Instance.currentDay);
        CapacityUpdated();
    }

    protected void UpdateCapacity(float amount)
    {
        currentCapacity += amount;
        currentCapacity = Mathf.Clamp(currentCapacity, minCapacity, maxCapacity);

        if (Mathf.Abs(currentCapacity - Mathf.Round(currentCapacity)) < 0.01f)
            currentCapacity = Mathf.Round(currentCapacity);

        CapacityUpdated();
    }

    protected void CapacityUpdated()
    {
        OnCapacityUpdated?.Invoke(currentCapacity);
    }

    protected abstract float GetInitialCapacity(Day day);
}
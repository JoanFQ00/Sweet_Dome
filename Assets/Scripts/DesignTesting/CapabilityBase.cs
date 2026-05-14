using System;
using System.Collections;
using UnityEngine;

public abstract class CapabilityBase : MonoBehaviour
{
    [Header("Capability")]
    [SerializeField] protected float maxCapacity = 100f;
    [SerializeField] protected float minCapacity = 0f;
    [SerializeField] protected float currentCapacity;

    public Action<float> OnCapacityUpdated;
    public Action<string> OnNameUpdated;

    public float MaxCapacity => maxCapacity;
    public float MinCapacity => minCapacity;
    public float CurrentCapacity => currentCapacity;

    protected virtual void Start()
    {
        StartCoroutine(DelayedStartDay());
        GameManager.NewDay += StartDay; //CAMBIAR
    }

    private void OnDisable()
    {
        GameManager.NewDay -= StartDay;
    }

    private IEnumerator DelayedStartDay()
    {
        yield return null;
        StartDay();
    }

    public void StartDay()
    {
        currentCapacity = PutInitialCapacity(GameManager.Instance.currentDay);
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

    protected abstract float PutInitialCapacity(Day day);
}
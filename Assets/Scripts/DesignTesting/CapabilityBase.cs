using System;
using System.Collections;
using UnityEngine;

public abstract class CapabilityBase : MonoBehaviour, IResetable
{
    // INTERRUPTOR GLOBAL: Si es true, congela el volumen de líquido de todo el juego
    public static bool IsGlobalFlowPaused = false;

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
        // REGISTRO AUTOMÁTICO: Le dice al GameManager "Aquí estoy, apúntame"
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        ResetValues();
    }

    protected virtual void OnDestroy()
    {
        // LIMPIEZA AUTOMÁTICA: Si el objeto se destruye, se borra de la lista
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    protected virtual void OnDisable()
    {
        // Limpieza de seguridad secundaria
    }

    public void ResetValues()
    {
        currentCapacity = PutInitialCapacity(GameManager.Instance.currentDay);
        CapacityUpdated();
        OnStartDayFinished();
    }

    protected virtual void OnStartDayFinished() { }

    protected void UpdateCapacity(float amount)
    {
        if (IsGlobalFlowPaused)
            return;

        currentCapacity += amount;
        currentCapacity = Mathf.Clamp(currentCapacity, minCapacity, maxCapacity);

        CapacityUpdated();
        OnCapacityChanged();
    }

    protected virtual void OnCapacityChanged() { }

    protected void CapacityUpdated()
    {
        OnCapacityUpdated?.Invoke(currentCapacity);
    }

    protected abstract float PutInitialCapacity(Day day);
}
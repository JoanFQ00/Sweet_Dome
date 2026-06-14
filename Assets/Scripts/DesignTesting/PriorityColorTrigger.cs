using UnityEngine;

public class PriorityColorTrigger : CustomButtonColorTrigger
{
    [Header("Suscripción por Referencia de Escena")]
    [Tooltip("Arrastra aquí el script o botón PriorityPress que disparará la señal.")]
    [SerializeField] private PriorityPress priorityButtonSource;

    private void OnEnable()
    {
        if (priorityButtonSource != null)
        {
            priorityButtonSource.OnPriorityButtonPressed += HandlePriorityPressed;
        }
    }

    private void OnDisable()
    {
        if (priorityButtonSource != null)
        {
            priorityButtonSource.OnPriorityButtonPressed -= HandlePriorityPressed;
        }
    }

    protected override void Start()
    {
        base.Start();
        EvaluateDayRestrictions();
    }

    private void HandlePriorityPressed()
    {
        // Si es el Día 1, el botón está muerto. Abortamos cualquier cambio de color o pulsación estética
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day1)
        {
            base.RestoreOriginalBehavior();
            return;
        }

        // Si estamos en el Día 2 o posteriores, inyectamos el color HDR en el sistema de doble material
        if (targetButton != null)
        {
            targetButton.OverrideEmissionColor(customHdrColor);
        }
    }

    private void EvaluateDayRestrictions()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day1)
        {
            base.RestoreOriginalBehavior();
        }
    }

    public override void ResetValues()
    {
        base.ResetValues();
        EvaluateDayRestrictions();
    }
}
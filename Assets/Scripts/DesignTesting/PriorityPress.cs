using UnityEngine;
using System;

public class PriorityPress : MainButt
{
    [ColorUsage(true, true)]
    [Tooltip("El color HDR que se quedará fijo en el botón todo el día tras haber sido pulsado.")]
    public Color activeEmissionColor = Color.white;

    private CityPriorityManager cityPriorityManager;
    private ButtonEmissionController emissionController;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    // CAMBIO: Ya no es estático global. Ahora es un evento público de instancia para suscripción directa por referencia.
    public event Action OnPriorityButtonPressed;

    // Mantengo tu acción estática antigua por si otros scripts de tu juego (como el gestor de ciudad) la siguen necesitando
    public static Action<Color, int> OnPressedButton;

    protected override void Awake()
    {
        base.Awake();

        cityPriorityManager = GetComponent<CityPriorityManager>();
        emissionController = GetComponent<ButtonEmissionController>();

        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    public override void Press()
    {
        base.Press();

        if (!wasLastPressValid)
            return;

        if (emissionController != null)
        {
            emissionController.OverrideEmissionColor(activeEmissionColor);
        }

        // =================================================================
        // NUEVO: Invocamos el evento público local de este botón específico
        // =================================================================
        OnPriorityButtonPressed?.Invoke();

        // Lógica global antigua
        if (cityPriorityManager != null)
        {
            OnPressedButton?.Invoke(activeEmissionColor, cityPriorityManager.GetIdxPrioritizedZone());
        }
    }

    public override void ResetValues()
    {
        if (legacyAnimation != null)
        {
            legacyAnimation.Stop();
        }

        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;

        currentVisualState = VisualButtonState.Up;
        isBeingHeld = false;
    }
}
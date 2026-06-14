using UnityEngine;

// Hereda de la base que creamos para triggers de color
public class PriorityColorTrigger : CustomButtonColorTrigger
{
    [Header("Suscripción por Referencia de Escena")]
    [Tooltip("Arrastra aquí el botón de prioridad que va a disparar el cambio de color.")]
    [SerializeField] private PriorityPress priorityButtonSource;

    private void OnEnable()
    {
        // Nos suscribimos al evento público de la instancia específica arrastrada
        if (priorityButtonSource != null)
        {
            priorityButtonSource.OnPriorityButtonPressed += HandlePriorityPressed;
        }
    }

    private void OnDisable()
    {
        // Nos desuscribimos de la instancia de forma segura
        if (priorityButtonSource != null)
        {
            priorityButtonSource.OnPriorityButtonPressed -= HandlePriorityPressed;
        }
    }

    /// <summary>
    /// Este método se ejecuta automáticamente cuando el botón de la escena invoca el evento.
    /// </summary>
    private void HandlePriorityPressed()
    {
        // Llamamos al método público del padre (CustomButtonColorTrigger) para cambiar el color
        base.ChangeToCustomColor();
    }

    protected override void Start()
    {
        // Ejecutamos el registro base del IResetable en el GameManager
        base.Start();
    }
}
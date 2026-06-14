using UnityEngine;

public class PumpColorTrigger : CustomButtonColorTrigger
{
    [Header("Suscripción por Referencia de Máquina")]
    [Tooltip("Arrastra aquí el script Machine01 de la escena que maneja el cambio de bombas.")]
    [SerializeField] private Machine01 machineSource;

    [Header("Configuración Bicolor Inteligente")]
    [ColorUsage(true, true)]
    [Tooltip("Color HDR cuando la máquina está en modo PURIFICADO.")]
    [SerializeField] private Color purifiedHdrColor = Color.cyan;

    [ColorUsage(true, true)]
    [Tooltip("Color HDR cuando la máquina está en modo CRUDO (Raw).")]
    [SerializeField] private Color rawHdrColor = Color.red;

    private void OnEnable()
    {
        if (machineSource != null)
        {
            machineSource.OnPumpSwapped += HandlePumpSwapped;
        }
    }

    private void OnDisable()
    {
        if (machineSource != null)
        {
            machineSource.OnPumpSwapped -= HandlePumpSwapped;
        }
    }

    /// <summary>
    /// Se ejecuta automáticamente cada vez que la máquina cambia o se inicializa al arrancar.
    /// </summary>
    private void HandlePumpSwapped(LiquidPump activePump)
    {
        if (targetButton == null || machineSource == null) return;

        // NUEVA REGLA DE ORO: Si el botón tiene un validador y dice que NO se puede pulsar (CanPress == false),
        // o si es el Día 1 y quieres que esté apagado, forzamos el color NEGRO (apagado).
        if (targetButton.Validator != null && !targetButton.Validator.CanPress())
        {
            targetButton.OverrideEmissionColor(Color.black);
            return; // Cortamos aquí, no dejamos que se encienda el color HDR
        }

        // Si el validador da el visto bueno, aplicamos el flujo normal bicolor
        Color selectedColor = machineSource.IsPurifiedMode ? purifiedHdrColor : rawHdrColor;

        // Forzamos el color correspondiente en el controlador de emisión del botón
        targetButton.OverrideEmissionColor(selectedColor);
    }

    protected override void Start()
    {
        base.Start();

        // Forzamos un refresco manual al arrancar la escena para que aplique la regla del validador desde el frame 1
        if (machineSource != null)
        {
            HandlePumpSwapped(null);
        }
    }

    // Al cambiar de día, el GameManager llamará a ResetValues a través de la clase base
    public override void ResetValues()
    {
        base.ResetValues();
        // Volvemos a evaluar el color tras el salto de día por si el validador ya permite pulsar
        if (machineSource != null)
        {
            HandlePumpSwapped(null);
        }
    }
}
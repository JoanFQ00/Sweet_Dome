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

        // Leemos la propiedad pública de la máquina para decidir qué color HDR inyectar
        Color selectedColor = machineSource.IsPurifiedMode ? purifiedHdrColor : rawHdrColor;

        // Forzamos el color correspondiente en el controlador de emisión del botón
        targetButton.OverrideEmissionColor(selectedColor);
    }

    protected override void Start()
    {
        // Aseguramos el registro en el GameManager para que se limpie al pasar el día
        base.Start();
    }
}
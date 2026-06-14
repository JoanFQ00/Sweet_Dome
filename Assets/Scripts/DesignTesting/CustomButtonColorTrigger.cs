using UnityEngine;

public class CustomButtonColorTrigger : MonoBehaviour, IResetable
{
    [Header("Referencias Base")]
    [Tooltip("El botón al que le quieres cambiar el color.")]
    [SerializeField] protected ButtonEmissionController targetButton;

    [Header("Configuración de Colores Base")]
    // AÑADIDO: Esto transforma la casilla normal en la casilla HDR que ves en tu imagen
    [ColorUsage(true, true)]
    [Tooltip("El color e intensidad HDR que quieres inyectarle al botón.")]
    [SerializeField] protected Color customHdrColor = Color.blue;

    protected virtual void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    /// <summary>
    /// Cambia el botón al color HDR personalizado.
    /// </summary>
    public virtual void ChangeToCustomColor()
    {
        if (targetButton != null)
        {
            targetButton.OverrideEmissionColor(customHdrColor);
        }
    }

    /// <summary>
    /// Devuelve el botón a su comportamiento original por validador.
    /// </summary>
    public virtual void RestoreOriginalBehavior()
    {
        if (targetButton != null)
        {
            targetButton.ClearOverride();
        }
    }

    public virtual void ResetValues()
    {
        RestoreOriginalBehavior();
    }
}
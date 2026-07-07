using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(Animation))]
public class RedButtonAnims : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private EventReference pressSound;

    [Header("Animation Clip")]
    [SerializeField] private AnimationClip pressClip;

    [Header("Time Reaction Speed")]
    [SerializeField] private float pressSpeedMultiplier = 1.0f;

    private Animation legacyAnimation;

    private void Awake()
    {
        legacyAnimation = GetComponent<Animation>();

        // Registramos el clip en el componente Legacy Animation si están configurados
        if (legacyAnimation != null && pressClip != null)
        {
            legacyAnimation.AddClip(pressClip, pressClip.name);
        }
    }

    /// <summary>
    /// Función pública para activar el botón desde cualquier otro script.
    /// </summary>
    public void TriggerButtonPress()
    {
        // 1. Reproducir Sonido FMOD
        if (!pressSound.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(pressSound, gameObject);
        }

        // 2. Reproducir Animación Legacy
        if (legacyAnimation != null && pressClip != null)
        {
            legacyAnimation.Stop();
            if (legacyAnimation[pressClip.name] != null)
            {
                legacyAnimation[pressClip.name].speed = pressSpeedMultiplier;
            }
            legacyAnimation.Play(pressClip.name);
        }
    }
}
using UnityEngine;
using System;

public class PriorityPress : MainButt
{
    [ColorUsage(true, true)]
    public Color activeEmissionColor = Color.white;

    private CityPriorityManager cityPriorityManager;
    private ButtonEmissionController emissionController;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    public event Action OnPriorityButtonPressed;

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

        // Si la pulsación no es válida, salimos sin hacer nada (y sin sonido)
        if (!wasLastPressValid)
            return;

        if (SoundManagerOwn.Instance != null && SoundManagerOwn.Instance.priorizedZoneSFX != null)
        {
            AudioSource buttonAudio = SoundManagerOwn.Instance.Create3DAudioSource(
                SoundManagerOwn.Instance.priorizedZoneSFX,
                transform.position,
                false
            );

            if (buttonAudio != null)
            {
                buttonAudio.Play();
                // Destruye el GameObject de audio temporal al finalizar el sonido
                Destroy(buttonAudio.gameObject, SoundManagerOwn.Instance.priorizedZoneSFX.length);
            }
        }

        if (emissionController != null)
        {
            emissionController.OverrideEmissionColor(activeEmissionColor);
        }

        OnPriorityButtonPressed?.Invoke();

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
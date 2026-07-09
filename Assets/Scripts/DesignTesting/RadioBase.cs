using System.Collections.Generic;
using UnityEngine;

public class RadioBase : MonoBehaviour, IInteractable, IResetable
{
    public enum MusicType
    {
        RADIO = 0,
        TURNTABLE
    }

    [SerializeField] private MusicType musicType;
    [SerializeField] private List<Animator> vinylAnimatorList;

    [Header("State")]
    [SerializeField] private bool isTurnedOn = false;
    [SerializeField] private GameObject pivotSound;

    private AudioSource myAudioSource;

    void Start()
    {
        // Dejamos que GameManager llene el mapa y llame a ResetValues() 
        // cuando el estado del día esté completamente asentado en memoria.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }
    }

    public void Interact()
    {
        isTurnedOn = !isTurnedOn;

        if (isTurnedOn)
        {
            TurnOn();
        }
        else
        {
            TurnOff();
        }
    }

    private void TurnOn()
    {
        if (musicType == MusicType.TURNTABLE)
        {
            foreach (Animator vinylAnimator in vinylAnimatorList)
            {
                if (vinylAnimator != null) vinylAnimator.SetTrigger("On");
            }
        }

        if (myAudioSource != null) myAudioSource.UnPause();
    }

    private void TurnOff()
    {
        if (musicType == MusicType.TURNTABLE)
        {
            foreach (Animator vinylAnimator in vinylAnimatorList)
            {
                if (vinylAnimator != null) vinylAnimator.SetTrigger("Off");
            }
        }

        if (myAudioSource != null) myAudioSource.Pause();
    }

    private void PlayMusicByDay()
    {
        if (GameManager.Instance == null || SoundManagerOwn.Instance == null) return;

        Day currentDay = GameManager.Instance.currentDay;

        if (musicType == MusicType.RADIO)
        {
            switch (currentDay)
            {
                case Day.Day1:
                case Day.Day2:
                case Day.Day3:
                case Day.Day4:
                case Day.Day5:
                    PlaySound(SoundManagerOwn.Instance.tvSFX, true);
                    break;
            }
        }
        else // TURNTABLE
        {
            switch (currentDay)
            {
                case Day.Day1:
                    PlaySound(SoundManagerOwn.Instance.gameplayMusic01, true);
                    break;
                case Day.Day2:
                    PlaySound(SoundManagerOwn.Instance.gameplayMusic02, true);
                    break;
                case Day.Day3:
                    PlaySound(SoundManagerOwn.Instance.gameplayMusic03, true);
                    break;
                case Day.Day4:
                    PlaySound(SoundManagerOwn.Instance.gameplayMusic04, true);
                    break;
                case Day.Day5:
                    PlaySound(SoundManagerOwn.Instance.gameplayMusic05, true);
                    break;
            }
        }
    }

    void PlaySound(AudioClip audio, bool inLoop)
    {
        if (audio == null) return;

        if (myAudioSource != null)
        {
            myAudioSource.Stop();
            Destroy(myAudioSource.gameObject);
            myAudioSource = null;
        }

        if (musicType == MusicType.TURNTABLE)
        {
            myAudioSource = SoundManagerOwn.Instance.Create3DAudioSource(audio, transform.position, inLoop);
        }
        else
        {
            if (pivotSound != null)
            {
                myAudioSource = SoundManagerOwn.Instance.Create3DAudioSource(audio, pivotSound.transform.position, inLoop);
            }
        }

        if (myAudioSource != null)
        {
            myAudioSource.Play();

            if (!isTurnedOn)
            {
                myAudioSource.Pause();
            }
        }
    }

    public void ResetValues()
    {
        // Al iniciar o resetear el día, definimos el estado por defecto
        isTurnedOn = true;

        // Sincronizamos los animadores si arranca encendido por defecto
        if (musicType == MusicType.TURNTABLE)
        {
            foreach (Animator vinylAnimator in vinylAnimatorList)
            {
                if (vinylAnimator != null) vinylAnimator.SetTrigger("On");
            }
        }

        PlayMusicByDay();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }

        if (myAudioSource != null)
        {
            Destroy(myAudioSource.gameObject);
        }
    }
}
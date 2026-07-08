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
        ResetValues();
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
                vinylAnimator.SetTrigger("On");
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
                vinylAnimator.SetTrigger("Off");
            }
        }

        if (myAudioSource != null) myAudioSource.Pause();
    }

    private void PlayMusicByDay()
    {
        if (GameManager.Instance == null) return;

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
                case Day.Day2:
                case Day.Day3:
                case Day.Day4:
                case Day.Day5:
                    PlaySound(SoundManagerOwn.Instance.gameplayMusic, true);
                    break;
            }
        }
    }

    void PlaySound(AudioClip audio, bool inLoop)
    {
        // detenemos su reproducción y destruimos su GameObject antes de crear el nuevo.
        if (myAudioSource != null)
        {
            myAudioSource.Stop();
            Destroy(myAudioSource.gameObject);
            myAudioSource = null;
        }

        if (musicType == MusicType.TURNTABLE)
        {
            foreach (Animator vinylAnimator in vinylAnimatorList)
            {
                vinylAnimator.SetTrigger("On");
            }
            myAudioSource = SoundManagerOwn.Instance.Create3DAudioSource(audio, transform.position, inLoop);
        }
        else
        {
            myAudioSource = SoundManagerOwn.Instance.Create3DAudioSource(audio, pivotSound.transform.position, inLoop);
        }

        if (myAudioSource != null)
        {
            myAudioSource.Play();

            // Si por un casual al iniciar el día la radio debiera estar apagada por defecto,
            // pausamos el audio inmediatamente para respetar el booleano 'isTurnedOn'
            if (!isTurnedOn)
            {
                myAudioSource.Pause();
            }
        }
    }

    public void ResetValues()
    {
        isTurnedOn = true;
        PlayMusicByDay();
    }

    private void OnDestroy()
    {
        // Limpieza preventiva al salir de la escena
        if (myAudioSource != null)
        {
            Destroy(myAudioSource.gameObject);
        }
    }
}
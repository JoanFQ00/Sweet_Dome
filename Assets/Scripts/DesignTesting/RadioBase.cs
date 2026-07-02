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

        myAudioSource.UnPause();
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

        myAudioSource.Pause();
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
                    PlaySound(SoundManagerOwn.Instance.tvSFX, true);
                    break;
                case Day.Day2:
                    PlaySound(SoundManagerOwn.Instance.tvSFX, true);
                    break;
                case Day.Day3:
                    PlaySound(SoundManagerOwn.Instance.tvSFX, true);
                    break;
                case Day.Day4:
                    PlaySound(SoundManagerOwn.Instance.tvSFX, true);
                    break;
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
                    PlaySound(SoundManagerOwn.Instance.gameplayMusic, true);
                    break;
                case Day.Day2:
                    PlaySound(SoundManagerOwn.Instance.gameplayMusic, true);
                    break;
                case Day.Day3:
                    PlaySound(SoundManagerOwn.Instance.gameplayMusic, true);
                    break;
                case Day.Day4:
                    PlaySound(SoundManagerOwn.Instance.gameplayMusic, true);
                    break;
                case Day.Day5:
                    PlaySound(SoundManagerOwn.Instance.gameplayMusic, true);
                    break;
            }
        }
    }

    void PlaySound(AudioClip audio, bool inLoop)
    {
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

        myAudioSource.Play();
    }

    public void ResetValues()
    {
        isTurnedOn = true;
        PlayMusicByDay();
    }
}
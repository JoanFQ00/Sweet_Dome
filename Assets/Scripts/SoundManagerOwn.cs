using UnityEngine;

public class SoundManagerOwn : MonoBehaviour
{
    public static SoundManagerOwn Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource SFXSource;

    [Header("Music Tracks")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;
    public AudioClip creditsMusic;

    [Header("Character Dialogue SFX")]
    public AudioClip talkAngrySFX;
    public AudioClip talkHesitantSFX;
    public AudioClip talkLoveSFX;
    public AudioClip talkNormalSFX;

    [Header("President Dialogue SFX")]
    public AudioClip presidentAngrySFX;
    public AudioClip presidentHappySFX;
    public AudioClip presidentNormalSFX;

    [Header("Environment & Interactivity")]
    public AudioClip drinkCoffeeSFX;
    public AudioClip rotateWheelSFX;
    public AudioClip tvSFX;

    [Header("Pumps")]
    public AudioClip absorbSFX;
    public AudioClip expelSFX;
    
    [Header("Other")]
    public AudioClip eatDorothyCapkakeSFX;
    public AudioClip recieveLiquidSFX;
    public AudioClip priorizedZoneSFX;
    public AudioClip losedLiquidSFX;
    public AudioClip crackSFX;
    public AudioClip movingMachineSFX;

    [Range(0f, 1f)] public float musicVolume = 0.07f;
    private float sfxVolume = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllVolumes();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (mainMenuMusic != null)
        {
            PlayMainMenuMusic();
        }
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic, true);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic, true);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    private void LoadAllVolumes()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.07f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        musicSource.volume = musicVolume; 
    }

    public void PlayCreditsMusic()
    {
        PlayMusic(creditsMusic, true);
    }

    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicClip == null) return;

        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip sfxClip, bool makeItLoop = false)
    {
        if (sfxClip == null) return;

        if (makeItLoop)
        {
            if (SFXSource.clip != sfxClip || !SFXSource.isPlaying)
            {
                SFXSource.clip = sfxClip;
                SFXSource.loop = true;
                SFXSource.volume = sfxVolume;
                SFXSource.Play();
            }
        }
        else
        {
            SFXSource.PlayOneShot(sfxClip, sfxVolume);
        }
    }

    public void StopSFX()
    {
        SFXSource.loop = false;
        SFXSource.Stop();
        SFXSource.clip = null;
    }

    private void OnValidate()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void PauseSFX()
    {
        SFXSource.Pause();
    }

    public void ResumeSFX()
    {
        SFXSource.UnPause();
    }

    public AudioSource Create3DAudioSource(AudioClip clip, Vector3 position, bool loop = true)
    {
        if (clip == null) return null;

        GameObject audioObj = new GameObject("3D_Audio_" + clip.name);
        audioObj.transform.position = position;

        AudioSource newSource = audioObj.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.loop = loop;
        newSource.volume = sfxVolume;

        // 3. ¡EL TRUCO TRIDIMENSIONAL! Cambiamos el Spatial Blend a 1.0f para que sea 3D puro
        newSource.spatialBlend = 1.0f;
        newSource.rolloffMode = AudioRolloffMode.Logarithmic;
        newSource.minDistance = 1.5f;  // Distancia a la que se escucha al máximo volumen
        newSource.maxDistance = 15.0f; // Distancia máxima a la que se deja de escuchar

        audioObj.transform.SetParent(this.transform);

        return newSource;
    }

    public void PlayDialogueVoice(AudioClip voiceClip)
    {
        if (voiceClip == null) return;

        SFXSource.PlayOneShot(voiceClip, sfxVolume);
    }

    public void StopDialogueVoice()
    {
        if (SFXSource != null)
        {
            SFXSource.Stop();
        }
    }
}
using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volumen Global")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    private FMOD.Studio.Bus masterBus;

    [Header("Player Sounds")]
    public EventReference chairEnter;
    public EventReference chairExit;

    [Header("Environment")]
    public EventReference doorOpen;
    public EventReference doorClose;

    [Header("Alarms & Radar")]
    public EventReference purpleAlarm;
    public EventReference redAlarm;
    public EventReference radarEventFail;

    [Header("Machine & Fluid Actions")]
    public EventReference pumpMove;
    public EventReference pumpSwap;
    public EventReference specificRedButton;
    public EventReference cityChangeArrowButton;

    [Header("Frequency Keyboard")]
    public EventReference frequencyNumberButton;

    [Header("Cracks & Damages")]
    public EventReference cityFunnelCrack;
    public EventReference alternativeCrack;

    [Header("Camera & Visuals")]
    public EventReference cameraZoomIn;
    public EventReference cameraZoomOut;

    [Header("Minigame Wave Puzzle")]
    public EventReference waveSuccess;
    public EventReference waveFail;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            masterBus = RuntimeManager.GetBus("bus:/");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        masterBus.setVolume(masterVolume);
    }

    public void PlayOneShot(EventReference sound, Vector3 position)
    {
        if (!sound.IsNull) RuntimeManager.PlayOneShot(sound, position);
    }

    public void PlayOneShotAttached(EventReference sound, GameObject target)
    {
        if (!sound.IsNull) RuntimeManager.PlayOneShotAttached(sound, target);
    }
}
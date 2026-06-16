using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadarMinigame : MonoBehaviour, IResetable
{
    [Header("Radar Move (Optical)")]
    public Transform radarPointZero;
    public Transform radarPoint;
    public float maxDist = 0.25f;
    [SerializeField] private float innerMargin = 0.05f;
    [SerializeField] private float outerMargin = 0.02f;

    [Header("UI & Prefabs Separated")]
    public TMPro.TextMeshProUGUI timeTMP;
    public GameObject spaceShipPrefab;
    public GameObject meteoritePrefab;

    [Header("Settings of return")]
    [SerializeField] private float autoResetSpeed = 0.4f;

    [Header("AFK Config")]
    public float maxIdleTime = 5f;
    private float currentIdleTimer = 0f;

    [Header("AntiSpam")]
    [SerializeField] private float minDistanceBetweenTargets = 0.04f;

    [Header("Alarm System")]
    [SerializeField] private Animation redAlarmAnimation;
    [SerializeField] private AnimationClip redAlarmClip;

    [Space(5)]
    [SerializeField] private Animation blueAlarmAnimation;
    [SerializeField] private AnimationClip blueAlarmClip;

    [Header("StateColors")]
    [ColorUsage(true, true)]
    [SerializeField] private Color redAlarmOffColor = new Color(0f, 0f, 0f, 0f);
    [ColorUsage(true, true)]
    [SerializeField] private Color blueAlarmOffColor = new Color(0f, 0f, 0f, 0f);

    private Material redMaterial;
    private Material blueMaterial;

    [Header("Objectives Spawn")]
    public List<GameObject> objectives = new List<GameObject>();

    public static Action<bool> OnEventFinished;

    private bool inEvent = false;
    private bool isSliderResetting = false;
    private bool isRotationResetting = false;

    private float currentEventTime;
    private int spawnQuantity = 3;
    private RadarEventType currentEventType;
    private string baseTxt = "";
    private bool onCenter;

    private Quaternion initialRadarRotation;

    private FMOD.Studio.EventInstance redAlarmAudioInstance;
    private FMOD.Studio.EventInstance purpleAlarmAudioInstance;

    public bool IsEventActive() => inEvent;
    public bool IsSliderResetting() => isSliderResetting;
    public bool IsRotationResetting() => isRotationResetting;

    void Start()
    {
        if (timeTMP != null) timeTMP.text = baseTxt;
        onCenter = true;
        initialRadarRotation = radarPoint.localRotation;

        CacheOriginalMaterials();

        if (GameManager.Instance != null) GameManager.Instance.RegisterResetable(this);
    }

    private void OnEnable()
    {
        SliderCont.OnSliderMoved += SliderMoved;
        WheelRotator.OnRotatedMoved += SliderMoved;
        RadarVision.OnObjectiveGet += ObjectiveTaken;
    }

    private void OnDisable()
    {
        SliderCont.OnSliderMoved -= SliderMoved;
        WheelRotator.OnRotatedMoved -= SliderMoved;
        RadarVision.OnObjectiveGet -= ObjectiveTaken;

        StopAlarmHardwareFeedback();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.UnregisterResetable(this);

        CleanAudioInstancesImmediate();
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Z)) StartRadarEvent(30f, RadarEventType.Meteorite, 3);

        if (inEvent)
        {
            EventTimer();
            HandleIdleCheck();
        }
    }

    public void StartRadarEvent(float duration, RadarEventType eventType, int quantity)
    {
        if (inEvent || isSliderResetting || isRotationResetting) return;

        inEvent = true;
        currentEventType = eventType;
        spawnQuantity = quantity;
        currentEventTime = duration;
        currentIdleTimer = 0f;

        ClearAllActiveObjectivesImmediate();
        PlayAlarmHardwareFeedback(eventType);

        for (int i = 0; i < spawnQuantity; i++)
        {
            SpawnRandomObjective();
        }
    }

    private void EventTimer()
    {
        if (objectives.Count <= 0 || isSliderResetting || isRotationResetting) return;

        currentEventTime -= Time.deltaTime;
        if (currentEventTime <= 0f)
        {
            currentEventTime = 0f;
            if (AudioManager.Instance != null)
            {
                FMODUnity.RuntimeManager.PlayOneShot(AudioManager.Instance.radarEventFail, transform.position);
            }
            StartSequenceEnd(false);
        }
        else
        {
            UpdateTimerText(currentEventTime);
        }
    }

    private void HandleIdleCheck()
    {
        if (isSliderResetting || isRotationResetting || SliderCont.IsAtMinPos)
        {
            currentIdleTimer = 0f;
            return;
        }

        currentIdleTimer += Time.deltaTime;

        if (currentIdleTimer >= maxIdleTime)
        {
            currentIdleTimer = 0f;

            StopAllCoroutines();
            StartCoroutine(FinalHardwareResetSequenceRoutine(null));
        }
    }

    void ObjectiveTaken(GameObject _objective)
    {
        if (_objective == null) return;

        objectives.Remove(_objective);
        Destroy(_objective);

        currentIdleTimer = 0f;

        if (objectives.Count <= 0)
        {
            StartSequenceEnd(true);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FinalHardwareResetSequenceRoutine(null));
        }
    }

    private void StartSequenceEnd(bool success)
    {
        inEvent = false;
        if (timeTMP != null) timeTMP.text = "";

        StopAlarmHardwareFeedback();
        StopAllCoroutines();
        StartCoroutine(FinalHardwareResetSequenceRoutine(success));
    }

    private void CacheOriginalMaterials()
    {
        if (redAlarmAnimation != null)
        {
            Renderer r = redAlarmAnimation.GetComponentInChildren<Renderer>();
            if (r == null) r = redAlarmAnimation.GetComponent<Renderer>();
            if (r != null) redMaterial = r.material;
        }

        if (blueAlarmAnimation != null)
        {
            Renderer r = blueAlarmAnimation.GetComponentInChildren<Renderer>();
            if (r == null) r = blueAlarmAnimation.GetComponent<Renderer>();
            if (r != null) blueMaterial = r.material;
        }

        StopAlarmHardwareFeedback();
    }

    private void PlayAlarmHardwareFeedback(RadarEventType type)
    {
        if (redMaterial != null) redMaterial.DisableKeyword("_EMISSION");
        if (blueMaterial != null) blueMaterial.DisableKeyword("_EMISSION");

        CleanAudioInstancesImmediate();

        if (type == RadarEventType.Meteorite)
        {
            if (redMaterial != null) redMaterial.EnableKeyword("_EMISSION");

            if (redAlarmAnimation != null && redAlarmClip != null)
            {
                redAlarmAnimation.AddClip(redAlarmClip, redAlarmClip.name);
                redAlarmAnimation.Play(redAlarmClip.name);
            }

            if (AudioManager.Instance != null)
            {
                redAlarmAudioInstance = FMODUnity.RuntimeManager.CreateInstance(AudioManager.Instance.redAlarm);
                redAlarmAudioInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
                redAlarmAudioInstance.start();
            }
        }
        else if (type == RadarEventType.SpaceShip)
        {
            if (blueMaterial != null) blueMaterial.EnableKeyword("_EMISSION");

            if (blueAlarmAnimation != null && blueAlarmClip != null)
            {
                blueAlarmAnimation.AddClip(blueAlarmClip, blueAlarmClip.name);
                blueAlarmAnimation.Play(blueAlarmClip.name);
            }

            if (AudioManager.Instance != null)
            {
                purpleAlarmAudioInstance = FMODUnity.RuntimeManager.CreateInstance(AudioManager.Instance.purpleAlarm);
                purpleAlarmAudioInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
                purpleAlarmAudioInstance.start();
            }
        }
    }

    private void StopAlarmHardwareFeedback()
    {
        if (redAlarmAnimation != null) redAlarmAnimation.Stop();
        if (blueAlarmAnimation != null) blueAlarmAnimation.Stop();

        CleanAudioInstancesImmediate();

        if (redMaterial != null)
        {
            if (redMaterial.HasProperty("_EmissionColor")) redMaterial.SetColor("_EmissionColor", redAlarmOffColor);
            if (redMaterial.HasProperty("_BaseColor")) redMaterial.SetColor("_BaseColor", redAlarmOffColor);
            redMaterial.DisableKeyword("_EMISSION");
            DynamicGI.SetEmissive(redAlarmAnimation.GetComponent<Renderer>() ?? redAlarmAnimation.GetComponentInChildren<Renderer>(), Color.black);
        }

        if (blueMaterial != null)
        {
            if (blueMaterial.HasProperty("_EmissionColor")) blueMaterial.SetColor("_EmissionColor", blueAlarmOffColor);
            if (blueMaterial.HasProperty("_BaseColor")) blueMaterial.SetColor("_BaseColor", blueAlarmOffColor);
            blueMaterial.DisableKeyword("_EMISSION");
            DynamicGI.SetEmissive(blueAlarmAnimation.GetComponent<Renderer>() ?? blueAlarmAnimation.GetComponentInChildren<Renderer>(), Color.black);
        }
    }

    private void CleanAudioInstancesImmediate()
    {
        if (redAlarmAudioInstance.isValid())
        {
            redAlarmAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            redAlarmAudioInstance.release();
        }

        if (purpleAlarmAudioInstance.isValid())
        {
            purpleAlarmAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            purpleAlarmAudioInstance.release();
        }
    }

    private IEnumerator FinalHardwareResetSequenceRoutine(bool? success)
    {
        isSliderResetting = true;
        isRotationResetting = true;

        if (success.HasValue)
        {
            ClearAllActiveObjectivesImmediate();
        }

        SliderCont sceneSlider = UnityEngine.Object.FindAnyObjectByType<SliderCont>();
        SliderCont.ForceMinPosState(false);

        while (Vector3.Distance(radarPoint.position, radarPointZero.position) > 0.001f)
        {
            radarPoint.position = Vector3.MoveTowards(radarPoint.position, radarPointZero.position, autoResetSpeed * Time.deltaTime);
            if (sceneSlider != null) sceneSlider.ApplyInstantPosition(0f);
            yield return null;
        }

        radarPoint.position = radarPointZero.position;
        onCenter = true;

        if (sceneSlider != null) sceneSlider.ResetValues();

        SliderCont.ForceMinPosState(true);
        isSliderResetting = false;

        yield return new WaitForEndOfFrame();

        WheelRotator rotationWheel = null;
        WheelRotator[] wheels = UnityEngine.Object.FindObjectsByType<WheelRotator>(FindObjectsSortMode.None);
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].type == SliderType.Rotation) rotationWheel = wheels[i];
        }

        float fastResetSpeed = autoResetSpeed * 8f;

        while (Quaternion.Angle(radarPoint.localRotation, initialRadarRotation) > 0.01f)
        {
            radarPoint.localRotation = Quaternion.Slerp(radarPoint.localRotation, initialRadarRotation, fastResetSpeed * Time.deltaTime);
            if (rotationWheel != null) rotationWheel.ForceRotationReset(radarPoint.localRotation.eulerAngles.x);
            yield return null;
        }

        radarPoint.localRotation = initialRadarRotation;
        if (rotationWheel != null) rotationWheel.ForceRotationReset(initialRadarRotation.eulerAngles.x);

        isRotationResetting = false;
        currentIdleTimer = 0f;

        if (success.HasValue)
        {
            bool finalResult = success.Value;

            if (!finalResult && currentEventType == RadarEventType.SpaceShip)
            {
            }
            else if (finalResult && currentEventType == RadarEventType.SpaceShip)
            {
                AttackButton attackButton = UnityEngine.Object.FindAnyObjectByType<AttackButton>();
                if (attackButton != null && attackButton.RawFunnel != null) attackButton.RawFunnel.AddLiquid(20f);
            }

            inEvent = false;
            OnEventFinished?.Invoke(finalResult);
        }
    }

    void UpdateTimerText(float currentTime)
    {
        if (timeTMP == null) return;
        int seconds = Mathf.CeilToInt(currentTime);
        timeTMP.text = $"{baseTxt}{seconds}";
    }

    void SliderMoved(float value, SliderType type)
    {
        if (!inEvent) return;

        if (type == SliderType.Position && isSliderResetting) return;
        if (type == SliderType.Rotation && isRotationResetting) return;

        currentIdleTimer = 0f;

        switch (type)
        {
            case SliderType.Position:
                Vector3 offset = radarPoint.forward * (maxDist * value);
                radarPoint.position = radarPointZero.position + offset;
                onCenter = Mathf.Abs(value) < 0.1f;
                break;

            case SliderType.Rotation:
                if (!SliderCont.IsAtMinPos) return;
                if (!onCenter) return;

                float angleDelta = value;
                radarPoint.Rotate(Vector3.up, angleDelta, Space.Self);
                break;
        }
    }

    void SpawnRandomObjective()
    {
        GameObject prefabToSpawn = (currentEventType == RadarEventType.Meteorite) ? meteoritePrefab : spaceShipPrefab;

        if (prefabToSpawn == null) return;

        float minRadius = innerMargin;
        float maxRadius = maxDist - outerMargin;
        Vector3 finalPosition = Vector3.zero;
        bool positionValid = false;

        for (int attempt = 0; attempt < 30 && !positionValid; attempt++)
        {
            float randomRadius = UnityEngine.Random.Range(minRadius, maxRadius);
            float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float x = Mathf.Cos(randomAngle);
            float y = Mathf.Sin(randomAngle);

            Vector3 direction = radarPointZero.right * x + radarPointZero.forward * y;
            finalPosition = radarPointZero.position + direction.normalized * randomRadius;

            positionValid = true;
            foreach (GameObject existingObj in objectives)
            {
                if (existingObj != null && Vector3.Distance(finalPosition, existingObj.transform.position) < minDistanceBetweenTargets)
                {
                    positionValid = false;
                    break;
                }
            }
        }

        GameObject obj = Instantiate(prefabToSpawn, finalPosition, Quaternion.identity);
        objectives.Add(obj);
    }

    private void ClearAllActiveObjectivesImmediate()
    {
        foreach (var obj in objectives) { if (obj != null) Destroy(obj); }
        objectives.Clear();
    }

    public void ResetValues()
    {
        StopAllCoroutines();
        ClearAllActiveObjectivesImmediate();
        StopAlarmHardwareFeedback();

        inEvent = false;
        isSliderResetting = false;
        isRotationResetting = false;
        currentEventTime = 0f;
        currentIdleTimer = 0f;

        if (timeTMP != null) timeTMP.text = "";

        radarPoint.position = radarPointZero.position;
        radarPoint.localRotation = initialRadarRotation;

        SliderCont sceneSlider = UnityEngine.Object.FindAnyObjectByType<SliderCont>();
        if (sceneSlider != null) sceneSlider.ResetValues();
        SliderCont.ForceMinPosState(true);
    }
}
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadarMinigame : MonoBehaviour, IResetable
{
    [Header("Radar Move (Pieza Óptica)")]
    public Transform radarPointZero;
    public Transform radarPoint;
    public float maxDist = 0.25f;
    [SerializeField] private float innerMargin = 0.05f;
    [SerializeField] private float outerMargin = 0.02f;

    [Header("UI & Prefabs")]
    public TMPro.TextMeshProUGUI timeTMP;
    public GameObject objectivePrefab;

    [Header("Ajustes de Retorno Mecánico (Polish)")]
    [SerializeField] private float autoResetSpeed = 0.4f;

    [Header("Configuración de Inactividad (AFK)")]
    public float maxIdleTime = 5f;
    private float currentIdleTimer = 0f;

    [Header("Seguridad de Spawn (Anti-solapamiento)")]
    [SerializeField] private float minDistanceBetweenTargets = 0.04f;

    [Header("SISTEMA DE ALARMAS SEPARADO (Visual)")]
    [Tooltip("Componente Animation físico de la Alarma Roja.")]
    [SerializeField] private Animation redAlarmAnimation;
    [Tooltip("Clip .anim de la alarma Roja.")]
    [SerializeField] private AnimationClip redAlarmClip;

    [Space(10)]
    [Tooltip("Componente Animation físico de la Alarma Azul.")]
    [SerializeField] private Animation blueAlarmAnimation;
    [Tooltip("Clip .anim de la alarma Azul.")]
    [SerializeField] private AnimationClip blueAlarmClip;

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

    public bool IsEventActive() => inEvent;
    public bool IsSliderResetting() => isSliderResetting;
    public bool IsRotationResetting() => isRotationResetting;

    void Start()
    {
        if (timeTMP != null) timeTMP.text = baseTxt;
        onCenter = true;
        initialRadarRotation = radarPoint.localRotation;

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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) StartRadarEvent(30f, RadarEventType.SpaceShip, 3);

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

        // Encendemos la alarma correspondiente al tipo de peligro
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
            StartSequenceEnd(false);
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
            inEvent = false;
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

        // Apagamos los componentes de animación de inmediato
        StopAlarmHardwareFeedback();

        StopAllCoroutines();
        StartCoroutine(FinalHardwareResetSequenceRoutine(success));
    }

    /// <summary>
    /// Activa el componente de animación y el clip concreto de la alarma correspondiente.
    /// </summary>
    private void PlayAlarmHardwareFeedback(RadarEventType type)
    {
        if (type == RadarEventType.Meteorite)
        {
            // Dispara la alarma Roja de forma independiente
            if (redAlarmAnimation != null && redAlarmClip != null)
            {
                redAlarmAnimation.Play(redAlarmClip.name);
            }
        }
        else if (type == RadarEventType.SpaceShip)
        {
            // Dispara la alarma Azul de forma independiente
            if (blueAlarmAnimation != null && blueAlarmClip != null)
            {
                blueAlarmAnimation.Play(blueAlarmClip.name);
            }
        }
    }

    /// <summary>
    /// Detiene ambos componentes físicos de animación para apagar las luces de la cabina.
    /// </summary>
    private void StopAlarmHardwareFeedback()
    {
        if (redAlarmAnimation != null)
        {
            redAlarmAnimation.Stop();
        }

        if (blueAlarmAnimation != null)
        {
            blueAlarmAnimation.Stop();
        }
    }

    private IEnumerator FinalHardwareResetSequenceRoutine(bool? success)
    {
        isSliderResetting = true;
        isRotationResetting = true;

        if (success.HasValue)
        {
            if (timeTMP != null) timeTMP.text = "";
            ClearAllActiveObjectivesImmediate();
        }

        // -------------------------------------------------------------
        // FASE 1: Retorno lento y pesado de la palanca de posición
        // -------------------------------------------------------------
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

        // -------------------------------------------------------------
        // FASE 2: Retorno ULTRA RÁPIDO del Volante de Rotación
        // -------------------------------------------------------------
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

        // -------------------------------------------------------------
        // FASE 3: Fin del evento e Historia Lineal
        // -------------------------------------------------------------
        if (success.HasValue)
        {
            bool finalResult = success.Value;

            if (!finalResult && currentEventType == RadarEventType.SpaceShip)
            {
                // LLAMAR_AQUÍ_A_LA_FUNCIÓN_DE_ATAQUE_ENEMIGO_POR_FALLO_DE_TIEMPO
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

        GameObject obj = Instantiate(objectivePrefab, finalPosition, Quaternion.identity);
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
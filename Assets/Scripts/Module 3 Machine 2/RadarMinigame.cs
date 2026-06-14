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
    [Tooltip("Velocidad de retorno de las palancas y volantes. Ahora es muy lento y pesado.")]
    [SerializeField] private float autoResetSpeed = 1.0f;

    [Header("Configuración de Inactividad (AFK)")]
    [Tooltip("Tiempo en segundos que puede estar el slider sin tocarse antes de regresar solo al inicio.")]
    public float maxIdleTime = 5f;
    private float currentIdleTimer = 0f;
    private bool isReturningFromIdle = false;

    [Header("Seguridad de Spawn (Anti-solapamiento)")]
    [SerializeField] private float minDistanceBetweenTargets = 0.04f;

    [Header("Objectives Spawn")]
    public List<GameObject> objectives = new List<GameObject>();

    public static Action<bool> OnEventFinished;

    private bool inEvent = false;
    private bool isResettingHardware = false; // Candado para bloquear interacciones en el fin del juego
    private float currentEventTime;
    private int spawnQuantity = 3;
    private RadarEventType currentEventType;
    private string baseTxt = "";
    private bool onCenter;

    private Quaternion initialRadarRotation;

    public bool IsEventActive() => inEvent;

    // Si el hardware está volviendo por AFK o por fin de juego, se considera bloqueado
    public bool IsResettingHardware() => isResettingHardware || isReturningFromIdle;

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
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.UnregisterResetable(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) StartRadarEvent(30f, RadarEventType.SpaceShip, 3);

        // El temporizador general del evento corre de fondo de forma independiente
        if (inEvent)
        {
            EventTimer();
            HandleIdleCheck();
        }
    }

    public void StartRadarEvent(float duration, RadarEventType eventType, int quantity)
    {
        if (inEvent || isResettingHardware || isReturningFromIdle) return;

        inEvent = true;
        currentEventType = eventType;
        spawnQuantity = quantity;
        currentEventTime = duration;
        currentIdleTimer = 0f;

        ClearAllActiveObjectivesImmediate();

        for (int i = 0; i < spawnQuantity; i++)
        {
            SpawnRandomObjective();
        }
    }

    private void EventTimer()
    {
        // Si el hardware se está moviendo (por fin de juego o AFK), el tiempo se congela o termina controlado
        if (isResettingHardware) return;

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
        // Si ya está regresando por inactividad o fin de juego, o si está abajo del todo, no sumamos tiempo
        if (isResettingHardware || isReturningFromIdle || SliderCont.IsAtMinPos)
        {
            currentIdleTimer = 0f;
            return;
        }

        currentIdleTimer += Time.deltaTime;

        if (currentIdleTimer >= maxIdleTime)
        {
            currentIdleTimer = 0f;
            StartCoroutine(IdleHardwareOnlyResetRoutine());
        }
    }

    /// <summary>
    /// REGLA AFK: Solo recoloca los mandos físicos de la mesa (Palanca y luego Volante) de forma lenta.
    /// NO borra naves de la pantalla, NO quita el temporizador, NO acaba el evento.
    /// </summary>
    private IEnumerator IdleHardwareOnlyResetRoutine()
    {
        isReturningFromIdle = true;
        SliderCont sceneSlider = UnityEngine.Object.FindAnyObjectByType<SliderCont>();

        // 1. LERP DEL SLIDER: Regresa lentamente a la posición inicial
        while (Vector3.Distance(radarPoint.position, radarPointZero.position) > 0.001f)
        {
            radarPoint.position = Vector3.MoveTowards(radarPoint.position, radarPointZero.position, autoResetSpeed * Time.deltaTime);
            if (sceneSlider != null) sceneSlider.ApplyInstantPosition(0f);
            yield return null;
        }
        radarPoint.position = radarPointZero.position;
        onCenter = true;
        if (sceneSlider != null) sceneSlider.ResetValues();

        yield return null; // Frame de seguridad para asentar IsAtMinPos

        // 2. LERP DEL VOLANTE: Gira lentamente hasta recuperar la rotación inicial
        WheelRotator rotationWheel = null;
        WheelRotator[] wheels = UnityEngine.Object.FindObjectsByType<WheelRotator>(FindObjectsSortMode.None);
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].type == SliderType.Rotation) rotationWheel = wheels[i];
        }

        while (Quaternion.Angle(radarPoint.localRotation, initialRadarRotation) > 0.01f)
        {
            radarPoint.localRotation = Quaternion.Slerp(radarPoint.localRotation, initialRadarRotation, autoResetSpeed * Time.deltaTime);
            if (rotationWheel != null) rotationWheel.ForceRotationReset(radarPoint.localRotation.eulerAngles.x);
            yield return null;
        }
        radarPoint.localRotation = initialRadarRotation;

        isReturningFromIdle = false;
        currentIdleTimer = 0f;
    }

    /// <summary>
    /// REGLA DE BAJA ÚNICA: Elimina la nave destruida y activa el rebobinado lento de mandos.
    /// </summary>
    void ObjectiveTaken(GameObject _objective)
    {
        if (_objective == null) return;

        objectives.Remove(_objective);
        Destroy(_objective); // Se muere SOLO el objetivo clicado

        // Reiniciamos el temporizador de inactividad de la mesa
        currentIdleTimer = 0f;

        // Si ya no quedan naves en toda la pantalla, cerramos el evento entero (Éxito)
        if (objectives.Count <= 0)
        {
            StartSequenceEnd(true);
        }
        else
        {
            // ¡NUEVO!: Si aún quedan naves vivas, congelamos temporalmente la entrada del jugador 
            // y activamos el mismo Lerp secuencial lento para limpiar la posición física de la mesa.
            StopAllCoroutines();
            StartCoroutine(FinalHardwareResetSequenceRoutine(null));
        }
    }

    private void StartSequenceEnd(bool success)
    {
        // DESACTIVAR TEMPORIZADOR: El reloj desaparece de la pantalla de la máquina fulminantemente
        if (timeTMP != null) timeTMP.text = "";

        // Cortamos cualquier corrutina de AFK residual que estuviera corriendo para que no choque
        StopAllCoroutines();

        StartCoroutine(FinalHardwareResetSequenceRoutine(success));
    }

    /// <summary>
    /// SECUENCIA DE FIN DE EVENTO O RESET INTERMEDIO: Elimina naves si aplica y rebobina los mandos lentamente.
    /// </summary>
    private IEnumerator FinalHardwareResetSequenceRoutine(bool? success) // <--- ¡ASEGÚRATE DE QUE TIENE EL bool? CON EL SIGNO DE INTERROGACIÓN!
    {
        isResettingHardware = true;

        // Si es el fin del evento real (tiene valor true o false), limpiamos los targets restantes inmediatamente
        if (success.HasValue)
        {
            ClearAllActiveObjectivesImmediate();
        }

        // -------------------------------------------------------------
        // FASE 1: El Slider de Posición hace su Lerp de vuelta LENTO hacia el inicio (MinPos)
        // -------------------------------------------------------------
        SliderCont sceneSlider = UnityEngine.Object.FindAnyObjectByType<SliderCont>();
        while (Vector3.Distance(radarPoint.position, radarPointZero.position) > 0.001f)
        {
            radarPoint.position = Vector3.MoveTowards(radarPoint.position, radarPointZero.position, autoResetSpeed * Time.deltaTime);
            if (sceneSlider != null) sceneSlider.ApplyInstantPosition(0f);
            yield return null;
        }
        radarPoint.position = radarPointZero.position;
        onCenter = true;
        if (sceneSlider != null) sceneSlider.ResetValues();

        yield return null; // Frame de seguridad para asentar físicas

        // -------------------------------------------------------------
        // FASE 2: Cuando el slider ya está en MinPos, el volante de Rotación hace su Lerp lento al origen
        // -------------------------------------------------------------
        WheelRotator rotationWheel = null;
        WheelRotator[] wheels = UnityEngine.Object.FindObjectsByType<WheelRotator>(FindObjectsSortMode.None);
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].type == SliderType.Rotation) rotationWheel = wheels[i];
        }

        while (Quaternion.Angle(radarPoint.localRotation, initialRadarRotation) > 0.01f)
        {
            radarPoint.localRotation = Quaternion.Slerp(radarPoint.localRotation, initialRadarRotation, autoResetSpeed * Time.deltaTime);
            if (rotationWheel != null) rotationWheel.ForceRotationReset(radarPoint.localRotation.eulerAngles.x);
            yield return null;
        }
        radarPoint.localRotation = initialRadarRotation;

        isResettingHardware = false;
        currentIdleTimer = 0f;

        // -------------------------------------------------------------
        // FASE 3: Solo si es el fin real del evento (Éxito/Fracaso), procesamos resultados de la historia
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
        // Bloqueamos la entrada del jugador si los mandos están regresando automáticamente por AFK o fin de juego
        if (isResettingHardware || isReturningFromIdle || !inEvent) return;

        // Mover los mandos rompe el estado idle y pone la cuenta atrás a cero
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

        inEvent = false;
        isResettingHardware = false;
        isReturningFromIdle = false;
        currentEventTime = 0f;
        currentIdleTimer = 0f;

        if (timeTMP != null) timeTMP.text = "";

        radarPoint.position = radarPointZero.position;
        radarPoint.localRotation = initialRadarRotation;

        SliderCont sceneSlider = UnityEngine.Object.FindAnyObjectByType<SliderCont>();
        if (sceneSlider != null) sceneSlider.ResetValues();
    }
}
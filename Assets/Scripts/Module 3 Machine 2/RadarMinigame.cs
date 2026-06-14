using System;
using UnityEngine;
using System.Collections.Generic;

public enum RadarEventType { Meteorite, SpaceShip }

public class RadarMinigame : MonoBehaviour
{
    [Header("Radar Move")]
    public Transform radarPointZero;
    public Transform radarPoint;
    public float maxDist = 0.25f;
    [SerializeField] private float innerMargin = 0.05f;
    [SerializeField] private float outerMargin = 0.02f;

    [Header("UI & Prefabs")]
    public TMPro.TextMeshProUGUI timeTMP;
    public GameObject objectivePrefab;

    [Header("Event State")]
    private bool inEvent = false;
    private float currentEventTime;
    private int spawnQuantity = 3;
    private RadarEventType currentEventType;

    [Header("Objectives Spawn")]
    public List<GameObject> objectives = new List<GameObject>();

    public static Action<bool> OnEventFinished;

    private string baseTxt = "";
    private bool onCenter;

    public bool IsEventActive() => inEvent;

    void Start()
    {
        timeTMP.text = baseTxt;
        onCenter = true;
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

    void Update()
    {
        // Disparador manual por teclado solicitado por motivos de testeo
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // Ejemplo rápido: 30 segundos, tipo Nave
            StartRadarEvent(30f, RadarEventType.SpaceShip, 3);
        }

        if (inEvent)
        {
            EventTimer();
        }
    }

    /// <summary>
    /// FUNCIÓN PÚBLICA LINEAL: Arranca de inmediato el evento sin delays.
    /// Solo se permite si no hay otra alarma o evento corriendo simultáneamente en la mesa.
    /// </summary>
    public void StartRadarEvent(float duration, RadarEventType eventType, int quantity)
    {
        if (inEvent) return; // Candado: evita solapar dos eventos a la vez

        inEvent = true;
        currentEventType = eventType;
        spawnQuantity = quantity;
        currentEventTime = duration;

        ResetObjectivesList();

        for (int i = 0; i < spawnQuantity; i++)
        {
            SpawnRandomObjective();
        }
    }

    private void EventTimer()
    {
        currentEventTime -= Time.deltaTime;

        if (currentEventTime <= 0f)
        {
            currentEventTime = 0f;
            ResolveEventFailure();
        }

        UpdateTimerText(currentEventTime);
    }

    private void ResolveEventFailure()
    {
        ClearAllActiveObjectives();
        inEvent = false;

        if (currentEventType == RadarEventType.SpaceShip)
        {
            // LLAMAR_AQUÍ_A_LA_FUNCIÓN_DE_ATAQUE_ENEMIGO_POR_FALLO_DE_TIEMPO
        }

        OnEventFinished?.Invoke(false); // <--- IDENTIFICADOR DESDE OTRO SCRIPT: FALSO = FRACASO
    }

    void ObjectiveTaken(GameObject _objective)
    {
        objectives.Remove(_objective);
        Destroy(_objective);

        if (objectives.Count <= 0)
        {
            ResolveEventSuccess();
        }
    }

    private void ResolveEventSuccess()
    {
        inEvent = false;

        if (currentEventType == RadarEventType.SpaceShip)
        {
            // Si eran naves superadas con éxito, inyectamos el líquido en el depósito
            AttackButton attackButton = UnityEngine.Object.FindAnyObjectByType<AttackButton>();
            if (attackButton != null && attackButton.RawFunnel != null)
            {
                attackButton.RawFunnel.AddLiquid(20f);
            }
        }

        OnEventFinished?.Invoke(true); // <--- IDENTIFICADOR DESDE OTRO SCRIPT: TRUE = ÉXITO
    }

    void UpdateTimerText(float currentTime)
    {
        int seconds = Mathf.CeilToInt(currentTime);
        timeTMP.text = $"{baseTxt}{seconds}";
    }

    void SliderMoved(float value, SliderType type)
    {
        if (!inEvent) return;

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

        float randomRadius = UnityEngine.Random.Range(minRadius, maxRadius);
        float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        float x = Mathf.Cos(randomAngle);
        float y = Mathf.Sin(randomAngle);

        Vector3 direction = radarPointZero.right * x + radarPointZero.forward * y;
        Vector3 finalPosition = radarPointZero.position + direction.normalized * randomRadius;

        GameObject obj = Instantiate(objectivePrefab, finalPosition, Quaternion.identity);
        objectives.Add(obj);
    }

    private void ClearAllActiveObjectives()
    {
        foreach (var obj in objectives)
        {
            if (obj != null) Destroy(obj);
        }
        objectives.Clear();
    }

    private void ResetObjectivesList()
    {
        ClearAllActiveObjectives();
    }
}
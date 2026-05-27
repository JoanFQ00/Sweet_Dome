using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SliderType { Rotation, Position }

public class RadarMinigame : MonoBehaviour
{
    [Header("Radar Move")]
    public Transform radarPointZero;
    public Transform radarPoint;
    public float maxDist = 0.25f;
    public SliderType type;
    [SerializeField] private float innerMargin = 0.05f;
    [SerializeField] private float outerMargin = 0.02f;

    public TMPro.TextMeshProUGUI timeTMP;
    public GameObject objectivePrefab;

    [Header("Event")]
    private bool inEvent;
    private float timeToEvent;

    public float eventTime; // // tiempo total del evento (ahora usado como countdown)
    private float currentEventTime; // // AHORA es COUNTDOWN en vez de countup

    private int spawnQuantity;

    private bool delay_On;
    private float eventStart_Delay;

    public static Action<bool> OnEventFinished;

    [Header("Objectives Spawn")]
    public GameObject[] objectives;
    private GameObject currentObjective;

    private string baseTxt = "00";
    private bool onCenter;
    private Quaternion startLocalRot;

    public GameObject CurrentObjective => currentObjective;

    void Start()
    {
        currentObjective = objectives[Random.Range(0, objectives.Length)];
        currentObjective.SetActive(false);

        timeTMP.text = baseTxt;
        startLocalRot = radarPoint.localRotation;
        onCenter = true;
    }

    private void OnEnable()
    {
        SliderCont.OnSliderMoved += SliderMoved;
        WheelRotator.OnRotatedMoved += SliderMoved; 
        RadarVision.OnObjectiveGet += ChangeCodeTxt;

        //Falta evento start event
    }

    private void OnDisable()
    {
        SliderCont.OnSliderMoved -= SliderMoved;
        WheelRotator.OnRotatedMoved -= SliderMoved; 
        RadarVision.OnObjectiveGet -= ChangeCodeTxt;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) 
        {
            EventLoader(5, 15, 1);
        }

        if (delay_On)
        {
            DelayTime();
        }

        if (inEvent)
        {
            EventTimer();
        }
    }

    public void EventLoader(float _timeToEvent, float _eventTime, int _quantity)
    {
        timeToEvent = _timeToEvent;
        eventTime = _eventTime;
        spawnQuantity = _quantity;

        eventStart_Delay = Time.time + timeToEvent;
        delay_On = true;

        currentEventTime = eventTime; // FIX: inicializamos el COUNTDOWN aquí
    }

    private void DelayTime()
    {
        if (Time.time >= eventStart_Delay)
        {
            inEvent = true;
            delay_On = false;
        }
    }

    private void EventTimer()
    {
        currentEventTime -= Time.deltaTime; // // FIX: ahora es COUNTDOWN

        if (currentEventTime <= 0f)
        {
            currentEventTime = 0f;

            OnEventFinished?.Invoke(false);
            inEvent = false;
        }

        UpdateTimerText(currentEventTime); // // FIX: actualiza UI cada frame
    }

    void ChangeCodeTxt(string code)
    {
        timeTMP.text = baseTxt + code;
    }

    void UpdateTimerText(float currentTime)
    {
        int seconds = Mathf.CeilToInt(currentTime); 

        timeTMP.text = $"{baseTxt}{seconds:00}";
    }

    void SliderMoved(float value, SliderType type)
    {
        switch (type)
        {
            case SliderType.Position:
                Vector3 offset = radarPoint.forward * (maxDist * value);
                radarPoint.position = radarPointZero.position + offset;

                onCenter = Mathf.Abs(value) < 0.015f; 

                return;

            case SliderType.Rotation:
                if (!onCenter) return;

                float angle = (value - 0.5f) * 360f;
                radarPoint.localRotation = startLocalRot * Quaternion.Euler(0f, angle, 0f);
                return;
        }
    }

    void SpawnRandomObjective()
    {
        currentObjective.SetActive(true);

        float minRadius = innerMargin;
        float maxRadius = maxDist - outerMargin;

        float randomRadius = Random.Range(minRadius, maxRadius);
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);

        float x = Mathf.Cos(randomAngle);
        float y = Mathf.Sin(randomAngle);

        Vector3 direction =
            radarPointZero.right * x +
            radarPointZero.forward * y;

        Vector3 finalPosition =
            radarPointZero.position +
            direction.normalized * randomRadius;

        currentObjective.transform.position = finalPosition;
    }
}
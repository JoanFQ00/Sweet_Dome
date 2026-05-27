using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;

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

    public float eventTime; 
    private float currentEventTime; 

    private int spawnQuantity;

    private bool delay_On;
    private float eventStart_Delay;

    public static Action<bool> OnEventFinished;

    [Header("Objectives Spawn")]
    public List<GameObject> objectives;
    private bool objectivesSpawned = false;

    private string baseTxt = "00";
    private bool onCenter;
    private Quaternion startLocalRot;

    void Start()
    {
        timeTMP.text = baseTxt;
        startLocalRot = radarPoint.localRotation;
        onCenter = true;
    }

    private void OnEnable()
    {
        SliderCont.OnSliderMoved += SliderMoved;
        WheelRotator.OnRotatedMoved += SliderMoved; 
        RadarVision.OnObjectiveGet += ObjectiveTaken;
        MachineManager.RadarDispatch += EventLoader;

        //Falta evento start event
    }

    private void OnDisable()
    {
        SliderCont.OnSliderMoved -= SliderMoved;
        WheelRotator.OnRotatedMoved -= SliderMoved; 
        RadarVision.OnObjectiveGet -= ObjectiveTaken;
        MachineManager.RadarDispatch -= EventLoader;
    }

    void Update()
    {
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

        currentEventTime = eventTime;

        objectivesSpawned = false;
    }

    private void DelayTime()
    {
        if (Time.time >= eventStart_Delay)
        {
            inEvent = true;
            delay_On = false;

            if (!objectivesSpawned)
            {
                objectivesSpawned = true;

                for (int i = 0; i < spawnQuantity; i++)
                {
                    SpawnRandomObjective(); 
                }
            }
        }
    }

    private void EventTimer()
    {
        currentEventTime -= Time.deltaTime;

        if (currentEventTime <= 0f)
        {
            currentEventTime = 0f;

            foreach (var objective in objectives)
            {
                if (objective != null)
                    Destroy(objective);
            }

            ResetEvent();

            OnEventFinished?.Invoke(false);
        }

        UpdateTimerText(currentEventTime);
    }

    void ObjectiveTaken(GameObject _objective)
    {
        objectives.Remove(_objective);

        Destroy(_objective);

        if (objectives.Count <= 0)
        {
            Debug.Log("All objectives Taken");

            ResetEvent();

            OnEventFinished?.Invoke(true);
        }
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

        GameObject obj = Instantiate(objectivePrefab, finalPosition, Quaternion.identity);
        objectives.Add(obj);

        obj.transform.position = finalPosition;
    }

    void ResetEvent()
    {
        inEvent = false;
        delay_On = false;

        currentEventTime = 0f;
        timeToEvent = 0f;
        eventStart_Delay = 0f;

        objectivesSpawned = false;

        objectives.Clear();
    }
}
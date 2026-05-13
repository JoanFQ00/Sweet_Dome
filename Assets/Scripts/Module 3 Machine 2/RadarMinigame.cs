using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum SliderType { Rotation, Position }

public class RadarMinigame : MonoBehaviour
{
    public Transform radarPointZero;
    public Transform radarPoint;
    public float maxDist = 0.25f;
    public TMPro.TextMeshProUGUI codeText;
    public SliderType type;
    public GameObject objectivePrefab;

    [SerializeField] private float innerMargin = 0.05f;
    [SerializeField] private float outerMargin = 0.02f;

    private string baseTxt = "CODE: ";
    private bool onCenter;
    private Quaternion startLocalRot;

    public GameObject[] objectives;
    private GameObject currentObjective;

    public GameObject CurrentObjective => currentObjective;

    void Start()
    {
        currentObjective = objectives[Random.Range(0, objectives.Length)];
        currentObjective.SetActive(false);

        codeText.text = baseTxt;
        startLocalRot = radarPoint.localRotation;
        onCenter = true;
    }

    private void OnEnable()
    {
        SliderCont.OnSliderMoved += SliderMoved;
        RadarVision.OnObjectiveGet += ChangeCodeTxt;
    }

    private void OnDisable()
    {
        SliderCont.OnSliderMoved -= SliderMoved;
        RadarVision.OnObjectiveGet -= ChangeCodeTxt;
    }

    void ChangeCodeTxt(string code) 
    {
        codeText.text = baseTxt + code;
    }

    void SliderMoved(float value, SliderType type)
    {
        switch (type) 
        {
            case SliderType.Position:
                Vector3 offset = radarPoint.forward * (maxDist * value);
                radarPoint.position = radarPointZero.position + offset;
                onCenter = Mathf.Abs(value) < 0.015;

                    return;

            case SliderType.Rotation:
                if (!onCenter) return;

                float angle = (value - 0.5f) * 360f;
                radarPoint.localRotation = startLocalRot * Quaternion.Euler(0f, angle, 0f);
                return;
        }
    }

    void Update()
    {
        //PONER TIMER
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnRandomObjective();
        }
    }

    void SpawnRandomObjective()
    {
        currentObjective.SetActive(true);

        float minRadius = innerMargin;
        float maxRadius = maxDist - outerMargin;

        float randomRadius = Random.Range(minRadius, maxRadius);
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);

        // Coordenadas circulares
        float x = Mathf.Cos(randomAngle);
        float y = Mathf.Sin(randomAngle);

        // Dirección en el plano LOCAL del radar
        Vector3 direction =
            radarPointZero.right * x +
            radarPointZero.forward * y;

        Vector3 finalPosition =
            radarPointZero.position +
            direction.normalized * randomRadius;

        currentObjective.transform.position = finalPosition;
    }
}
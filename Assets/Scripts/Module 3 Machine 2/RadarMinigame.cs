using System;
using UnityEngine;

public enum SliderType { Rotation, Position }


public class RadarMinigame : MonoBehaviour
{
    public Transform radarPointZero;
    public Transform radarPoint;
    public float maxDist = 0.25f;
    public TMPro.TextMeshProUGUI codeText;
    public SliderType type;
    public GameObject objectivePrefab;

    private string baseTxt = "CODE: ";
    private bool onCenter;
    private Quaternion startLocalRot;

    void Start()
    {
        codeText.text = baseTxt;
        startLocalRot = radarPoint.localRotation;
    }

    private void OnEnable()
    {
        SliderCont.OnSliderMoved += SliderMoved;
        RadarVision.OnObjectiveGet += ChangeCodeTxt;
        GameManager.OnSpawnObj += SpawnRandomObjective;
    }

    private void OnDisable()
    {
        SliderCont.OnSliderMoved -= SliderMoved;
        RadarVision.OnObjectiveGet -= ChangeCodeTxt;
        GameManager.OnSpawnObj -= SpawnRandomObjective;
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
                if (value == 0)
                    onCenter = true;
                else
                    onCenter = false;

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
        Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * maxDist;

        Vector3 spawnPos = radarPointZero.transform.position
            + radarPointZero.right * randomPoint.x
            + radarPointZero.forward * randomPoint.y;

        Instantiate(objectivePrefab, spawnPos, Quaternion.identity);
    }
}

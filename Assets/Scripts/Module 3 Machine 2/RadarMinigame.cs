using UnityEngine;

public class RadarMinigame : MonoBehaviour
{
    public Transform radarPointZero;
    public Transform radarPoint;
    public float maxDist = 0.25f;

    private Vector3 maxPosition;


    void Start()
    {
        radarPointZero = radarPoint.GetComponent<Transform>();
        maxPosition = radarPointZero.position + new Vector3(0, 0, maxDist);
    }

    private void OnEnable()
    {
        SliderCont.OnSliderMoved += SliderMoved;
    }

    private void OnDisable()
    {
        SliderCont.OnSliderMoved -= SliderMoved;
    }
    void SliderMoved(float value)
    {
        Vector3 localPos = radarPoint.localPosition;

        localPos.z = Mathf.Lerp(0f, maxDist, value);

        radarPoint.localPosition = localPos;
    }
}

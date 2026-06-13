using System;
using UnityEngine;

public class SliderCont : MonoBehaviour
{
    public GameObject sliderPhy;

    public Transform minPos;
    public Transform maxPos;

    bool isDragging;
    public static bool IsAtMinPos { get; private set; } = true;

    public SliderType type;
    public static Action<float, SliderType> OnSliderMoved;

    public void Start()
    {
        sliderPhy.transform.position = minPos.transform.position;
        IsAtMinPos = true;
    }

    void Update()
    {
        HandleDrag();
    }

    void HandleDrag() 
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit)) 
            {
                if (hit.transform.gameObject == sliderPhy) 
                {
                    isDragging = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            isDragging = false;
        }

        if (!isDragging) return;

        Ray dragRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane planeSlide = new Plane(-Camera.main.transform.forward, minPos.position);

        if (planeSlide.Raycast(dragRay, out float enter)) 
        {
            Vector3 hitPoint = dragRay.GetPoint(enter);

            Vector3 dir = (maxPos.position - minPos.position).normalized;
            Vector3 projected = Vector3.Project(hitPoint - minPos.position, dir);

            float distance = Vector3.Dot(projected, dir);
            float maxDistance = Vector3.Distance(minPos.position, maxPos.position);

            float t = Mathf.Clamp01(distance / maxDistance);
            IsAtMinPos = (t < 0.01f);
            sliderPhy.transform.position = Vector3.Lerp(minPos.position, maxPos.position, t);

            OnSliderMoved?.Invoke(t, type);
        }
    }
}

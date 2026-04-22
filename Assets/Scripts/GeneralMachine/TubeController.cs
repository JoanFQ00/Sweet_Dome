using UnityEngine;
using System.Collections.Generic;
using System;

public class TubeController : MonoBehaviour
{
    public enum TubeMode { Suck, Out}

    [Header("Movement Related")]
    public List<Transform> tubePositions;
    public GameObject tube;
    public float tubeSpeed = 2.5f;

    private Vector3 originalPos;
    private Vector3 finalPos;
    private bool movingTube = false;
    private int currentTubePos;
    private float movePct;

    [Header("Capacity Related")]
    public float maxCapacity = 100f;
    public float startCapacity = 50.0f;
    public float outSpeed = 1.0f;
    public float inSpeed = 1.0f;

    private float currentCapacity;

    [Header("Actions")]
    public TubeMode mode;
    public float suckSpeed = 1.0f;
    public float outCapacity = 1.0f;
    public float downSpeed = 1.25f;
    public float downFinalPos = -2.5f;

    private bool isInActionMove;
    private bool isInAction;

    [Header("Funnels")]
    public Funnel cityFunnel;
    public Funnel blenderFunnel;
    public Funnel domeFunnel;

    private void Start()
    {
        tube.transform.position = tubePositions[0].transform.position;
        currentTubePos = 0;
        currentCapacity = startCapacity;
        isInActionMove = false;
        mode = TubeMode.Suck;
    }

    private void Update()
    {
        if(movingTube)
            MoveTube();

        if (!movingTube && isInActionMove)
            ModeMove();

        if (isInAction)
            ModeAction();

        if (Input.GetMouseButtonDown(0))  // Per fer testing, CAL PASARHO A BOTONS
        {
            ChangeTubePositionRight();
        }

        if (Input.GetMouseButtonDown(1))  // Per fer testing, CAL PASARHO A BOTONS
        {
            if (movingTube) return;

            ActivateTube();
        }
    }

    void ActivateTube() 
    {
        isInActionMove = true;
        originalPos = tube.transform.position;
        finalPos = tube.transform.position + new Vector3(0, -2.5f, 0);
    }

    void MoveTube() 
    {
        movePct += Time.deltaTime * tubeSpeed;
        tube.transform.position = Vector3.Lerp(originalPos, finalPos, movePct);

        if (movePct >= 1) 
        {
            tube.transform.position = finalPos;
            movingTube = false;
            movePct = 0;
        }
    }

    void ModeMove() 
    {
        movePct += Time.deltaTime * downSpeed;
        tube.transform.position = Vector3.Lerp(originalPos, finalPos, movePct);

        if (movePct >= 1) 
        {
            tube.transform.position = finalPos;
            isInActionMove = false;
            isInAction = true;
            movePct = 0f;
        }
    }

    void ModeAction() 
    {
        if (mode == TubeMode.Out) 
        {
            if (currentTubePos == 0)
                cityFunnel.Fill();

            if (currentTubePos == 1)
                blenderFunnel.Fill();

            if (currentTubePos == 2)
                domeFunnel.Fill();
        }

        if (mode == TubeMode.Suck) 
        {
            if (currentTubePos == 0)
                cityFunnel.Empty();

            if (currentTubePos == 1)
                blenderFunnel.Empty();

            if (currentTubePos == 2)
                domeFunnel.Empty();
        }
    }

    void ChangeMode() 
    {
        if (mode == TubeMode.Suck)
            mode = TubeMode.Out;
        else if (mode == TubeMode.Out)
            mode = TubeMode.Suck;
    }

    public void ChangeTubePositionRight() 
    {
        if (currentTubePos + 1 >= tubePositions.Count)
            return; // Fer animacio de no puc anar cap alla
        else
            currentTubePos++;

        originalPos = tube.transform.position;
        finalPos = tubePositions[currentTubePos].transform.position;
        movePct = 0f;
        movingTube = true;
    }

    public void ChangeTubePositionLeft() 
    {
        if (currentTubePos - 1 < 0)
            return; // Fer animacio de no puc anar cap alla
        else
            currentTubePos--;

        originalPos = tube.transform.position;
        finalPos = tubePositions[currentTubePos].transform.position;
        movePct = 0f;
        movingTube = true;
    }
}

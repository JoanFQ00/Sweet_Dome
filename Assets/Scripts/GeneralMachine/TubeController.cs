using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class TubeController : MonoBehaviour
{
    public enum TubeMode { Suck, Out }
    public enum TubeState { Idle, MovingHorizontal, MovingDown, Acting, MovingUp }

    [Header("Movement Related")]
    public List<Transform> tubePositions;
    public GameObject tube;
    public float tubeSpeed = 2.5f;
    public float downSpeed = 1.25f;

    private Vector3 originalPos;
    private Vector3 finalPos;
    private float movePct;
    private int currentTubePos;

    [Header("Capacity")]
    public float maxCapacity = 200f;
    public float outSpeed = 1.0f;
    public float inSpeed = 1.0f;

    private float currentCapacity;

    [Header("Mode")]
    public TubeMode mode;

    [Header("Funnels")]
    public Funnel cityFunnel;
    public Funnel blenderFunnel;
    public Funnel domeFunnel;

    private TubeState state = TubeState.Idle;

    [Header("Debug")]
    public Color suckColor;
    public Color outColor;
    public MeshRenderer tubeMeshRenderer;


    void Start()
    {
        currentCapacity = maxCapacity/2;
        tube.transform.position = tubePositions[0].position;
        currentTubePos = 0;
        mode = TubeMode.Suck;
        tubeMeshRenderer = tube.GetComponent<MeshRenderer>();
        ChangeColor();
    }

    void Update()
    {
        switch (state)
        {
            case TubeState.MovingHorizontal:
                Move(tubeSpeed);
                break;

            case TubeState.MovingDown:
                Move(downSpeed);
                break;

            case TubeState.MovingUp:
                Move(downSpeed);
                break;

            case TubeState.Acting:
                ModeAction();
                break;
        }
    }

    public void ActivateTube()
    {
        if (state == TubeState.Idle)
        {
            originalPos = tube.transform.position;
            finalPos = originalPos + Vector3.down * 2.5f;

            movePct = 0f;
            state = TubeState.MovingDown;
        }
        else if (state == TubeState.Acting)
        {
            originalPos = tube.transform.position;
            finalPos = originalPos + Vector3.up * 2.5f;

            movePct = 0f;
            state = TubeState.MovingUp;
        }
    }

    public void ChangeColor() 
    {
        if (mode == TubeMode.Out)
            tubeMeshRenderer.material.color = outColor;
        else 
            tubeMeshRenderer.material.color = suckColor;
    }

    public void StopAction()
    {
        if (state != TubeState.Acting) return;

        originalPos = tube.transform.position;
        finalPos = originalPos + Vector3.up * 2.5f;

        movePct = 0f;
        state = TubeState.MovingUp;
    }

    public void ChangeTubePositionRight()
    {
        if (state != TubeState.Idle) return;

        if (currentTubePos + 1 >= tubePositions.Count)
            return;

        currentTubePos++;

        StartHorizontalMove();
    }

    public void ChangeTubePositionLeft()
    {
        if (state != TubeState.Idle) return;

        if (currentTubePos - 1 < 0)
            return;

        currentTubePos--;

        StartHorizontalMove();
    }

    public void ChangeMode()
    {
        if (mode == TubeMode.Out)
        {
            mode = TubeMode.Suck;
            ChangeColor();
        }
        else 
        {
            mode = TubeMode.Out;
            ChangeColor();
        }
    }

    void StartHorizontalMove()
    {
        originalPos = tube.transform.position;
        finalPos = tubePositions[currentTubePos].position;
        movePct = 0f;

        state = TubeState.MovingHorizontal;
    }

    void Move(float speed)
    {
        movePct += Time.deltaTime * speed;
        tube.transform.position = Vector3.Lerp(originalPos, finalPos, movePct);

        if (movePct >= 1f)
        {
            tube.transform.position = finalPos;
            movePct = 0f;
            OnMoveComplete();
        }
    }

    void OnMoveComplete()
    {
        switch (state)
        {
            case TubeState.MovingHorizontal:
                state = TubeState.Idle;
                break;

            case TubeState.MovingDown:
                state = TubeState.Acting;
                break;

            case TubeState.MovingUp:
                state = TubeState.Idle;
                break;
        }
    }

    void ModeAction()
    {
        Funnel funnel = GetCurrentFunnel();
        if (funnel == null) return;

        float amount;
        if (mode == TubeMode.Out)
            amount = Time.deltaTime * outSpeed;
        else
            amount = Time.deltaTime * inSpeed;

        if (mode == TubeMode.Out)
        {
            funnel.Fill(amount);
            currentCapacity -= amount;
        }
        else
        {
            funnel.Empty(amount);
            currentCapacity += amount;
        }
    }

    Funnel GetCurrentFunnel()
    {
        switch (currentTubePos)
        {
            case 0: return cityFunnel;
            case 1: return blenderFunnel;
            case 2: return domeFunnel;
        }
        return null;
    }
}
//using UnityEngine;
//using System.Collections.Generic;
//using UnityEngine.Rendering;
//using UnityEngine.UIElements;

//public class TubeController : MonoBehaviour
//{
//    public enum LeverMode
//    {
//        Neutral,
//        Suck,
//        Out
//    }
//    public enum TubeMode { Suck, Out }
//    public enum TubeState { Idle, MovingHorizontal, MovingDown, Acting, MovingUp }

//    [Header("Movement Related")]
//    public List<Transform> tubePositions;
//    public GameObject tube;
//    public float tubeSpeed = 2.5f;
//    public float downSpeed = 1.25f;

//    private Vector3 originalPos;
//    private Vector3 finalPos;
//    private float movePct;
//    private int currentTubePos;

//    [Header("Capacity")]
//    public float maxCapacity = 200f;
//    public float outSpeed = 1.0f;
//    public float inSpeed = 1.0f;

//    private float currentCapacity;

//    [Header("Mode")]
//    public TubeMode mode;
//    public LeverMode currentMode = LeverMode.Neutral;

//    [Header("Funnels")]
//    public Funnel cityFunnel;
//    public Funnel blenderFunnel;
//    public Funnel domeFunnel;

//    private TubeState state = TubeState.Idle;

//    [Header("Debug")]
//    public Color suckColor;
//    public Color outColor;
//    public MeshRenderer tubeMeshRenderer;


//    void Start()
//    {
//        currentCapacity = maxCapacity/2;
//        tube.transform.position = tubePositions[0].position;
//        currentTubePos = 0;
//        mode = TubeMode.Suck;
//        tubeMeshRenderer = tube.GetComponent<MeshRenderer>();
//        ChangeColor();
//    }

//    void Update()
//    {
//        switch (state)
//        {
//            case TubeState.MovingHorizontal:
//                Move(tubeSpeed);
//                break;

//            case TubeState.MovingDown:
//                Move(downSpeed);
//                break;

//            case TubeState.MovingUp:
//                Move(downSpeed);
//                break;

//            case TubeState.Acting:
//                ModeAction();
//                break;
//        }
//    }

//    public void ActivateTube()
//    {
//        if (state == TubeState.Idle)
//        {
//            originalPos = tube.transform.position;
//            finalPos = originalPos + Vector3.down * 2.5f;

//            movePct = 0f;
//            state = TubeState.MovingDown;
//        }
//        else if (state == TubeState.Acting)
//        {
//            originalPos = tube.transform.position;
//            finalPos = originalPos + Vector3.up * 2.5f;

//            movePct = 0f;
//            state = TubeState.MovingUp;
//        }
//    }

//    public void ChangeColor() 
//    {
//        if (mode == TubeMode.Out)
//            tubeMeshRenderer.material.color = outColor;
//        else 
//            tubeMeshRenderer.material.color = suckColor;
//    }

//    public void StopAction()
//    {
//        if (state != TubeState.Acting) return;

//        originalPos = tube.transform.position;
//        finalPos = originalPos + Vector3.up * 2.5f;

//        movePct = 0f;
//        state = TubeState.MovingUp;
//    }

//    public void ChangeTubePositionRight()
//    {
//        if (state != TubeState.Idle) return;

//        if (currentTubePos + 1 >= tubePositions.Count)
//            return;

//        currentTubePos++;

//        StartHorizontalMove();
//    }

//    public void ChangeTubePositionLeft()
//    {
//        if (state != TubeState.Idle) return;

//        if (currentTubePos - 1 < 0)
//            return;

//        currentTubePos--;

//        StartHorizontalMove();
//    }

//    public void ChangeMode()
//    {
//        if (mode == TubeMode.Out)
//        {
//            mode = TubeMode.Suck;
//            ChangeColor();
//        }
//        else 
//        {
//            mode = TubeMode.Out;
//            ChangeColor();
//        }
//    }

//    void StartHorizontalMove()
//    {
//        originalPos = tube.transform.position;
//        finalPos = tubePositions[currentTubePos].position;
//        movePct = 0f;

//        state = TubeState.MovingHorizontal;
//    }

//    void Move(float speed)
//    {
//        movePct += Time.deltaTime * speed;
//        tube.transform.position = Vector3.Lerp(originalPos, finalPos, movePct);

//        if (movePct >= 1f)
//        {
//            tube.transform.position = finalPos;
//            movePct = 0f;
//            OnMoveComplete();
//        }
//    }

//    void OnMoveComplete()
//    {
//        switch (state)
//        {
//            case TubeState.MovingHorizontal:
//                state = TubeState.Idle;
//                break;

//            case TubeState.MovingDown:
//                state = TubeState.Acting;
//                break;

//            case TubeState.MovingUp:
//                state = TubeState.Idle;
//                break;
//        }
//    }

//    void ModeAction()
//    {
//        Funnel funnel = GetCurrentFunnel();
//        if (funnel == null) return;

//        float amount;
//        if (mode == TubeMode.Out)
//            amount = Time.deltaTime * outSpeed;
//        else
//            amount = Time.deltaTime * inSpeed;

//        if (mode == TubeMode.Out)
//        {
//            funnel.Fill(amount);
//            currentCapacity -= amount;
//        }
//        else
//        {
//            funnel.Empty(amount);
//            currentCapacity += amount;
//        }
//    }

//    Funnel GetCurrentFunnel()
//    {
//        switch (currentTubePos)
//        {
//            case 0: return cityFunnel;
//            case 1: return blenderFunnel;
//            case 2: return domeFunnel;
//        }
//        return null;
//    }
//}
using UnityEngine;
using System.Collections.Generic;

public class TubeController : MonoBehaviour
{
    public enum LeverMode
    {
        Neutral,
        Suck,
        Out
    }

    public enum TubeState
    {
        Idle,
        MovingHorizontal,
        MovingDown,
        Working,
        MovingUp
    }

    [System.Serializable]
    public class TubeData
    {
        public string tubeName;

        public LiquidType liquidType;

        public GameObject tubeObject;

        public MeshRenderer meshRenderer;

        public List<Transform> positions;

        public List<Funnel> compartments;

        public float maxCapacity = 100f;

        public float currentCapacity;

        [HideInInspector]
        public int currentPosition;
    }

    [Header("Tubes")]
    public TubeData pinkTube;

    public TubeData magentaTube;

    private TubeData currentTube;

    [Header("Movement")]
    public float horizontalSpeed = 2f;

    public float verticalSpeed = 2f;

    public float verticalDistance = 2.5f;

    [Header("Transfer")]
    public float transferSpeed = 10f;

    public float dumpSpeed = 25f;

    [Header("State")]
    public LeverMode currentMode;

    public TubeState currentState;

    private Vector3 originalPos;

    private Vector3 targetPos;

    private float movePct;

    //private bool isDumping;

    private void Start()
    {
        currentTube = pinkTube;

        SetupTube(pinkTube);
        SetupTube(magentaTube);
    }

    void SetupTube(TubeData tube)
    {
        tube.currentPosition = 0;

        if (tube.positions.Count > 0)
        {
            tube.tubeObject.transform.position = tube.positions[0].position;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case TubeState.MovingHorizontal:
                Move(horizontalSpeed);
                break;

            case TubeState.MovingDown:
                Move(verticalSpeed);
                break;

            case TubeState.MovingUp:
                Move(verticalSpeed);
                break;

            case TubeState.Working:
                Work();
                break;
        }

        //if (isDumping)
        //{
        //    DumpLiquid();
        //}
    }

    public void SwapTube()
    {
        if (currentTube == pinkTube)
            currentTube = magentaTube;
        else
            currentTube = pinkTube;
    }

    public void MoveRight()
    {
        if (currentState != TubeState.Idle)
            return;

        if (currentTube.currentPosition + 1 >= currentTube.positions.Count)
            return;

        currentTube.currentPosition++;

        StartHorizontalMove();
    }

    public void MoveLeft()
    {
        if (currentState != TubeState.Idle)
            return;

        if (currentTube.currentPosition - 1 < 0)
            return;

        currentTube.currentPosition--;

        StartHorizontalMove();
    }

    void StartHorizontalMove()
    {
        originalPos = currentTube.tubeObject.transform.position;
        targetPos = currentTube.positions[currentTube.currentPosition].position;
        movePct = 0f;
        currentState = TubeState.MovingHorizontal;
    }

    void StartMoveDown()
    {
        originalPos = currentTube.tubeObject.transform.position;
        targetPos = originalPos + Vector3.down * verticalDistance;
        movePct = 0f;
        currentState = TubeState.MovingDown;
    }

    void StartMoveUp()
    {
        originalPos = currentTube.tubeObject.transform.position;
        targetPos = originalPos + Vector3.up * verticalDistance;
        movePct = 0f;
        currentState = TubeState.MovingUp;
    }

    void Move(float speed)
    {
        movePct += Time.deltaTime * speed;
        currentTube.tubeObject.transform.position = Vector3.Lerp(originalPos, targetPos, movePct);

        if (movePct >= 1f)
        {
            currentTube.tubeObject.transform.position = targetPos;
            movePct = 0f;
            OnMoveComplete();
        }
    }

    void OnMoveComplete()
    {
        switch (currentState)
        {
            case TubeState.MovingHorizontal:
                currentState = TubeState.Idle;
                break;
            case TubeState.MovingDown:
                currentState = TubeState.Working;
                break;
            case TubeState.MovingUp:
                currentState = TubeState.Idle;
                break;
        }
    }

    public void SetNeutral()
    {
        currentMode = LeverMode.Neutral;
        if (currentState == TubeState.Working)
        {
            StartMoveUp();
        }
    }

    public void SetSuck()
    {
        currentMode = LeverMode.Suck;
        if (currentState == TubeState.Idle)
        {
            StartMoveDown();
        }
    }

    public void SetOut()
    {
        currentMode = LeverMode.Out;
        if (currentState == TubeState.Idle)
        {
            StartMoveDown();
        }
    }


    void Work()
    {
        Funnel compartment = currentTube.compartments[currentTube.currentPosition];

        if (compartment.acceptedLiquid != currentTube.liquidType)
            return;

        float amount = transferSpeed * Time.deltaTime;

        switch (currentMode)
        {
            case LeverMode.Suck:

                if (currentTube.currentCapacity >= currentTube.maxCapacity)
                    return;

                if (!compartment.CanEmpty(amount))
                    return;

                compartment.Empty(amount);
                currentTube.currentCapacity += amount;
                break;
            case LeverMode.Out:

                if (currentTube.currentCapacity <= 0f)
                    return;

                if (!compartment.CanFill(amount))
                    return;
                compartment.Fill(amount);
                currentTube.currentCapacity -= amount;
                break;
        }

        currentTube.currentCapacity = Mathf.Clamp(currentTube.currentCapacity,0f, currentTube.maxCapacity);
    }

    //public void StartDump()
    //{
    //    isDumping = true;
    //}

    //public void StopDump()
    //{
    //    isDumping = false;
    //}

    //void DumpLiquid()
    //{
    //    currentTube.currentCapacity -= dumpSpeed * Time.deltaTime;
    //    currentTube.currentCapacity = Mathf.Clamp(currentTube.currentCapacity,0f, currentTube.maxCapacity);
    //}
}
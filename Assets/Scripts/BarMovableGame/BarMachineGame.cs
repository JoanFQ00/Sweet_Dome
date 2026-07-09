using UnityEngine;
using TMPro;
using System.Collections;

public class BarMachineGame : MonoBehaviour, IResetable
{
    [Header("Referencias")]
    public Transform bar;
    public Transform leftLimit;
    public Transform rightLimit;

    [Header("Zonas")]
    public GameObject phase1Zone;
    public GameObject phase2Zone;
    public GameObject phase3Zone;

    [Header("Movimiento")]
    public float speed = 0.25f;

    [Header("Funnels")]
    [SerializeField] private FunnelBase sourceFunnel;
    [SerializeField] private FunnelBase targetFunnel;

    private bool moving = true;
    private bool toRight = true;
    private bool completed = false;

    private int phase = 0;
    private Vector3 moveDirection;

    private Vector3 initialLocalPosition;

    private bool isPlayerSeated = false;
    public bool IsGameActive => isPlayerSeated && !completed && moving;

    void Start()
    {
        if (bar == null || leftLimit == null || rightLimit == null)
        {
            enabled = false;
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        initialLocalPosition = bar.localPosition;
        moveDirection = (rightLimit.position - leftLimit.position).normalized;

        InitializeGameLogic();
    }

    private void OnEnable()
    {
        MiniGameTest.OnMachineEntered += HandleMachineSeatInteraction;
    }

    private void OnDisable()
    {
        MiniGameTest.OnMachineEntered -= HandleMachineSeatInteraction;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    void Update()
    {
        if (!isPlayerSeated && !completed)
        {
            moving = false;
            phase = 0;
            DeactivateAllZones();

            if (Vector3.Distance(bar.localPosition, initialLocalPosition) > 0.001f)
            {
                bar.localPosition = Vector3.Lerp(bar.localPosition, initialLocalPosition, 8f * Time.deltaTime);
            }
            return;
        }

        int visibleLiquid = (sourceFunnel != null) ? Mathf.RoundToInt(sourceFunnel.CurrentCapacity) : 0;
        bool hasLiquid = visibleLiquid >= 1;

        if (completed && hasLiquid)
        {
            completed = false;
            phase = 0;
            moving = true;
            if (phase1Zone != null) phase1Zone.SetActive(true);
        }

        if (!hasLiquid)
        {
            moving = false;
            phase = 0;

            if (completed) completed = false;

            DeactivateAllZones();

            if (Vector3.Distance(bar.localPosition, initialLocalPosition) > 0.001f)
            {
                bar.localPosition = Vector3.Lerp(bar.localPosition, initialLocalPosition, 8f * Time.deltaTime);
            }
            return;
        }

        CheckLiquidState();

        if (!moving || completed)
            return;

        float direction = toRight ? 1f : -1f;
        bar.position += moveDirection * direction * speed * Time.deltaTime;

        float distanceToRight = Vector3.Distance(bar.position, rightLimit.position);
        float distanceToLeft = Vector3.Distance(bar.position, leftLimit.position);

        if (distanceToRight < 0.02f || (toRight && Vector3.Distance(leftLimit.position, bar.position) > Vector3.Distance(leftLimit.position, rightLimit.position)))
        {
            if (toRight)
            {
                bar.position = rightLimit.position;
                toRight = false;
            }
        }

        if (distanceToLeft < 0.02f || (!toRight && Vector3.Distance(rightLimit.position, bar.position) > Vector3.Distance(leftLimit.position, rightLimit.position)))
        {
            if (!toRight)
            {
                bar.position = leftLimit.position;
                toRight = true;
            }
        }
    }

    private void HandleMachineSeatInteraction(bool entered)
    {
        isPlayerSeated = entered;

        if (isPlayerSeated)
        {
            InitializeGameLogic();
        }
    }

    public void PressButtonMovingBar()
    {
        if (!isPlayerSeated || completed)
            return;

        moving = false;
        CheckGoodArea();
    }

    private void CheckLiquidState()
    {
        if (completed || sourceFunnel == null)
            return;

        if (phase == 0)
        {
            int visibleLiquid = Mathf.RoundToInt(sourceFunnel.CurrentCapacity);
            bool hasLiquid = visibleLiquid >= 1;

            if (phase1Zone != null && !phase1Zone.activeSelf)
            {
                phase1Zone.SetActive(hasLiquid);
            }
            moving = hasLiquid;
        }
    }

    void CheckGoodArea()
    {
        GameObject currentZone = GetCurrentZone();
        if (currentZone == null) return;

        Renderer zoneRenderer = currentZone.GetComponent<Renderer>();
        if (zoneRenderer == null) return;

        bool success = zoneRenderer.bounds.Contains(bar.position);

        if (success) Correct();
        else Fail();
    }

    GameObject GetCurrentZone()
    {
        switch (phase)
        {
            case 0: return phase1Zone;
            case 1: return phase2Zone;
            case 2: return phase3Zone;
            default: return null;
        }
    }

    void Correct()
    {
        if (phase == 0)
        {
            if (phase1Zone != null) phase1Zone.SetActive(false);
            if (phase2Zone != null) phase2Zone.SetActive(true);
            phase = 1;
            moving = true;
            return;
        }

        if (phase == 1)
        {
            if (phase2Zone != null) phase2Zone.SetActive(false);
            if (phase3Zone != null) phase3Zone.SetActive(true);
            phase = 2;
            moving = true;
            return;
        }

        if (phase == 2)
        {
            CompleteMiniGame();
        }
    }

    void Fail()
    {
        if (sourceFunnel != null)
        {
            sourceFunnel.RemoveLiquid(sourceFunnel.CurrentCapacity);
        }

        phase = 0;
        DeactivateAllZones();
        moving = false;
    }

    void CompleteMiniGame()
    {
        completed = true;
        moving = false;

        if (phase3Zone != null) phase3Zone.SetActive(false);

        if (sourceFunnel == null || targetFunnel == null) return;

        int sourceRounded = Mathf.RoundToInt(sourceFunnel.CurrentCapacity);
        int targetCapacityRounded = Mathf.RoundToInt(targetFunnel.CurrentCapacity);
        int targetMaxRounded = Mathf.RoundToInt(targetFunnel.MaxCapacity);

        int availableSpaceRounded = targetMaxRounded - targetCapacityRounded;
        int amountToTransferRounded = Mathf.Min(sourceRounded, availableSpaceRounded);

        if (amountToTransferRounded > 0)
        {
            targetFunnel.AddLiquidFromMinigame(amountToTransferRounded);

            if (sourceRounded == amountToTransferRounded)
            {
                sourceFunnel.RemoveLiquid(sourceFunnel.CurrentCapacity);
            }
            else
            {
                sourceFunnel.RemoveLiquid(amountToTransferRounded);
            }
        }
    }

    public void ResetValues()
    {
        completed = false;
        phase = 0;
        toRight = true;

        if (bar != null)
        {
            bar.localPosition = initialLocalPosition;
        }

        InitializeGameLogic();
    }

    private void InitializeGameLogic()
    {
        int visibleLiquid = (sourceFunnel != null) ? Mathf.RoundToInt(sourceFunnel.CurrentCapacity) : 0;
        bool hasLiquid = visibleLiquid >= 1;

        moving = hasLiquid && isPlayerSeated;

        DeactivateAllZones();

        if (moving && phase1Zone != null)
        {
            phase1Zone.SetActive(true);
        }
    }

    private void DeactivateAllZones()
    {
        if (phase1Zone != null) phase1Zone.SetActive(false);
        if (phase2Zone != null) phase2Zone.SetActive(false);
        if (phase3Zone != null) phase3Zone.SetActive(false);
    }
}
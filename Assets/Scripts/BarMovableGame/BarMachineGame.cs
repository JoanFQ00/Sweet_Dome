using UnityEngine;

public class BarMachineGame : MonoBehaviour
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

    void Start()
    {
        if (bar == null || leftLimit == null || rightLimit == null)
        {
            Debug.LogError("Faltan referencias");
            enabled = false;
            return;
        }

        moveDirection = (rightLimit.position - leftLimit.position).normalized;

        bool hasLiquid = sourceFunnel != null && sourceFunnel.CurrentCapacity > 0;

        moving = hasLiquid;

        if (phase1Zone != null) phase1Zone.SetActive(hasLiquid);
        if (phase2Zone != null) phase2Zone.SetActive(false);
        if (phase3Zone != null) phase3Zone.SetActive(false);
    }

    void Update()
    {
        CheckLiquidState();

        if (!moving || completed)
            return;

        float direction = toRight ? 1f : -1f;

        bar.position += moveDirection * direction * speed * Time.deltaTime;

        float distanceToRight = Vector3.Distance(bar.position, rightLimit.position);
        float distanceToLeft = Vector3.Distance(bar.position, leftLimit.position);

        if (distanceToRight < 0.02f)
            toRight = false;

        if (distanceToLeft < 0.02f)
            toRight = true;
    }

    public void PressButtonMovingBar()
    {
        if (completed)
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
            bool hasLiquid = sourceFunnel.CurrentCapacity > 0;

            phase1Zone.SetActive(hasLiquid);
            moving = hasLiquid;
        }
    }

    void CheckGoodArea()
    {
        GameObject currentZone = GetCurrentZone();

        if (currentZone == null)
            return;

        Renderer zoneRenderer = currentZone.GetComponent<Renderer>();

        if (zoneRenderer == null)
            return;

        bool success = zoneRenderer.bounds.Contains(bar.position);

        Debug.Log("Acierto: " + success);

        if (success)
            Correct();
        else
            Fail();
    }

    GameObject GetCurrentZone()
    {
        switch (phase)
        {
            case 0:
                return phase1Zone;

            case 1:
                return phase2Zone;

            case 2:
                return phase3Zone;

            default:
                return null;
        }
    }

    void Correct()
    {
        if (phase == 0)
        {
            Debug.Log("FASE 1 SUPERADA");

            phase1Zone.SetActive(false);
            phase2Zone.SetActive(true);

            phase = 1;
            moving = true;

            return;
        }

        if (phase == 1)
        {
            Debug.Log("FASE 2 SUPERADA");

            phase2Zone.SetActive(false);
            phase3Zone.SetActive(true);

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
        Debug.Log("FALLO");
        if (sourceFunnel != null)
        {
            sourceFunnel.RemoveLiquid(sourceFunnel.CurrentCapacity);
        }

        phase = 0;

        if (phase1Zone != null) phase1Zone.SetActive(false);
        if (phase2Zone != null) phase2Zone.SetActive(false);
        if (phase3Zone != null) phase3Zone.SetActive(false);

        moving = false;
    }

    void CompleteMiniGame()
    {
        completed = true;
        moving = false;

        if (phase3Zone != null)
            phase3Zone.SetActive(false);

        float availableSpace = targetFunnel.MaxCapacity - targetFunnel.CurrentCapacity;
        float amountToTransfer = Mathf.Min(sourceFunnel.CurrentCapacity, availableSpace);

        targetFunnel.AddLiquid(amountToTransfer);
        sourceFunnel.RemoveLiquid(amountToTransfer);
        Debug.Log("JUEGO COMPLETADO");
    }
}
using UnityEngine;

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

    // Guardamos la posición local de diseño para los retornos y reseteos
    private Vector3 initialLocalPosition;

    // Control interno de seguridad para saber si el jugador sigue sentado
    private bool isPlayerSeated = false;

    // Añade esto dentro de BarMachineGame.cs para que el validador sepa si se puede jugar
    public bool IsGameActive => isPlayerSeated && !completed && moving;

    void Start()
    {
        if (bar == null || leftLimit == null || rightLimit == null)
        {
            Debug.LogError("Faltan referencias");
            enabled = false;
            return;
        }

        // Registramos el módulo en el GameManager para el cambio de día
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        // Guardamos tu posición inicial de la escena de forma sagrada
        initialLocalPosition = bar.localPosition;
        moveDirection = (rightLimit.position - leftLimit.position).normalized;

        InitializeGameLogic();
    }

    private void OnEnable()
    {
        // =================================================================
        // VINCULACIÓN CON LA SILLA: Nos acoplamos al mismo evento del MachineManager
        // =================================================================
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
        // =================================================================
        // CANDADO DE SEGURIDAD 1: Si el jugador NO está sentado, congelamos y Lerp
        // =================================================================
        if (!isPlayerSeated && !completed)
        {
            moving = false;
            phase = 0; // Reseteamos el progreso para que al sentarse empiece en Fase 1
            DeactivateAllZones();

            // Retorno suave por Lerp a la posición inicial estética de tu escena
            if (Vector3.Distance(bar.localPosition, initialLocalPosition) > 0.001f)
            {
                bar.localPosition = Vector3.Lerp(bar.localPosition, initialLocalPosition, 8f * Time.deltaTime);
            }
            return; // Cortamos el Update de inmediato
        }

        // CANDADO DE SEGURIDAD 2: Control de líquido basado en enteros (Evita el bug del 0.2)
        int visibleLiquid = (sourceFunnel != null) ? Mathf.RoundToInt(sourceFunnel.CurrentCapacity) : 0;
        bool hasLiquid = visibleLiquid >= 1;

        // Si estaba completado pero vuelve a entrar líquido visible, reiniciamos partida
        if (completed && hasLiquid)
        {
            completed = false;
            phase = 0;
            moving = true;
            if (phase1Zone != null) phase1Zone.SetActive(true);
        }

        // Si el líquido cae por debajo del umbral visible, congelamos todo
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

        // Flujo normal de juego si hay líquido entero disponible y estamos sentados
        CheckLiquidState();

        if (!moving || completed)
            return;

        float direction = toRight ? 1f : -1f;
        bar.position += moveDirection * direction * speed * Time.deltaTime;

        // Sistema de waypoints controlado para evitar fugas definitivas
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

    // =================================================================
    // MANEJADOR DEL EVENTO DE LA SILLA (Lanzado por tu sistema de juego)
    // =================================================================
    private void HandleMachineSeatInteraction(bool entered)
    {
        isPlayerSeated = entered;

        // Si se acaba de sentar y la máquina tiene líquido visible, inicializamos la marcha
        if (isPlayerSeated)
        {
            InitializeGameLogic();
        }
    }

    public void PressButtonMovingBar()
    {
        // Si el jugador no está sentado frente a la pantalla, bloqueamos el botón físico
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
        Debug.Log("Acierto: " + success);

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
            Debug.Log("FASE 1 SUPERADA");
            if (phase1Zone != null) phase1Zone.SetActive(false);
            if (phase2Zone != null) phase2Zone.SetActive(true);
            phase = 1;
            moving = true;
            return;
        }

        if (phase == 1)
        {
            Debug.Log("FASE 2 SUPERADA");
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
        Debug.Log("FALLO");
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

        // Transferencia limpia basada en enteros matemáticos para evitar descuadres en los tanques
        int sourceRounded = Mathf.RoundToInt(sourceFunnel.CurrentCapacity);
        int targetCapacityRounded = Mathf.RoundToInt(targetFunnel.CurrentCapacity);
        int targetMaxRounded = Mathf.RoundToInt(targetFunnel.MaxCapacity);

        int availableSpaceRounded = targetMaxRounded - targetCapacityRounded;
        int amountToTransferRounded = Mathf.Min(sourceRounded, availableSpaceRounded);

        if (amountToTransferRounded > 0)
        {
            targetFunnel.AddLiquid(amountToTransferRounded);

            if (sourceRounded == amountToTransferRounded)
            {
                sourceFunnel.RemoveLiquid(sourceFunnel.CurrentCapacity);
            }
            else
            {
                sourceFunnel.RemoveLiquid(amountToTransferRounded);
            }
        }

        Debug.Log($"JUEGO COMPLETADO - Transferidos {amountToTransferRounded}L enteros.");
    }

    // =================================================================
    // INTERFAZ DE RESETEO DE TU JUEGO (IResetable) - NUEVO DÍA / INTERFAZ
    // =================================================================
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
        // Solo puede inicializar el movimiento real si hay líquido Y el jugador está sentado
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
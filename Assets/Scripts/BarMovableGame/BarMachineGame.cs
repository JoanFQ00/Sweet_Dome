using UnityEngine;

public class BarMachineGame : MonoBehaviour
{
    [Header("Referencias")]
    public Transform bar;
    public Transform leftLimit;
    public Transform rightLimit;

    [Header("Zona verde")]
    public Transform greenZone;

    [Header("Movimiento")]
    public float speed = 0.25f;

    private bool moving = true;
    private bool toRight = true;

    private int phase = 1;
    private bool completed = false;

    private Vector3 originalGreenZoneScale;

    void Start()
    {
        if (bar == null || leftLimit == null || rightLimit == null || greenZone == null)
        {
            Debug.LogError("Faltan referencias en BarMachineGame");
            enabled = false;
            return;
        }

        originalGreenZoneScale = greenZone.localScale;
    }

    void Update()
    {
        if (!moving || completed)
            return;

        float direction = toRight ? 1f : -1f;

        // Movimiento en Z local
        bar.localPosition += Vector3.forward * direction * speed * Time.deltaTime;

        if (bar.localPosition.z >= rightLimit.localPosition.z)
        {
            Vector3 pos = bar.localPosition;
            pos.z = rightLimit.localPosition.z;
            bar.localPosition = pos;

            toRight = false;
        }

        if (bar.localPosition.z <= leftLimit.localPosition.z)
        {
            Vector3 pos = bar.localPosition;
            pos.z = leftLimit.localPosition.z;
            bar.localPosition = pos;

            toRight = true;
        }
    }

    public void PressButtonMovingBar()
    {
        Debug.Log("BOTON PULSADO");

        moving = false;

        CheckGoodArea();
    }

    void CheckGoodArea()
    {
        Renderer zoneRenderer = greenZone.GetComponent<Renderer>();

        if (zoneRenderer == null)
        {
            Debug.LogError("GreenZone necesita un Renderer");
            return;
        }

        float barZ = bar.position.z;

        float min = zoneRenderer.bounds.min.z;
        float max = zoneRenderer.bounds.max.z;

        bool success = barZ >= min && barZ <= max;

        Debug.Log(
            $"Barra:{barZ}  Zona:[{min} - {max}]  Acierto:{success}"
        );

        if (success)
            Correct();
        else
            Fail();
    }

    void Correct()
    {
        if (phase == 1)
        {
            Debug.Log("FASE 1 SUPERADA");

            phase = 2;

            ReduceZone();

            moving = true;
        }
        else
        {
            CompleteMiniGame();
        }
    }

    void ReduceZone()
    {
        greenZone.localScale = new Vector3(
            originalGreenZoneScale.x * 0.5f,
            originalGreenZoneScale.y,
            originalGreenZoneScale.z
        );

        Debug.Log("Zona reducida");
    }

    void Fail()
    {
        Debug.Log("FALLO");

        phase = 1;

        greenZone.localScale = originalGreenZoneScale;

        moving = true;
    }

    void CompleteMiniGame()
    {
        completed = true;
        moving = false;

        Debug.Log("MAQUINA REPARADA");
    }
}
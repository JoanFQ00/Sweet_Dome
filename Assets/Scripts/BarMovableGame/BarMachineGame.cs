using System;
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
    public float speed = 2f;

    private bool moving = true;
    private bool toRight = true;

    private int phase = 1;
    private Vector3 originalGreenZoneScale;
    private bool completed = false;

    void Start()
    {
        if (bar == null || leftLimit == null || rightLimit == null || greenZone == null)
        {
            Debug.LogError("Faltan referencias en BarMachineGame");
        }
        
        originalGreenZoneScale = greenZone.localScale;
    }

    void Update()
    {
        if (!moving)
            return;

        float direction = toRight ? 1 : -1;
        bar.position += Vector3.right * direction * speed * Time.deltaTime;

        if (bar.position.x >= rightLimit.position.x)
            toRight = false;

        if (bar.position.x <= leftLimit.position.x)
            toRight = true;
    }

    public void PressButtonMovingBar()
    {
        moving = false;
        CheckGoodArea();
    }

    void CheckGoodArea()
    {
        float barX = bar.position.x;
        Renderer rendererZone = greenZone.GetComponent<Renderer>();
        float min = rendererZone.bounds.min.x;
        float max = rendererZone.bounds.max.x;

        bool success = barX >= min && barX <= max;

        if (success)
            Correct();
        else
            Fail();
    }

    void Correct()
    {
        if (phase == 1)
        {
            phase = 2;
            ReduceZone();
            ResetMovement();

            Debug.Log("Fase 2");
        }
        else
            CompleteMiniGame();
    }

    void ReduceZone()
    {
        Vector3 escala = greenZone.localScale;
        escala.x *= 0.35f;
        greenZone.localScale = escala;
    }

    void ResetMovement()
    {
        moving = true;
    }

    void Fail()
    {
        Debug.Log("Fallo");
        phase = 1;
        greenZone.localScale = originalGreenZoneScale;
        ResetMovement();
    }

    void CompleteMiniGame()
    {
        Debug.Log("Máquina reparada");
        moving = false;
        completed = true;
    }
}
using UnityEngine;
using System.Collections.Generic;

public class TubeController : MonoBehaviour
{
    public enum TubeMode
    {
        Neutral,
        Suck,
        Out
    }

    [Header("Selected Tube")]
    public bool controllingPinkTube = true;

    [Header("Pink Tube")]
    public Transform pinkTube;
    public List<Transform> pinkPositions;
    public List<Funnel> pinkFunnels;
    public float pinkCapacity;
    private int pinkIndex;

    [Header("Magenta Tube")]
    public Transform magentaTube;
    public List<Transform> magentaPositions;
    public List<Funnel> magentaFunnels;
    public float magentaCapacity;
    private int magentaIndex;

    [Header("Settings")]
    public float maxCapacity = 100f;
    public float verticalOffset = 2.5f;
    public float transferSpeed = 10f;

    private TubeMode currentMode;

    private void Start()
    {
        if (!pinkTube || !magentaTube || !pinkPositions[0] || !magentaPositions[0]) return;

        pinkTube.position = pinkPositions[0].position;
        magentaTube.position = magentaPositions[0].position;
    }

    private void Update()
    {
        HandleTransfer();
    }

    public void SwapTube()
    {
        controllingPinkTube = !controllingPinkTube;
    }

    public void MoveLeft()
    {
        if (controllingPinkTube)
        {
            if (pinkIndex > 0)
            {
                pinkIndex--;
                MovePinkTube();
            }
        }
        else
        {
            if (magentaIndex > 0)
            {
                magentaIndex--;
                MoveMagentaTube();
            }
        }
    }

    public void MoveRight()
    {
        if (controllingPinkTube)
        {
            if (pinkIndex < pinkPositions.Count - 1)
            {
                pinkIndex++;
                MovePinkTube();
            }
        }
        else
        {
            if (magentaIndex < magentaPositions.Count - 1)
            {
                magentaIndex++;
                MoveMagentaTube();
            }
        }
    }

    void MovePinkTube()
    {
        pinkTube.position = pinkPositions[pinkIndex].position;

        if (currentMode != TubeMode.Neutral)
        {
            LowerPinkTube();
        }
    }

    void MoveMagentaTube()
    {
        magentaTube.position = magentaPositions[magentaIndex].position;

        if (currentMode != TubeMode.Neutral)
        {
            LowerMagentaTube();
        }
    }

    public void SetNeutral()
    {
        currentMode = TubeMode.Neutral;

        RaisePinkTube();
        RaiseMagentaTube();
    }

    public void SetSuck()
    {
        currentMode = TubeMode.Suck;

        if (controllingPinkTube)
        {
            LowerPinkTube();
        }
        else
        {
            LowerMagentaTube();
        }
    }

    public void SetOut()
    {
        currentMode = TubeMode.Out;

        if (controllingPinkTube)
        {
            LowerPinkTube();
        }
        else
        {
            LowerMagentaTube();
        }

        Debug.Log("Lever: Expelling");
    }

    void LowerPinkTube()
    {
        Vector3 pos = pinkTube.position;

        pos.y = pinkPositions[pinkIndex].position.y - verticalOffset;

        pinkTube.position = pos;
    }

    void RaisePinkTube()
    {
        Vector3 pos = pinkTube.position;

        pos.y = pinkPositions[pinkIndex].position.y;

        pinkTube.position = pos;
    }

    void LowerMagentaTube()
    {
        Vector3 pos = magentaTube.position;

        pos.y = magentaPositions[magentaIndex].position.y - verticalOffset;

        magentaTube.position = pos;
    }

    void RaiseMagentaTube()
    {
        Vector3 pos = magentaTube.position;

        pos.y = magentaPositions[magentaIndex].position.y;

        magentaTube.position = pos;
    }

    void HandleTransfer()
    {
        if (currentMode == TubeMode.Neutral)
            return;

        float amount = transferSpeed * Time.deltaTime;

        if (controllingPinkTube)
        {
            Funnel funnel = pinkFunnels[pinkIndex];

            if (currentMode == TubeMode.Suck)
            {
                if (pinkCapacity >= maxCapacity)
                    return;

                if (!funnel.CanEmpty(amount))
                    return;

                funnel.Empty(amount);
                pinkCapacity += amount;
            }

            else if (currentMode == TubeMode.Out)
            {
                if (pinkCapacity <= 0f)
                    return;

                if (!funnel.CanFill(amount))
                    return;

                funnel.Fill(amount);

                pinkCapacity -= amount;
            }

            pinkCapacity = Mathf.Clamp(pinkCapacity, 0f, maxCapacity);
        }

        else
        {
            Funnel funnel = magentaFunnels[magentaIndex];

            if (currentMode == TubeMode.Suck)
            {
                if (magentaCapacity >= maxCapacity)
                    return;

                if (!funnel.CanEmpty(amount))
                    return;

                funnel.Empty(amount);

                magentaCapacity += amount;
            }

            else if (currentMode == TubeMode.Out)
            {
                if (magentaCapacity <= 0f)
                    return;

                if (!funnel.CanFill(amount))
                    return;

                funnel.Fill(amount);

                magentaCapacity -= amount;
            }

            magentaCapacity = Mathf.Clamp(magentaCapacity, 0f, maxCapacity);
        }
    }
}
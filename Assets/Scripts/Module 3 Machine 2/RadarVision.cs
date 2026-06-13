using UnityEngine;
using System;

public class RadarVision : MonoBehaviour
{
    public float maxDist = 5f;
    public float visionAngle = 65.0f;
    public LayerMask visionMask;

    public RadarMinigame radarMinigame;

    // BORRAR
    public FunnelBase rawFunnel;

    public static Action<GameObject> OnObjectiveGet;

    public GameObject CurrentTarget { get; private set; }

    void Update()
    {
        if (radarMinigame.objectives.Count <= 0)
            return;

        foreach (GameObject objective in radarMinigame.objectives)
        {
            if (objective == null)
                continue;

            MeshRenderer mesh = objective.GetComponent<MeshRenderer>();

            if (mesh != null)
            {
                mesh.enabled = true;
            }

            if (!objective.activeSelf)
            {
                objective.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!radarMinigame.objectives.Contains(other.gameObject))
            return;
        CurrentTarget = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if(CurrentTarget == other.gameObject)
            CurrentTarget = null;
    }

    bool CheckVision(GameObject objective)
    {
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDist);

        Vector3 left = DirFromAngle(-visionAngle / 2);
        Vector3 right = DirFromAngle(visionAngle / 2);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + left * maxDist);
        Gizmos.DrawLine(transform.position, transform.position + right * maxDist);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);

        if (radarMinigame == null)
            return;

        Gizmos.color = Color.red;

        foreach (GameObject objective in radarMinigame.objectives)
        {
            if (objective != null)
            {
                Gizmos.DrawLine(transform.position, objective.transform.position);
            }
        }
    }

    Vector3 DirFromAngle(float angle)
    {
        Quaternion rot = transform.rotation * Quaternion.Euler(0, angle, 0);
        return rot * Vector3.forward;
    }
}
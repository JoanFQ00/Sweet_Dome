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

    void Update()
    {
        if (radarMinigame.objectives.Count <= 0)
            return;

        foreach (GameObject objective in radarMinigame.objectives)
        {
            if (objective == null)
                continue;

            MeshRenderer mesh = objective.GetComponent<MeshRenderer>();

            if (mesh == null)
                continue;

            bool visible = CheckVision(objective);

            mesh.enabled = visible;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!radarMinigame.objectives.Contains(other.gameObject))
            return;

        OnObjectiveGet?.Invoke(other.gameObject);

        rawFunnel.AddLiquid(20f);
    }

    bool CheckVision(GameObject objective)
    {
        Vector3 dir = objective.transform.position - transform.position;
        float dist = dir.magnitude;

        if (dist > maxDist) return false;
        if (dist < 0.01f) return false;

        dir /= dist;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > visionAngle * 0.5f) return false;

        Ray ray = new Ray(transform.position, dir);

        if (Physics.Raycast(ray, out RaycastHit hit, dist - 0.1f, visionMask))
        {
            return false;
        }
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
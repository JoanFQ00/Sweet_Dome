using UnityEngine;
using System;
using System.Collections.Generic;

public class RadarVision : MonoBehaviour
{
    public float maxDist = 5f;
    public float visionAngle = 65.0f;
    public LayerMask visionMask;
    public RadarMinigame radarMinigame;

    // EVENTO ESTÁTICO GLOBAL: Conecta el botón de ataque con el minijuego de radar
    public static Action<GameObject> OnObjectiveGet;

    // Lista en cola de todos los objetivos que están pisando el cono a la vez
    private List<GameObject> targetsInCone = new List<GameObject>();

    /// <summary>
    /// Devuelve el primer objetivo válido de la cola (si hay alguno).
    /// </summary>
    public GameObject CurrentTarget
    {
        get
        {
            // Limpiamos de la cola naves nulas o destruidas previamente en este frame
            targetsInCone.RemoveAll(item => item == null);
            return targetsInCone.Count > 0 ? targetsInCone[0] : null;
        }
    }

    void Update()
    {
        if (radarMinigame == null || radarMinigame.objectives.Count <= 0) return;

        foreach (GameObject objective in radarMinigame.objectives)
        {
            if (objective == null) continue;

            MeshRenderer mesh = objective.GetComponent<MeshRenderer>();
            if (mesh != null) mesh.enabled = true;

            if (!objective.activeSelf) objective.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (radarMinigame == null || !radarMinigame.objectives.Contains(other.gameObject)) return;

        // Si la nave entra en el trigger y no estaba registrada, la metemos a la cola
        if (!targetsInCone.Contains(other.gameObject))
        {
            targetsInCone.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (targetsInCone.Contains(other.gameObject))
        {
            targetsInCone.Remove(other.gameObject);
        }
    }

    private void OnDisable()
    {
        targetsInCone.Clear();
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

        if (radarMinigame == null) return;

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
using UnityEngine;
using System;

public class RadarVision : MonoBehaviour
{
    public float maxDist = 5f;

    public float visionAngle = 65.0f;
    public LayerMask visionMask;

    public RadarMinigame radarMinigame;

    public static Action<string> OnObjectiveGet;

    private int randomCode;
    private string departingCode;

    [Header("Manager")]
    public MachineManager machineManager;
    public bool completed = false;

    void Start()
    {
        GenerateRandomCode();
    }

    void Update()
    {
        if (CheckVision())
            radarMinigame.CurrentObjective.GetComponent<MeshRenderer>().enabled = true;
        else
            radarMinigame.CurrentObjective.GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (radarMinigame.CurrentObjective != null)
        {
            if (other.gameObject == radarMinigame.CurrentObjective)
            {
                GenerateRandomCode();
                OnObjectiveGet?.Invoke(departingCode);
                other.gameObject.SetActive(false);

                completed = true;

                if (machineManager != null)
                {
                    machineManager.CompleteRadar();
                }
            }
        }
    }

    bool CheckVision() 
    {
        Vector3 dir = radarMinigame.CurrentObjective.transform.position -  transform.position;
        float dist = dir.magnitude;

        if (dist > maxDist) return false;

        dir /= dist;

        float angle = Vector3.Angle(transform.forward, dir);

        if (angle > visionAngle / 2)
            return false;

        if (angle >= Mathf.Cos(visionAngle * 0.5f * Mathf.Deg2Rad))
        {
            Ray ray = new Ray(transform.position, dir);
            if (!Physics.Raycast(ray, dist, visionMask))
                return false;
        }
        return true;   
    }

    void GenerateRandomCode() 
    {
        randomCode = UnityEngine.Random.Range(1000, 9999);
        departingCode = randomCode.ToString();
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

        if (radarMinigame.CurrentObjective != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, radarMinigame.CurrentObjective.transform.position);
        }
    }

    Vector3 DirFromAngle(float angle)
    {
        Quaternion rot = transform.rotation * Quaternion.Euler(0, angle, 0);
        return rot * Vector3.forward;
    }
}

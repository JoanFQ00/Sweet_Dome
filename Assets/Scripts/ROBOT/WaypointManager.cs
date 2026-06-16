using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    [Header("Routine Tasks")]
    public List<Waypoint> taskWaypoints;

    [Header("Story Points")]
    public List<Waypoint> historyWaypoints;

    [Header("Recharge")]
    public Transform rechargeWaypoint;

    public Waypoint GetRandomPatrolWaypoint()
    {
        if (taskWaypoints.Count == 0)
            return null;

        return taskWaypoints[Random.Range(0, taskWaypoints.Count)];
    }
}
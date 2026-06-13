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

    public Waypoint GetHistoryWaypointByKey(string eventKey)
    {
        foreach (Waypoint waypoint in historyWaypoints)
        {
            if (waypoint.historyEventKey == eventKey)
                return waypoint;
        }

        return null;
    }
}
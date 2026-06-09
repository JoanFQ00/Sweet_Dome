using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    [Header("Generic waypoints")]
    public List<Waypoints> genericWaypoints;

    [Header("Narrative waypoints")]
    public List<Waypoints> narrativeWaypoints;

    [Header("Recharge")]
    public Transform rechargeWaypoint;

    private Waypoints lastWaypoints;

    public Waypoints GetRandomGenericWaypoint()
    {

        List<Waypoints> posibleWaypoints = new List<Waypoints>();



        return lastWaypoints;
    }

    public Waypoints GetNarrativeWaypoint(string narrativeID)
    {
        return narrativeWaypoints.Find(w => w.narrativeID == narrativeID);
    }

    public void MarkWaypointUsed(Waypoints waypoint)
    {
        if (waypoint != null)
            waypoint.hasBeenUsed = true;
    }
}

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

    //Method to get random waypoint without repeting the last one
    public Waypoints GetRandomGenericWaypoint()
    {

        List<Waypoints> posibleWaypoints = new List<Waypoints>();



        return lastWaypoints;
    }

    //Method for narrative ones
}

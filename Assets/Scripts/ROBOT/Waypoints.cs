using UnityEngine;

public enum WaypointType
{
    Generic, 
    Narrative
}

public class Waypoints : MonoBehaviour
{
    public WaypointType waypointType;

    [Header("Idle/Zona")]
    public string zoneAnimation;

    [Header("Narrative")]
    public string narrativeID;

    [Header("Animation")]
    public string animationTrigger;

    [HideInInspector]
    public bool hasBeenUsed;
}

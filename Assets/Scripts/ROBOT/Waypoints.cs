using UnityEngine;

public enum WaypointType
{
    Task,
    History
}

public class Waypoint : MonoBehaviour
{
    public WaypointType waypointType;

    [Header("History")]
    public string historyEventKey;

    [Header("Task")]
    public string animationTrigger;
    public float actionDuration = 5f;
}
using UnityEngine;
using UnityEngine.AI;

public class GoToPlayerState : IState
{
    private RobotBehaviour robotWomen;

    public GoToPlayerState(RobotBehaviour robot)
    {
        robotWomen = robot;
    }

    public void OnEnter()
    {
        robotWomen.agent.isStopped = false;
    }

    public void OnState()
    {
        Vector3 offsetDirection = (robotWomen.player.position - robotWomen.transform.position).normalized;
        robotWomen.agent.SetDestination(robotWomen.player.position - offsetDirection * 1.5f);
        robotWomen.LookAtTarget(robotWomen.player.position);
    }

    public void OnExit()
    {
        robotWomen.agent.ResetPath();
    }
}

using UnityEngine;

public class HistoryActionState : IState
{
    private RobotBehaviour robot;
    private float actionTimer;

    public HistoryActionState(RobotBehaviour robot)
    {
        this.robot = robot;
    }

    public void OnEnter()
    {
        //robot.agent.isStopped = false;
        //robot.animator.SetBool("IsMoving", false);
        //robot.ExecuteWaypointAnimation(robot.currentHistoryWaypoint);
        //actionTimer = robot.currentHistoryWaypoint != null ? robot.currentHistoryWaypoint.actionDuration : 5f;
    }

    public void OnState()
    {
        //actionTimer -= Time.deltaTime;
        //if (actionTimer <= 0)
        //{
        //    robot.FinishHistoryAction();
        //}
    }

    public void OnExit()
    {
        robot.stateMachine.ChangeState(robot.patrolState);
        robot.agent.isStopped = false;
    }
}
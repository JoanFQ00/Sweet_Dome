using UnityEngine;

public class TaskActionState : IState
{
    private RobotBehaviour robot;
    private float actionTimer;

    public TaskActionState(RobotBehaviour robot)
    {
        this.robot = robot;
    }

    public void OnEnter()
    {
        robot.agent.isStopped = true;
        robot.animator.SetBool("IsMoving", false);

        robot.ExecuteWaypointAnimation(robot.currentTask);
        actionTimer = robot.currentTask != null ? robot.currentTask.actionDuration : 3f;
    }

    public void OnState()
    {
        actionTimer -= Time.deltaTime;
        if (actionTimer <= 0)
        {
            robot.stateMachine.ChangeState(robot.idleState);
        }
    }

    public void OnExit()
    {
        robot.currentTask = null;
    }
}
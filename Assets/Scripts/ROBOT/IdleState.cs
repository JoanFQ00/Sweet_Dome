using UnityEngine;

public class IdleState : IState
{
    private RobotBehaviour robot;

    private float timer;

    public IdleState(RobotBehaviour robot)
    {
        this.robot = robot;
    }

    public void OnEnter()
    {
        timer = Random.Range(4f, 10f);
    }

    public void OnState()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            robot.stateMachine.ChangeState(robot.patrolState);
        }
    }

    public void OnExit()
    {
    }
}

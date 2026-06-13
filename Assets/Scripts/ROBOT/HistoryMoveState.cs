using UnityEngine;

public class HistoryMoveState : IState
{
    private RobotBehaviour robot;

    public HistoryMoveState(RobotBehaviour robot)
    {
        this.robot = robot;
    }

    public void OnEnter()
    {
        if (robot.currentHistoryWaypoint == null)
        {
            robot.FinishHistoryAction();
            return;
        }
        robot.agent.isStopped = false;
        robot.animator.SetBool("IsMoving", true);
        robot.agent.SetDestination(robot.currentHistoryWaypoint.transform.position);
    }

    public void OnState()
    {
        //if(robot.agent.hasPath)
        //    robot.LookAtTarget(robot.agent.steeringTarget);

        //if(!robot.agent.pathPending && robot.agent.remainingDistance <= 0.5f)
        //{
        //    robot.stateMachine.ChangeState(robot.historyActionState);
        //}

        float currentSpeed = robot.agent.velocity.magnitude;
        robot.animator.SetFloat("Speed", currentSpeed);
        robot.animator.SetBool("IsMoving", currentSpeed > 0.1f);

        if (robot.agent.hasPath)
        {
            robot.LookAtTarget(robot.agent.steeringTarget);
        }

        bool haLlegadoPorDistancia = !robot.agent.pathPending && robot.agent.remainingDistance <= 0.8f;
        bool seHaQuedadoParado = !robot.agent.pathPending && robot.agent.remainingDistance <= 1.5f && currentSpeed <= 0.05f;

        if (haLlegadoPorDistancia || seHaQuedadoParado)
        {
            robot.agent.ResetPath();
            robot.animator.SetFloat("Speed", 0f);
            robot.animator.SetBool("IsMoving", false);
            Waypoint currentWp = robot.waypointManager.GetHistoryWaypointByKey(robot.activeHistoryEvent);

            if (currentWp != null && !string.IsNullOrEmpty(currentWp.animationTrigger))
            {
                robot.animator.SetTrigger(currentWp.animationTrigger);
                Debug.Log($"[History System] Disparando Trigger: {currentWp.animationTrigger}");
            }

            robot.stateMachine.ChangeState(robot.historyActionState);
        }
    }

    public void OnExit()
    {
        robot.agent.ResetPath();
    }
}
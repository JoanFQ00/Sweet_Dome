using UnityEngine;

public class StateMachineChanger
{
    private IState currentRobotState;

    public IState CurrentState => currentRobotState;

    public string CurrentStateName => currentRobotState?.GetType().Name;

    public void ChangeState(IState newState)
    {
        currentRobotState?.OnExit();
        currentRobotState = newState;
        currentRobotState?.OnEnter();
    }

    public void Update()
    {
        if(currentRobotState!= null)
            currentRobotState.OnState();
    }
}

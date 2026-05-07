using UnityEngine;

public class StateMachineChanger
{
    private IState currentRobotState;

    public IState CurrentState => currentRobotState;

    public void ChangeState(IState newState)
    {
        Debug.Log("Cambiando estado a: " + newState);

        currentRobotState?.OnExit();
        currentRobotState = newState;
        currentRobotState?.OnEnter();
    }

    public void Update()
    {
        if(currentRobotState!= null)
        {
            currentRobotState.OnState();
        }
    }
}

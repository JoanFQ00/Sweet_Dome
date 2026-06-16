using UnityEngine;

public interface IState
{
    void OnEnter();
    void OnState();
    void OnExit();
}
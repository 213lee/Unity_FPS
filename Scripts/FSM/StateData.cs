using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void Enter(Enemy owner);
    void Excute(Enemy owner);
    void Exit(Enemy owner);
}

//Enemy FSM의 모든 State를 가지는 State Data
[CreateAssetMenu(fileName = "State Data", menuName = "ScriptableObject/FPS State/State Data", order = 0)]
public class StateData : ScriptableObject
{
    [SerializeField] StopState stop;
    [SerializeField] MoveState move;
    [SerializeField] RotateState rotate;
    [SerializeField] AttackState attack;
    [SerializeField] ReloadState reload;
    [SerializeField] DieState die;

    public IState StopState => stop;
    public IState MoveState => move;
    public IState RotateState => rotate;
    public IState AttackState => attack;
    public IState ReloadState => reload;
    public IState DieState => die;
}
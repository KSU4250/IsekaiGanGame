using UnityEngine;

using EnumTypes;
public abstract class BasePlayerState
{
    protected PlayerStateType stateType;
    protected PlayerManager playerManager { get; private set; }

    public PlayerStateType StateType
    {
        get { return stateType; }
    }

    public BasePlayerState(PlayerManager _playerManager)
    {
        playerManager = _playerManager;
    }

    public abstract void OnEnterState();
    public abstract void OnUpdateState();
    public abstract void OnExitState();
}

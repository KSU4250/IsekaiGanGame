using UnityEngine;

using EnumTypes;

public class DamagedState : BasePlayerState
{
    public DamagedState(PlayerManager playerMng) : base(playerMng)
    {
        stateType = PlayerStateType.Damaged;
    }

    public override void OnEnterState()
    {
        playerManager.AnimationManager.PlayDamagedAnimation();
    }

    public override void OnExitState()
    {

    }

    public override void OnUpdateState()
    {

    }
}

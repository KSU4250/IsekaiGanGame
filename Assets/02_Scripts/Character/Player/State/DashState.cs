using UnityEngine;

using EnumTypes;
using StructTypes;
using System.Collections;

public class DashState : BasePlayerState
{
    public DashState(PlayerManager playerMng) : base(playerMng)
    {
        stateType = PlayerStateType.Dash;
    }

    public override void OnEnterState()
    {
    }

    public override void OnExitState()
    {
        
    }

    public override void OnUpdateState()
    {
        
    }
}

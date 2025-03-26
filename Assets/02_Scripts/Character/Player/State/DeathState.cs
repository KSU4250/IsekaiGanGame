using UnityEngine;

public class DeathState : BasePlayerState
{
    public DeathState(PlayerManager playerMng) : base(playerMng)
    {
        stateType = EnumTypes.PlayerStateType.Death;
    }

    public override void OnEnterState()
    {
        // 플레이어가 죽었을 때, 로컬 게임이 아닌 경우에만 네트워크에게 죽음을 알림.
        if (!GameManager.Instance.IsLocalGame)
        {
            Debug.Log("Hello!");
            ulong clientId = playerManager.PlayerNetworkManager.OwnerClientId;
            playerManager.PlayerNetworkManager.OnNetworkPlayerDeath?.Invoke(clientId);
        }

        // 플레이어가 죽었을 때, 스킬 UI를 비활성화하고 이동을 멈추며 사망 애니메이션을 재생.
        {
            if (playerManager.SkillUIManager != null)
                playerManager.SkillUIManager.SetAllSkillUIEnabled(false);
            if (playerManager.MovementManager != null)
                playerManager.MovementManager.StopMove();
            if (playerManager.AnimationManager != null)
                playerManager.AnimationManager.PlayDeathAnimation();
        }

        playerManager.GetComponent<CharacterController>().enabled = false;

    }

    public override void OnExitState()
    {
        
    }

    public override void OnUpdateState()
    {
        
    }
}

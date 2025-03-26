using UnityEngine;

using EnumTypes;
using StructTypes;

public class SkillState : BasePlayerState
{
    public SkillState(PlayerManager _playerManager) : base(_playerManager)
    {
        stateType = PlayerStateType.Skill;
    }

    public override void OnEnterState()
    {
        playerManager.MovementManager.StopMove();
        playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
    }

    public override void OnExitState()
    {
        playerManager.ParticleController.ForceDestroyParticles();

        if(playerManager.AttackManager.IsMeleeWeaponSet())
        {
            playerManager.AttackManager.DisableMeleeAttack();
        }
    }

    public override void OnUpdateState()
    {
        if (!GameManager.Instance.IsLocalGame && playerManager.PlayerNetworkManager.IsClientPlayer())
            return;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.Dash);
        }

#endif

        // 스킬 사용 상태일 때는 회피 사용 가능함. 회피 스킬 사용 체크.
        InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

        if (inputBuffer.skillType == SkillSlot.Dash)
        {
            playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
        }
    }
}

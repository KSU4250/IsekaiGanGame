using System.Collections;
using UnityEngine;
using Unity.Netcode;

using EnumTypes;
using StructTypes;

public class IdleState : BasePlayerState
{
    private JoystickInputData joystickInputData;
    private Coroutine updateCoroutine = null;
    private Camera mainCamera = null;
    private Vector3 mouseRayPosition = Vector3.zero;

    private bool skillUsedTrigger = false;

    public IdleState(PlayerManager _playerManager) : base(_playerManager)
    {
        stateType = PlayerStateType.Idle;
    }

    public override void OnEnterState()
    {
        /*
        //// 로컬 게임이 아니고, 네트워크 오브젝트가 로컬 플레이어가 아닐 때
        //if (!GameManager.Instance.IsLocalGame && !playerManager.PlayerNetworkManager.IsClientPlayer())
        //    return;
        */

        mainCamera = Camera.main;
        mouseRayPosition = Vector3.zero;
        skillUsedTrigger = false;
    }

    public override void OnExitState()
    {
        playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
    }

    public override void OnUpdateState()
    {
        // 로컬 게임이 아니고, 네트워크 오브젝트가 로컬 플레이어가 아닐 때는 대기 중 입력 처리를 하지 않는다.
        if (!GameManager.Instance.IsLocalGame && !playerManager.PlayerNetworkManager.IsClientPlayer())
            return;

#if UNITY_ANDROID

        // 스킬 사용 처리
        {
            InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

            if (inputBuffer.skillType != SkillSlot.None)
            {
                if (playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData))
                {
                    if (inputBuffer.skillType != SkillSlot.Dash && inputBuffer.skillType != SkillSlot.BasicAttack)
                        skillUsedTrigger = true;
                }

                return;
            }
        }

        if (!skillUsedTrigger)
        {
            // 대기 상태일 때만 움직일 수 있음.
            playerManager.InputManager.GetJoystickInputValue(out joystickInputData);
            playerManager.MovementManager.MoveByJoystick(joystickInputData);
        }

#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        // 스킬 사용 처리
        {
            InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

            if (inputBuffer.skillType != SkillSlot.None)
            {
                playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
            }
        }
        // 스킬 입력 처리
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.Skill_A);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.Skill_B);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.Skill_C);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.Dash);
            }

            if (Input.GetMouseButtonDown(0))
            {
                playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.BasicAttack);
            }
        }

        // 플레이어 이동 처리
        {
            // 오른 쪽 마우스 입력을 감지했을 때 이동 가능한 곳에 마우스가 있을 시
            if (Input.GetMouseButton(1))
            {
                // 마우스의 위치에 이동 가능한 지형이 있을 때만 이동.
                if (playerManager.InputManager.GetMouseRayHitPosition(out mouseRayPosition))
                {
                    // 지정한 위치로 이동.
                    playerManager.MovementManager.MoveToPosition(mouseRayPosition);
                }
                // 마우스의 위치에 이동 가능한 지형이 없으면 이동 중지.
                else
                {
                    playerManager.MovementManager.StopMove();
                    playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
                }
            }
            // 마우스 클릭을 멈추면 이동 중지.
            if (Input.GetMouseButtonUp(1))
            {
                playerManager.MovementManager.StopMove();
                playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
            }
        }

#endif
    }
}

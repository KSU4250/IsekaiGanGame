using UnityEngine;

using EnumTypes;
[CreateAssetMenu(fileName = "ReviveSkill", menuName = "Scriptable Objects/Player Skill/Revive")]
public class ReviveSkill : PlayerSkillBase
{
    public float reviveRange = 2f;

    public override void StartSkill(PlayerManager _player)
    {
        base.StartSkill(_player);
        _player.ChangeState(PlayerStateType.Action);
    }

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);

        ReviveNearPlayers(_player);
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);
        _player.ChangeState(PlayerStateType.Idle);
    }

    private void ReviveNearPlayers(PlayerManager _player)
    {
        Collider[] hits = Physics.OverlapSphere(_player.transform.position, reviveRange, LayerMask.GetMask("Player"));

        foreach (Collider hitCollider in hits)
        {
            PlayerManager playerManager = hitCollider.GetComponent<PlayerManager>();

            if (playerManager != null && playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Death)
            {
                playerManager.RevivePlayer();
            }
        }
    }
}

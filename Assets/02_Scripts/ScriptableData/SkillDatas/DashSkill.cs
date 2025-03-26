using System.Collections;
using UnityEngine;

using EnumTypes;

[CreateAssetMenu(fileName = "DashSkill", menuName = "Scriptable Objects/Player Skill/Dash")]
public class DashSkill : PlayerSkillBase
{
    [Header("Dash Skill")]
    public float dashSpeed = 5f;
    public float dashTime = 1f;
    public bool isForward = true;

    private PlayerSkillMove skillMove = null;

    public DashSkill()
    {
        skillMove = new PlayerSkillMove();
    }

    public override void StartSkill(PlayerManager _player)
    {
        base.StartSkill(_player);

        _player.ChangeState(PlayerStateType.Dash);
    }

    public override void UseSkill(PlayerManager _player)
    {
        Vector3 direction = isForward ? _player.transform.forward : (_player.transform.forward * -1f);

        skillMove.StartPlayerMove(_player, dashSpeed, dashTime, direction);
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);

        _player.ChangeState(PlayerStateType.Idle);
    }
}

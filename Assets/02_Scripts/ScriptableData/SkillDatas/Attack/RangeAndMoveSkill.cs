using System.Collections;
using UnityEngine;

/// <summary>
/// 원거리 공격과 동시에 이동하는 스킬.
/// </summary>
[CreateAssetMenu(fileName = "RangeAndMoveSkill", menuName = "Scriptable Objects/Player Skill/RangeAndMove")]
public class RangeAndMoveSkill : RangeSkill
{
    public float moveSpeed = 1f;
    public float moveTime = 1f;
    public bool isForward = true;

    private PlayerSkillMove skillMove = null;

    public RangeAndMoveSkill()
    {
        skillMove = new PlayerSkillMove();
    }

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);

        Vector3 direction = isForward ? _player.transform.forward : (_player.transform.forward * -1f);
        skillMove.StartPlayerMove(_player, moveSpeed, moveTime, direction);
    }
}

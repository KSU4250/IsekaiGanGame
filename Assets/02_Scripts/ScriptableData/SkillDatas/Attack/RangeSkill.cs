using UnityEngine;

/// <summary>
/// 레이로 원거리 공격을 처리하는 스킬을 처리하기 위한 스크립터블 오브젝트.
/// </summary>
[CreateAssetMenu(fileName = "RangeSkill", menuName = "Scriptable Objects/Player Skill/Range")]
public class RangeSkill : AttackSkill
{
    [Header("Range Skill")]
    public float attackRange = 5f;

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);

        Transform startTr = _player.RangeAttackStartTr;
        Ray ray = new Ray(startTr.position, _player.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange, bossLayerMask))
        {
            GameManager.Instance.DamageToBoss(_player, DamageCalculate(_player), aggro);
        }
    }
}

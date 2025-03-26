using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerPositionAOESkill", menuName = "Scriptable Objects/Player Skill/Player Position AOE Skill")]
public class PlayerPositionAOESkill : AttackSkill
{
    [Header("Player Position AOE Skill")]
    public float attackAreaRadius = 1f;

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);

        Vector3 damagePos = _player.transform.position;
        Collider[] hits = Physics.OverlapSphere(damagePos, attackAreaRadius, bossLayerMask);
        // 충돌이 검출됐을 경우
        foreach (Collider hitCollider in hits)
        {
            _player.AttackManager.AddDamageToBoss(DamageCalculate(_player), aggro);
        }
    }

}

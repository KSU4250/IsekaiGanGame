using UnityEngine;

/// <summary>
/// 근접 공격 스킬을 정의하는 스크립터블 데이터
/// </summary>
[CreateAssetMenu(fileName = "MeleeSkill", menuName = "Scriptable Objects/Player Skill/Melee")]
public class MeleeSkill : AttackSkill
{
    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);

        _player.AttackManager.EnableMeleeAttack(damage, aggro);
        // _player.AttackManager.AddDamageToBoss(DamageCalculate(_player), aggro);
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);

        _player.AttackManager.DisableMeleeAttack();
    }
}

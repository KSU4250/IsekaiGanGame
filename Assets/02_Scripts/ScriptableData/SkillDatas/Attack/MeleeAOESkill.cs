using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAOESkill", menuName = "Scriptable Objects/Player Skill/MeleeAOESkill")]
public class MeleeAOESkill : AttackSkill
{
    [Header("Melee AOE Skill")]
    public float attackAreaRadius = 1f;
    public float duration = 2f;
    public float damageTickTime = 0.5f;

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);
        _player.StartCoroutine(TickDamageCoroutine(_player));
    }

    private IEnumerator TickDamageCoroutine(PlayerManager _player)
    {
        float currentTime = 0f;
        WaitForSeconds waitSec = new WaitForSeconds(damageTickTime);
        Vector3 damagePos = _player.AttackManager.GetMeleeWeaponPostion();
        while (currentTime <= duration)
        {
            Collider[] hits = Physics.OverlapSphere(damagePos, attackAreaRadius, bossLayerMask);
            // 충돌이 검출됐을 경우
            foreach (Collider hitCollider in hits)
            {
                _player.AttackManager.AddDamageToBoss(DamageCalculate(_player), aggro);
            }

            yield return waitSec;

            currentTime += damageTickTime;
        }
    }
}

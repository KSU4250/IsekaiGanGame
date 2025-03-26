using UnityEngine;

/// <summary>
/// 발사체를 발사하는 스킬을 정의하기 위한 스크립터블 오브젝트
/// </summary>
[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "Scriptable Objects/Player Skill/Projectile")]
public class ProjectileSkill : AttackSkill
{
    [Header("Projectile Skill")]
    public float projectileDuration = 3f;
    public float projectileSpeed = 1f;

    public override void UseSkill(PlayerManager _player)
    {
        // 발사체 스폰 후 정면 방향으로 발사.
        base.UseSkill(_player);

        PlayerProjectileManager projectileManager = _player.GetComponent<PlayerProjectileManager>();

        if(projectileManager == null)
        {
            Debug.LogWarning("Projectile Manager is Null!");
            return;
        }

        projectileManager.ShootProjectile(damage, aggro, projectileDuration, projectileSpeed
            , isBackattackEnable, backAttackTimes);
    }
}

using System.Collections;
using UnityEngine;
using EnumTypes;
using Unity.VisualScripting;

public class PlayerAttackManager
{
    private PlayerManager playerManager = null;
    private CharacterController characterController = null;
    private MeleeWeapon meleeWeapon = null;
    private Transform rangeAttackTransform = null;

    public Transform RangeAttackTransform
    {
        get { return rangeAttackTransform; }
    }

    public void Init(PlayerManager _playerManager)
    {
        playerManager = _playerManager;
        characterController = playerManager.GetComponent<CharacterController>();
        meleeWeapon = playerManager.PlayerMeleeWeapon;
        rangeAttackTransform = playerManager.RangeAttackStartTr;
    }

    public void AddDamageToBoss(int _damage, float _aggro)
    {
        GameManager.Instance.DamageToBoss(playerManager, _damage, _aggro);
    }

    public void EnableMeleeAttack(int _damage, float _aggro)
    {
        meleeWeapon.Init(_damage, _aggro);
        meleeWeapon.SetTriggerEnabled(true);
    }

    public void DisableMeleeAttack()
    {
        meleeWeapon.SetTriggerEnabled(false);
    }

    public Vector3 GetMeleeWeaponPostion()
    {
        return meleeWeapon.transform.position;
    }

    public bool IsMeleeWeaponSet()
    {
        return meleeWeapon != null;
    }

    /// <summary>
    /// 플레이어가 데미지를 받음.
    /// </summary>
    /// <param name="_damage"></param>
    public void TakeDamage(int _damage)
    {
        playerManager.StatusManager.OnDamaged(_damage);
    }

    /// <summary>
    /// 플레이어에게 넉백 효과를 부여함.
    /// </summary>
    /// <param name="_attackOriginPos"></param>
    /// <param name="_distance"></param>
    public void KnockbackPlayer(Vector3 _attackOriginPos, float _distance)
    {
        // playerManager.ChangeState(PlayerStateType.Damaged);

        playerManager.AnimationManager.PlayDamagedAnimation();

        if (characterController.enabled)
        {
            playerManager.StartCoroutine(KnockBackCoroutine(_attackOriginPos, _distance));
        }
    }

    /// <summary>
    /// 플레이어가 현재 보스의 뒤에 위치하고 있는지 여부를 리턴함.
    /// </summary>
    /// <returns></returns>
    public bool IsPlayerBehindBoss()
    {
        if (Vector3.Angle(
            playerManager.transform.forward, 
            GameManager.Instance.GetBossTransform().forward) < 80f)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 플레이어 넉백 시 밀려나는 효과를 주는 코루틴.
    /// </summary>
    /// <param name="_attackOriginPos"></param>
    /// <param name="_distance"></param>
    /// <returns></returns>
    private IEnumerator KnockBackCoroutine(Vector3 _attackOriginPos, float _distance)
    {
        float knockbackTime = 0.5f;
        float currentTime = 0f;
        float speed = _distance / knockbackTime;

        Vector3 direction = playerManager.transform.position - _attackOriginPos;
        direction.y = 0f;
        direction.Normalize();

        Debug.LogFormat("Direction : {0}, speed : {1}", direction, speed);

        while (currentTime <= knockbackTime)
        {
            playerManager.GetComponent<CharacterController>().Move(direction * speed * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
    }
}

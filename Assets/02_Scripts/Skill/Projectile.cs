using System.Collections;
using UnityEngine;

public class Projectile : PoolableObject<Projectile>
{
    [SerializeField]
    private TrailRenderer trailRenderer = null;

    private PlayerManager owner = null;
    private int damage = 1;
    private float aggro = 1f;
    private float duration = 3f;
    private float speed = 2f;
    private bool backAttackEnabled = false;
    private int backAttackTime = 3;

    public void Init(PlayerManager _player, int _damage, float _aggro, 
        bool _backAttackEnabled = false, int _backAttackTime = 3)
    {
        transform.position = _player.RangeAttackStartTr.position;
        transform.rotation = _player.transform.rotation;

        owner = _player;
        damage = _damage;
        aggro = _aggro;
        backAttackEnabled = _backAttackEnabled;
        backAttackTime = _backAttackTime;

        if(trailRenderer != null)
        {
            trailRenderer.Clear();
            // trailRenderer.emitting = true;
        }
    }

    /// <summary>
    /// 발사체를 발사시키는 함수
    /// </summary>
    /// <param name="_duration"></param>
    /// <param name="_speed"></param>
    public void ShootProjectile(float _duration, float _speed)
    {
        duration = _duration;
        speed = _speed;
        StartCoroutine(ShootCoroutine());
    }

    private void OnTriggerEnter(Collider other)
    {
        StopAllCoroutines();

        if (backAttackEnabled && IsHitBehindBoss(other.transform))
        {
            GameManager.Instance.DamageToBoss(owner, damage * backAttackTime, aggro);
        }
        else
        {
            GameManager.Instance.DamageToBoss(owner, damage, aggro);
        }

        ReturnToPool();
    }

    private IEnumerator ShootCoroutine()
    {
        float currentTime = 0f;

        Vector3 direction = owner.transform.forward;

        while(currentTime <= duration)
        {
            transform.position += (direction * speed * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        ReturnToPool();
    }

    public bool IsHitBehindBoss(Transform _targetTr)
    {
        if (Vector3.Angle(
            transform.forward,
            _targetTr.forward) < 80f)
        {
            return true;
        }

        return false;
    }
}

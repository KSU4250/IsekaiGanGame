using UnityEngine;

public class PlayerProjectileManager : MonoBehaviour
{
    [SerializeField]
    private Projectile projectilePrefab = null;

    private ObjectPoolManager<Projectile> projectilePoolManager = null;
    private PlayerManager playerManager = null;

    public void ShootProjectile(int _damage, float _aggro, float _duration, float _speed, 
        bool _backAttackEnabled = false, int _backAttackTime = 3)
    {
        if(projectilePoolManager == null)
        {
            Debug.LogWarning("Projectile Pool Manger is Null!");

            return;
        }

        Projectile projectile = projectilePoolManager.Get();
        projectile.Init(playerManager, _damage, _aggro);
        projectile.ShootProjectile(_duration, _speed);
    }

    private void Awake()
    {
        if (projectilePrefab != null)
        {
            projectilePoolManager = new ObjectPoolManager<Projectile>(projectilePrefab);
        }

        playerManager = GetComponent<PlayerManager>();
    }


}

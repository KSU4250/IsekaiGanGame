using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField]
    PlayerManager playerManager = null;

    [HideInInspector]
    public int maxHitCount = 1;
    [HideInInspector]
    public int damage = 0;
    [HideInInspector]
    public float aggro = 0f;

    private int currentHitCount = 0;

    private SphereCollider attackTrigger = null;

    public void Init(int _damage, float _aggro)
    {
        currentHitCount = 0;
        damage = _damage;
        aggro = _aggro;
    }

    public void SetTriggerEnabled(bool _enable)
    {
        attackTrigger.enabled = _enable;
    }

    private void Awake()
    {
        attackTrigger = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(currentHitCount <= maxHitCount)
        {
            ++currentHitCount;

            playerManager.AttackManager.AddDamageToBoss(damage, aggro);
        }
    }
}

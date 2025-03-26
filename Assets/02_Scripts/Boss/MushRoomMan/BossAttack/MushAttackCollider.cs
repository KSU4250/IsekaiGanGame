using Unity.Netcode;
using UnityEngine;

public class MushAttackCollider : MonoBehaviour
{
    private int damage = 0;
    private string skillName = string.Empty;
    private float knockBackDistance = 0f;

    public int Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    public string SkillName
    {
        get { return skillName; }
        set { skillName = value; }
    }

    public float KnockBackDistance
    {
        get { return knockBackDistance; }
        set { knockBackDistance = value; }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (damage == 0) return;

        if (other.gameObject.tag == "Player" && gameObject.tag == "BossAttack" && other.gameObject.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            // 플레이어 데미지 입도록 설정
            GameManager.Instance.DamageToPlayer(other.gameObject.GetComponent<PlayerManager>(), damage, transform.position, KnockBackDistance);
        }
    }
}

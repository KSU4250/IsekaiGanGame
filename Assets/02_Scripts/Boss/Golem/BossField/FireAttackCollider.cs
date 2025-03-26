using Unity.Netcode;
using UnityEngine;

public class FireAttackCollider : MonoBehaviour
{
    [SerializeField] private int damage = 30;
    [SerializeField] private float TickTime = 1f;

    private float stayTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && gameObject.tag == "Fire" && other.gameObject.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            // 플레이어 데미지 입도록 설정
            GameManager.Instance.DamageToPlayer(other.gameObject.GetComponent<PlayerManager>(), damage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && gameObject.tag == "Fire" && other.gameObject.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            stayTime += Time.deltaTime;

            if (stayTime > TickTime)
            {
                // 플레이어 데미지 입도록 설정
                GameManager.Instance.DamageToPlayer(other.gameObject.GetComponent<PlayerManager>(), damage);
                stayTime = 0f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        stayTime = 0f;
    }
}

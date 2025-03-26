using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class RockDestroy : NetworkBehaviour
{
    private NavMeshObstacle nav;
    private Collider col;
    private bool donDestory = true;
    private ParticleManager particleManager;

    private void Awake()
    {
        nav = GetComponent<NavMeshObstacle>();
        col = GetComponent<Collider>();
        particleManager = FindAnyObjectByType<ParticleManager>();
    }

    public override void OnNetworkSpawn()
    {
        Invoke("EnalbedNav", 1.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BossAttack")
        {
            string skill = other.GetComponent<BossAttackCollider>().SkillName;

            if (!IsServer) return;

            if (skill == "Attack5" || skill == "SpecialAttack" || skill == "Attack6" || skill == "Attack7" || skill == "Attack8")
            {
                Debug.Log("락 부서짐 호출");

                if (skill == "Attack6" && donDestory)
                {
                    donDestory = false;
                    return;
                }

                ParticleClientRpc();
                transform.GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }

    private void EnalbedNav()
    {
        nav.enabled = true;
        col.enabled = true;
    }

    [ClientRpc]
    private void ParticleClientRpc()
    {
        particleManager.PlayParticle(particleManager.attack2, transform.position);
    }
}

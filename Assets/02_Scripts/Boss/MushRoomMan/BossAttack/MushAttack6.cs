using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MushAttack6 : MonoBehaviour
{
    [SerializeField] private int damage = 0;
    [SerializeField] private float dis = 0f;
    [SerializeField] private float TickTime = 1f;
    private float stayTime = 0f;

    private GameObject boom;
    private GameObject poisionFloor;
    private GameObject poisionSun;

    private ParticleSystem poisionSunParticle;

    private void Awake()
    {
        boom = transform.GetChild(0).gameObject;
        poisionFloor = transform.GetChild(1).gameObject;
        poisionSun = transform.GetChild (2).gameObject;
        poisionSunParticle = poisionSun.GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        poisionSun.SetActive(true);

        StartCoroutine(ChangePoisionSunColor());
    }

    // 색이 초록 -> 검정으로 바꾸는 코루틴
    private IEnumerator ChangePoisionSunColor()
    {
        float elapseTime = 0f;
        Color startColor = Color.green;
        Color endColor = Color.black;

        while (true)
        {
            elapseTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapseTime / 10f);

            if (poisionSunParticle != null)
            {
                Color lerpedColor = Color.Lerp(startColor, endColor, t);
                var main = poisionSunParticle.main;
                main.startColor = lerpedColor;
            }

            if (elapseTime >= 10f)
            {
                break;
            }

            yield return null;
        }


        transform.SetParent(null);

        yield return new WaitForSeconds(0.5f);

        poisionFloor.SetActive(true);
        boom.SetActive(true);
        GetComponent<MeshCollider>().enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && gameObject.tag == "BossAttack" && other.gameObject.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            // 플레이어 데미지 입도록 설정
            GameManager.Instance.DamageToPlayer(other.gameObject.GetComponent<PlayerManager>(), damage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && gameObject.tag == "BossAttack" && other.gameObject.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
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

using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class CreateFire : MonoBehaviour
{
    public GameObject firePrefab; // fire 프리팹
    public int fireCount = 10; // 생성할 개수
    public float spawnRangeX = 17.5f; // 생성 범위
    public float spawnRangeZ = 15f;
    public float spawnTime = 30f; // 30초마다 실행

    private void Start()
    {
        StartCoroutine(SpawnFiresRoutine()); // 코루틴 시작
    }

    private IEnumerator SpawnFiresRoutine()
    {
        while (true) // 무한 루프
        {
            SpawnFires();
            yield return new WaitForSeconds(spawnTime); // 30초 대기
        }
    }

    private void SpawnFires()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            for (int i = 0; i < fireCount; i++)
            {
                // XZ 평면에서 랜덤한 위치 지정
                float randomX = Random.Range(-spawnRangeX, spawnRangeX);
                float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);
                Vector3 spawnPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

                // fire 프리팹 생성
                GameObject fire = Instantiate(firePrefab, spawnPosition, Quaternion.Euler(270, 0, 0));

                fire.GetComponent<NetworkObject>().Spawn(true);
            }
        }
    }
}

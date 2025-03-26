using UnityEngine;

public class TestBackRock : MonoBehaviour
{
    public Transform boss; // 보스 위치
    public GameObject player;

    void Update()
    {
        Vector3 bossDir = (boss.position - transform.position).normalized; // 돌 → 보스 방향
        Vector3 playerDir = (player.transform.position - transform.position).normalized;

        // 돌의 앞뒤 판별 (보스 방향과 같은 방향인지 확인)
        float dot = Vector3.Dot(bossDir, playerDir);

        if (dot >= 0)
        {
            Debug.Log(player.name + "플레이어는 돌앞쪽");
        }
        else
        {
            Debug.Log(player.name + "플레이어는 돌뒤쪽");
        }


    }
}

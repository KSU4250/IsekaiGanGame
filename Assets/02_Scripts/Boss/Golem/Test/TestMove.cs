using UnityEngine;

public class TestMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // 방향키 입력 받기
        float horizontal = Input.GetAxis("Horizontal"); // A, D 또는 좌우 화살표
        float vertical = Input.GetAxis("Vertical"); // W, S 또는 상하 화살표

        // 이동 방향 설정
        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;

        // Transform을 이용해 플레이어 이동
        transform.Translate(movement);
    }
}

using UnityEngine;

public class TestBack : MonoBehaviour
{
    public Transform player;
    public Transform boss;

    private void Update()
    {
        CheckBehindObject(boss, player);
    }

    void CheckBehindObject(Transform _bossTr, Transform _playerTr)
    {
        Vector3 toPlayer = _playerTr.position - _bossTr.position;

        Debug.Log(Vector3.Dot(toPlayer.normalized, -_bossTr.forward));
    }
}

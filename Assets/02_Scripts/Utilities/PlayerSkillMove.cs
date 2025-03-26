using System.Collections;
using UnityEngine;

public class PlayerSkillMove
{
    private float moveSpeed = 1f;
    private float moveTime = 1f;
    private Vector3 moveDirection = Vector3.zero;

    public Coroutine StartPlayerMove(PlayerManager _player, float _dist, float _time, Vector3 _direction)
    {
        moveSpeed = _dist;
        moveTime = _time;
        moveDirection = _direction;

        return _player.StartCoroutine(MoveCoroutine(_player));
    }

    private IEnumerator MoveCoroutine(PlayerManager _player)
    {
        float currentTime = 0f;
        CharacterController characterCont = _player.GetComponent<CharacterController>();

        while (currentTime <= moveTime)
        {
            characterCont.Move(moveDirection * moveSpeed * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
    }
}

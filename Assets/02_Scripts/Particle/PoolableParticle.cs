using System.Collections;
using UnityEngine;

public class PoolableParticle : PoolableObject<PoolableParticle>
{
    public void Init(Vector3 _spawnPos, Quaternion _spawnRot, float _autoReturnTime)
    {
        transform.position = _spawnPos;
        transform.rotation = _spawnRot;
        StartCoroutine(ReturnToPoolCoroutine(_autoReturnTime));
    }

    /// <summary>
    /// 즉시 오브젝트 풀에 파티클을 반환함.
    /// </summary>
    public void InstantReturnToPool()
    {
        StopAllCoroutines();
        ReturnToPool();
    }

    /// <summary>
    /// 지정된 시간 후 오브젝트 풀에 파티클을 반환함.
    /// </summary>
    /// <param name="_returnTime"></param>
    /// <returns></returns>
    private IEnumerator ReturnToPoolCoroutine(float _returnTime)
    {
        yield return new WaitForSeconds(_returnTime);

        ReturnToPool();
    }
}

using UnityEngine;
using UnityEngine.Pool;

public abstract class PoolableObject<T> : MonoBehaviour where T : MonoBehaviour
{
    private ObjectPool<T> _pool;


    public void SetPool(ObjectPool<T> pool)
    {
        _pool = pool;
    }

    
    protected virtual void OnEnable()
    {
    }

    /// <summary>
    /// 오브젝트 풀로 오브젝트를 반환하는 함수.
    /// </summary>
    public void ReturnToPool()
    {
        _pool.Release(this as T);
    }
}

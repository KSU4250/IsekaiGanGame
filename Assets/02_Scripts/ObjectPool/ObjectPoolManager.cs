using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager<T> where T : PoolableObject<T>
{
    private ObjectPool<T> pool;
    private T prefab;
    private Transform parent = null;

    public ObjectPoolManager(T _prefab, int _defaultCapacity = 10, int _maxSize = 20, Transform _parent = null)
    {
        prefab = _prefab;
        parent = _parent;
        pool = new ObjectPool<T>(
            createFunc: CreateObject,
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: obj => Object.Destroy(obj.gameObject),
            collectionCheck: false,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );
    }

    private T CreateObject()
    {
        T obj = Object.Instantiate(prefab);

        if (parent != null)
        {
            obj.transform.SetParent(parent);
        }

        return obj;
    }

    public T Get()
    {
        T obj = pool.Get();
        obj.SetPool(pool);
        return obj;
    }
}

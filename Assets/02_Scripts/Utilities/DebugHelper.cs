using UnityEngine;

public class DebugHelper : MonoBehaviour
{
    [SerializeField]
    private GameObject debugSpherePrefab = null;

    public void SpawnDebugSphere(Vector3 _spawnPos, float _radius)
    {
        GameObject spawnedObj = Instantiate(debugSpherePrefab, _spawnPos, Quaternion.identity);
    }
}

using UnityEngine;

public class ParticleLifetime : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 2f;

    public void DestroyAfterLifetime()
    {
        Destroy(gameObject, lifeTime);
    }
}

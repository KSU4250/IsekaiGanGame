using System.Collections;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    public ParticleSystem attack1;
    public ParticleSystem attack2;
    public ParticleSystem attack3;
    public ParticleSystem attack4;
    public ParticleSystem attack5;
    public ParticleSystem attack6;
    public ParticleSystem attack7;
    public ParticleSystem attack8;
    public ParticleSystem attack9;
    public ParticleSystem hitParticle;

    public ParticleSystem SpecialAttack;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayParticle(ParticleSystem particle, Vector3 position)
    {
        if (particle != null)
        {
            ParticleSystem newParticle = Instantiate(particle, position, Quaternion.identity);
            newParticle.Play();
            Destroy(newParticle.gameObject, newParticle.main.duration + newParticle.main.startLifetime.constantMax); // 파티클 자동 삭제
        }
    }

    public void PlayParticle(ParticleSystem particle, Vector3 position, Quaternion _rotation)
    {
        if (particle != null)
        {
            ParticleSystem newParticle = Instantiate(particle, position, _rotation);
            newParticle.Play();
            Destroy(newParticle.gameObject, newParticle.main.duration + newParticle.main.startLifetime.constantMax); // 파티클 자동 삭제
        }
    }

    public void PlayParticleSetParent(ParticleSystem particle, Vector3 position, GameObject _parents, float _duration)
    {
        if (particle != null)
        {
            ParticleSystem newParticle = Instantiate(particle, position, Quaternion.identity);
            newParticle.gameObject.transform.SetParent(_parents.transform);
            newParticle.Play();

            StartCoroutine(DestoryParticle(newParticle, _duration));
        }
    }

    private IEnumerator DestoryParticle(ParticleSystem _particle, float _duration)
    {
        yield return new WaitForSeconds(_duration);

        Destroy(_particle.gameObject);
    }
}


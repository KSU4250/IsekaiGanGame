using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class DamageParticle : MonoBehaviour
{
    public ParticleSystem P_Particle;
    public ParticleSystem P_ParticleMine;
    public BossStateManager bossStateManager;
    public ParticleSystem[] digitParticles;
    public List<Sprite> numberSprites;
    public float damage;

    private Dictionary<int, Sprite> spriteDict;
    private ParticleSystemRenderer[] particleRenderers;

    void Awake()
    {
        // 숫자별 스프라이트 매핑
        spriteDict = new Dictionary<int, Sprite>();
        for (int i = 0; i < numberSprites.Count; i++)
        {
            spriteDict[i] = numberSprites[i];
        }
    }

    // 데미지에 맞는 파티클 실행
    public void SetupAndPlayParticles(float _damage)
    {
        // 파티클 시스템 생성
        ParticleSystem particle = Instantiate(P_Particle, new Vector3(bossStateManager.Boss.transform.position.x, bossStateManager.Boss.transform.position.y + 1f, bossStateManager.Boss.transform.position.z), Quaternion.identity);

        // 파티클 시스템 자식들 가져옴.
        digitParticles = particle.GetComponentsInChildren<ParticleSystem>().Where(ps => ps != particle).ToArray();

        // 각 파티클 시스템의 Renderer 저장
        particleRenderers = new ParticleSystemRenderer[digitParticles.Length];
        for (int i = 0; i < digitParticles.Length; i++)
        {
            particleRenderers[i] = digitParticles[i].GetComponent<ParticleSystemRenderer>();
        }

        // 데미지에 맞게 스프라이트 설정
        SetDamage(_damage);

        // 파티클 재생
        particle.Play();

        Destroy(particle.gameObject, particle.main.duration + particle.main.startLifetime.constantMax);
    }

    // 데미지에 맞는 파티클 실행 (내)
    public void SetupAndPlayParticlesMine(float _damage)
    {
        // 파티클 시스템 생성
        ParticleSystem particle = Instantiate(P_ParticleMine, new Vector3(bossStateManager.Boss.transform.position.x, bossStateManager.Boss.transform.position.y + 1f, bossStateManager.Boss.transform.position.z), Quaternion.identity);

        // 파티클 시스템 자식들 가져옴.
        digitParticles = particle.GetComponentsInChildren<ParticleSystem>().Where(ps => ps != particle).ToArray();

        // 각 파티클 시스템의 Renderer 저장
        particleRenderers = new ParticleSystemRenderer[digitParticles.Length];
        for (int i = 0; i < digitParticles.Length; i++)
        {
            particleRenderers[i] = digitParticles[i].GetComponent<ParticleSystemRenderer>();
        }

        // 데미지에 맞게 스프라이트 설정
        SetDamage(_damage);

        // 파티클 재생
        particle.Play();

        Destroy(particle.gameObject, particle.main.duration + particle.main.startLifetime.constantMax);
    }

    // 데미지에 맞게 스프라이트 설정
    private void SetDamage(float damage)
    {
        string damageStr = damage.ToString();
        int length = damageStr.Length;

        // 각 자리수에 맞게 파티클 시스템 설정
        for (int i = 0; i < digitParticles.Length; i++)
        {
            if (i < length)
            {
                int digit = int.Parse(damageStr[length - 1 - i].ToString());
                var textureSheet = digitParticles[i].textureSheetAnimation;
                textureSheet.SetSprite(0, spriteDict[digit]);

                particleRenderers[i].enabled = true;
            }
            else
            {
                particleRenderers[i].enabled = false;
            }
        }
    }

}

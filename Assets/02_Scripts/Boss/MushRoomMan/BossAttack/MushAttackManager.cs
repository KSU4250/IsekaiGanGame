using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MushAttackManager : NetworkBehaviour
{
    [Header("Attack1 - 브레스")]
    [SerializeField] private GameObject bressDecal;
    [SerializeField] private GameObject bress;

    [Header("Attack2 - 독구름")]
    [SerializeField] private GameObject P_PoisonCloud;

    [Header("Attack3 - 길다란 독장판")]
    [SerializeField] private GameObject attack3DecalPos;
    [SerializeField] private DecalProjector attack3Decal;
    [SerializeField] private GameObject P_PoisionRay;

    [Header("Attack4 - 여러번 Jump공격")]
    [SerializeField] private GameObject attckJumpDecalPos;
    [SerializeField] private DecalProjector attckJumpFullDecal;
    [SerializeField] private DecalProjector attckJumpChargeDecal;
    [SerializeField] private GameObject P_AttackJump;

    [Header("Attack6 - 똥뿌리기")]
    [SerializeField] private GameObject P_Attack6;

    // 참조 할것들
    private Animator anim;
    private BossSkillManager mushSkillManager;
    private MushStateManager mushStateManager;

    // 스킬 관련
    private BossSkillData skill;
    private string skillName;
    private float range;
    private int damage;
    private float delay;
    private float duration;
    private float knockBackDis;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        mushSkillManager = GetComponent<BossSkillManager>();
        mushStateManager = GetComponent<MushStateManager>();
    }

    private void PerformAttack(string _state)
    {
        switch (_state)
        {
            case "Attack1":
                StartCoroutine(Attack1());
                break;
            case "Attack1-1":
                StartCoroutine(Attack1_1());
                break;
            case "Attack2":
                StartCoroutine(Attack2());
                break;
            case "Attack3_1":
                StartCoroutine(Attack3_1());
                break;
            case "Attack3_2":
                StartCoroutine(Attack3_2());
                break;
            case "Attack4":
                StartCoroutine(Attack4());
                break;
            case "Attack5":
                StartCoroutine(Attack5());
                break;
            case "Attack6":
                StartCoroutine(Attack6());
                break;
            default:
                break;
        }
    }

    #region [Attack]

    // 브레스
    private IEnumerator Attack1()
    {
        // 스킬 정보를 브레스에 넣는 코드
        skill = mushSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack1").SkillData;

        skillName = skill.SkillName;
        damage = skill.Damage;
        knockBackDis = skill.KnockbackDistance;

        MushAttackCollider attack1 = bress.GetComponent<MushAttackCollider>();

        attack1.SkillName = skillName;
        attack1.Damage = damage;
        attack1.KnockBackDistance = knockBackDis;

        // 데칼 키고 위치변경
        bressDecal.transform.position = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 1f + mushStateManager.Boss.transform.right * 1f;
        bressDecal.transform.rotation = mushStateManager.Boss.transform.localRotation;
        bressDecal.SetActive(true);

        // 애니메이션이 끝났다면
        while (true)
        {
            if (CheckEndAnim("Attack1"))
            {
                break;
            }

            yield return null;
        }

        bressDecal.SetActive(false);
    }

    private IEnumerator Attack1_1()
    {
        // 위치 설정 및 자식으로
        bress.transform.position = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 2f;
        bress.transform.rotation = mushStateManager.Boss.transform.rotation;
        bress.transform.SetParent(mushStateManager.Boss.transform);
        bress.SetActive(true);

        // 오디오 재생
        MushAudioManager.Instance.AudioPlay(MushAudioManager.Instance.Attack1);

        // 애니메이션이 끝났다면
        while (true)
        {
            if (CheckEndAnim("Attack1-1"))
            {
                break;
            }

            yield return null;
        }

        bress.SetActive(false);

    }

    // 독구름
    private IEnumerator Attack2()
    {
        // 스킬 정보를 독구름에 넣는 코드
        skill = mushSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack2").SkillData;

        skillName = skill.SkillName;
        damage = skill.Damage;
        knockBackDis = skill.KnockbackDistance;

        GameObject attack2Object;
        Vector3 pos = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 1f;

        attack2Object = Instantiate(P_PoisonCloud, new Vector3(pos.x, 0f, pos.z), Quaternion.identity, null);

        MushAttackCollider attack2 = attack2Object.GetComponent<MushAttackCollider>();

        attack2.SkillName = skillName;
        attack2.Damage = damage;
        attack2.KnockBackDistance = knockBackDis;

        yield return null;
    }

    // 길다란 독장판
    private IEnumerator Attack3_1()
    {
        // 네모 데칼 설정
        attack3DecalPos.transform.position = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 2f;
        attack3DecalPos.transform.rotation = mushStateManager.Boss.transform.localRotation;
        attack3Decal.size = new Vector3(0f, 2f, 100f);
        attack3DecalPos.transform.SetParent(mushStateManager.Boss.transform);
        attack3DecalPos.SetActive(true);
        StartCoroutine(Attack3DecalCoroutine(0.5f));

        yield return null;
    }
    private IEnumerator Attack3_2()
    {
        // 데칼 안보이게 설정
        attack3DecalPos.SetActive(false);

        // 일자로 버섯 포자 공격 서버에서 생성
        if (IsServer)
        {
            Vector3 pos = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 2f;
            Quaternion rot = mushStateManager.Boss.transform.localRotation;
            GameObject attack3Object = Instantiate(P_PoisionRay, pos, rot, null);
            attack3Object.GetComponent<NetworkObject>().Spawn(true);

            // 스킬 정보를 길다란 독장판에 넣는 코드
            skill = mushSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack3").SkillData;

            skillName = skill.SkillName;
            damage = skill.Damage;
            knockBackDis = skill.KnockbackDistance;

            MushAttackCollider attack3 = attack3Object.GetComponent<MushAttackCollider>();

            attack3.SkillName = skillName;
            attack3.Damage = damage;
            attack3.KnockBackDistance = knockBackDis;
        }

        // 오디오 재생
        MushAudioManager.Instance.AudioPlay(MushAudioManager.Instance.Attack3);

        yield return null;
    }

    // 미친 점프
    private IEnumerator Attack4()
    {
        float spd = 1f;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4"))
        {
            spd = 1f;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4-1"))
        {
            spd = 1.5f;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4-2"))
        {
            spd = 2f;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4-3"))
        {
            spd = 2.5f;
        }

        // 스킬설정
        skill = mushSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack4").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / spd;
        knockBackDis = skill.KnockbackDistance;

        // 스킬위치 조정
        attckJumpDecalPos.transform.position = new Vector3(mushStateManager.RandomPlayer.transform.position.x, 0.3f, mushStateManager.RandomPlayer.transform.position.z);

        // 스킬 보이게 스킬범위데칼 설정
        attckJumpFullDecal.size = new Vector3(range, range, 1f);

        float elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;

            attckJumpChargeDecal.size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            if (elapseTime >= delay)
            {
                break;
            }

            yield return null;
        }

        // 데칼 범위 초기화
        attckJumpChargeDecal.size = Vector3.zero;
        attckJumpFullDecal.size = Vector3.zero;

        // 프리펩 생성
        GameObject attack4Object = Instantiate(P_AttackJump, attckJumpDecalPos.transform.position, Quaternion.identity, null);

        // 오디오 재생
        MushAudioManager.Instance.AudioPlay(MushAudioManager.Instance.Attack4);

        MushAttackCollider attack4 = attack4Object.GetComponent<MushAttackCollider>();

        attack4.SkillName = skillName;
        attack4.Damage = damage;
        attack4.KnockBackDistance = knockBackDis;

        yield return null;
    }

    // 그냥 점프
    private IEnumerator Attack5()
    {
        // 스킬설정
        skill = mushSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack5").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay;
        knockBackDis = skill.KnockbackDistance;

        // 스킬위치 조정
        attckJumpDecalPos.transform.position = new Vector3(mushStateManager.AggroPlayer.transform.position.x, 0.3f, mushStateManager.AggroPlayer.transform.position.z);

        // 스킬 보이게 스킬범위데칼 설정
        attckJumpFullDecal.size = new Vector3(range, range, 1f);

        float elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;

            attckJumpChargeDecal.size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            if (elapseTime >= delay)
            {
                break;
            }

            yield return null;
        }

        // 데칼 범위 초기화
        attckJumpChargeDecal.size = Vector3.zero;
        attckJumpFullDecal.size = Vector3.zero;

        // 프리펩 생성
        GameObject attack5Object = Instantiate(P_AttackJump, attckJumpDecalPos.transform.position, Quaternion.identity, null);

        // 오디오 재생
        MushAudioManager.Instance.AudioPlay(MushAudioManager.Instance.Attack5);

        // 스킬 데미지 설정
        MushAttackCollider attack5 = attack5Object.GetComponent<MushAttackCollider>();
        attack5.SkillName = skillName;
        attack5.Damage = damage;
        attack5.KnockBackDistance = knockBackDis;

        yield return null;
    }

    // 똥뿌리기
    private IEnumerator Attack6()
    {
        for (int i = 0; i < 4; i++)
        {
            if (mushStateManager.AlivePlayers[i] == null) continue;

            GameObject attack6Object = Instantiate(P_Attack6, mushStateManager.AlivePlayers[i].transform);
        }

        yield return null;
    }
    #endregion

    #region [Function]

    // 애니메이션 끝났는지 확인
    private bool CheckEndAnim(string _animName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(_animName) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Attack3 데칼 찍는 코루틴
    private IEnumerator Attack3DecalCoroutine(float _delayTime)
    {
        float elapseTime = 0f;
        Vector3 startSize = new Vector3(0f, 3f, 100f);
        Vector3 targetSize = new Vector3(3f, 3f, 100f);

        while (true)
        {
            elapseTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapseTime / _delayTime);
            attack3Decal.size = Vector3.Lerp(startSize, targetSize, t);

            if (elapseTime >= _delayTime)
            {
                break;
            }

            yield return null;
        }
    }

    #endregion
}
